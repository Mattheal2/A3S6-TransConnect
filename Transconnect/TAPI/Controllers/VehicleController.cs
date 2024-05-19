using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransLib;
using TransLib.Vehicles;
using TransLib.Auth;
using TAPI.Auth;
using TransLib.Persons;
using ZstdSharp;
using System.Runtime.CompilerServices;

namespace TAPI.Controllers
{

    [Route("api/[controller]/[action]")]
    public class VehicleController : Controller
    {
        private class VehicleResponse
        {
            public Car? car { get; }
            public Van? van { get; }
            public Truck? truck { get; }

            public static Vehicle Parse(CreateVehicleRequest body)
            {
                switch (body.type.ToLower())
                {
                    case "car":
                        if (!body.seats.HasValue) throw new Exception("Missing required field 'seats' for 'Car'");
                        return new Car(body.license_plate, body.brand, body.model, body.price, body.seats.Value);
                    case "van":
                        if (body.usage == null) throw new Exception("Missing required field 'usage' for 'Van'");
                        return new Van(body.license_plate, body.brand, body.model, body.price, body.usage);
                    case "truck":
                        if (!body.volume.HasValue) throw new Exception("Missing required field 'volume' for 'Truck'");
                        if (body.truck_type == null) throw new Exception("Missing required field 'truck_type' for 'Truck'");
                        return new Truck(body.license_plate, body.brand, body.model, body.price, body.volume.Value, body.truck_type);
                    default:
                        throw new Exception("Tried to parse an invalid vehicle type");
                }
            }
        }
        public struct CreateVehicleRequest
        {
            public string type { get; set; }
            public string license_plate { get; set; }
            public string brand { get; set; }
            public string model { get; set; }
            public int price { get; set; }

            public int? seats { get; set; } // for Car
            public string? usage { get; set; } // for Van
            public int? volume { get; set; } // for Truck
            public string? truck_type { get; set; } // for Truck

        }

        [HttpPost(Name = "AddVehicle")]
        public async Task<ApiResponse<Vehicle>> AddVehicle([FromBody] CreateVehicleRequest body)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Vehicle>();

            Vehicle new_vehicle = VehicleResponse.Parse(body);
            await new_vehicle.create(Config.cfg);

            return ApiResponse<Vehicle>.Success(new_vehicle);
        }

        [HttpGet(Name = "ListVehicles")]
        public async Task<ApiResponse<Vehicle[]>> ListVehicles([FromQuery] string filter = "", [FromQuery] int limit = 20, [FromQuery] int offset = 0, [FromQuery] string order_field = "brand", [FromQuery] string order_dir = "ASC")
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Vehicle[]>();

            filter = filter.ToLower();
            if (filter != null && filter != "" && filter != "car" && filter != "van" && filter != "truck")
            {
                return ApiResponse<Vehicle[]>.Failure(400, "vehicle.invalid_filter", "Invalid filter");
            }

            if (order_field != "brand" && order_field != "model" && order_field != "price")
            {
                return ApiResponse<Vehicle[]>.Failure(400, "vehicle.invalid_order_field", "Invalid order field");
            }

            if (order_dir != "ASC" && order_dir != "DESC")
            {
                return ApiResponse<Vehicle[]>.Failure(400, "vehicle.invalid_order_dir", "Invalid order direction");
            }

            if (limit < 0)
            {
                return ApiResponse<Vehicle[]>.Failure(400, "vehicle.invalid_limit", "Invalid limit");
            }

            if (offset < 0)
            {
                return ApiResponse<Vehicle[]>.Failure(400, "vehicle.invalid_offset", "Invalid offset");
            }

            var vehicles = (await Vehicle.list_vehicles(Config.cfg, "", order_field, order_dir, limit, offset)).ToArray();
            return ApiResponse<Vehicle[]>.Success(vehicles);
        }

        public struct UpdateVehicleRequest
        {
            public string type { get; set; }
            public string license_plate { get; set; }
            public string? brand { get; set; }
            public string? model { get; set; }
            public int? price { get; set; }

            public int? seats { get; set; } // for Car
            public string? usage { get; set; } // for Van
            public int? volume { get; set; } // for Truck
            public string? truck_type { get; set; } // for Truck
        }

        [HttpPost(Name = "UpdateVehicle")]
        public async Task<ApiResponse<Vehicle>> UpdateVehicle([FromBody] CreateVehicleRequest body)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Vehicle>();

            Vehicle? vehicle = await Vehicle.get_vehicle_by_license_plate(Config.cfg, body.license_plate);

            if (vehicle == null) return ApiResponse<Vehicle>.Failure(404, "vehicle.not_found", "Vehicle not found");

            if (body.brand != null)
                await vehicle.set_brand(Config.cfg, body.brand);
            if (body.model != null)
                await vehicle.set_model(Config.cfg, body.model);
            if (body.price != 0)
                await vehicle.set_price(Config.cfg, body.price);
            if (body.type.ToLower() == "car" && body.seats != null && vehicle is Car)
                await ((Car)vehicle).set_seats(Config.cfg, body.seats.Value);
            if (body.type.ToLower() == "van" && body.usage != null && vehicle is Van)
                await ((Van)vehicle).set_usage(Config.cfg, body.usage);
            if (body.type.ToLower() == "truck" && body.volume != null && vehicle is Truck)
                await ((Truck)vehicle).set_volume(Config.cfg, body.volume.Value);
            if (body.type.ToLower() == "truck" && body.truck_type != null && vehicle is Truck)
                await ((Truck)vehicle).set_truck_type(Config.cfg, body.truck_type);

            return ApiResponse<Vehicle>.Success(vehicle);
        }

        [HttpPost(Name = "DeleteVehicle")]
        public async Task<ApiResponse<Vehicle>> DeleteVehicle([FromBody] string license_plate)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Vehicle>();

            Vehicle? vehicle = await Vehicle.get_vehicle_by_license_plate(Config.cfg, license_plate);
            if (vehicle == null) return ApiResponse<Vehicle>.Failure(404, "vehicle.not_found", "Vehicle not found");

            await vehicle.delete(Config.cfg);
            return ApiResponse<Vehicle>.Success(vehicle);
        }

        [HttpGet(Name = "GetVehicleByLicensePlate")]
        public async Task<ApiResponse<Vehicle>> GetVehicleByLicensePlate([FromQuery] string license_plate)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Vehicle>();

            Vehicle? vehicle = await Vehicle.get_vehicle_by_license_plate(Config.cfg, license_plate);
            if (vehicle == null) return ApiResponse<Vehicle>.Failure(404, "vehicle.not_found", "Vehicle not found");

            return ApiResponse<Vehicle>.Success(vehicle);
        }

    }
}
