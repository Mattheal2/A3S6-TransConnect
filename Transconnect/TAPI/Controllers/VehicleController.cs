using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransLib;
using TransLib.Vehicles;
using TAPI.Auth;
using TransLib.Persons;

namespace TAPI.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VehicleController : ControllerBase
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

        /// <summary>
        /// A temporary struct to represent any instance of vehicle as JSON without losing child class information.
        /// </summary>
        public class VehicleResp {
            public Van? van { get; set; }
            public Car? car { get; set; }
            public Truck? truck { get; set; }

            public VehicleResp(Vehicle vehicle) {
                switch (vehicle.vehicle_type)
                {
                    case "CAR":
                        car = (Car)vehicle;
                        break;
                    case "VAN":
                        van = (Van)vehicle;
                        break;
                    case "TRUCK":
                        truck = (Truck)vehicle;
                        break;
                }
            }
        }

        [HttpPost(Name = "AddVehicle")]
        public async Task<ApiResponse<VehicleResp>> AddVehicle([FromBody] CreateVehicleRequest body)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<VehicleResp>();

            Vehicle new_vehicle = VehicleResponse.Parse(body);
            await new_vehicle.create(Config.cfg);

            return ApiResponse<VehicleResp>.Success(new VehicleResp(new_vehicle));
        }

        [HttpGet(Name = "ListVehicles")]
        public async Task<ApiResponse<VehicleResp[]>> ListVehicles([FromQuery] string order_field = "brand", [FromQuery] string order_dir = "ASC")
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<VehicleResp[]>();

            if (order_field != "brand" && order_field != "model" && order_field != "price")
            {
                return ApiResponse<VehicleResp[]>.Failure(400, "vehicle.invalid_order_field", "Invalid order field");
            }

            if (order_dir != "ASC" && order_dir != "DESC")
            {
                return ApiResponse<VehicleResp[]>.Failure(400, "vehicle.invalid_order_dir", "Invalid order direction");
            }

            var vehicles = (await Vehicle.list_vehicles(Config.cfg, "", order_field, order_dir)).Map(vehicle => new VehicleResp(vehicle)).ToArray();
            return ApiResponse<VehicleResp[]>.Success(vehicles);
        }

        public struct UpdateVehicleRequest
        {
            public required string type { get; set; }
            public required string license_plate { get; set; }
            public required string brand { get; set; }
            public required string model { get; set; }
            public required int price { get; set; }

            public int? seats { get; set; } // for Car
            public string? usage { get; set; } // for Van
            public int? volume { get; set; } // for Truck
            public string? truck_type { get; set; } // for Truck
        }

        [HttpPost(Name = "UpdateVehicle")]
        public async Task<ApiResponse<VehicleResp>> UpdateVehicle([FromBody] UpdateVehicleRequest body)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<VehicleResp>();

            Vehicle? vehicle = await Vehicle.get_vehicle_by_license_plate(Config.cfg, body.license_plate);

            if (vehicle == null) return ApiResponse<VehicleResp>.Failure(404, "vehicle.not_found", "Vehicle not found");

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

            return ApiResponse<VehicleResp>.Success(new VehicleResp(vehicle));
        }

        [HttpPost(Name = "DeleteVehicle")]
        public async Task<ApiResponse<VehicleResp>> DeleteVehicle([FromQuery] string license_plate)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<VehicleResp>();

            Vehicle? vehicle = await Vehicle.get_vehicle_by_license_plate(Config.cfg, license_plate);
            if (vehicle == null) return ApiResponse<VehicleResp>.Failure(404, "vehicle.not_found", "Vehicle not found");

            await vehicle.delete(Config.cfg);
            return ApiResponse<VehicleResp>.Success(new VehicleResp(vehicle));
        }

        [HttpGet(Name = "GetVehicleByLicensePlate")]
        public async Task<ApiResponse<VehicleResp>> GetVehicleByLicensePlate([FromQuery] string license_plate)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<VehicleResp>();

            Vehicle? vehicle = await Vehicle.get_vehicle_by_license_plate(Config.cfg, license_plate);
            if (vehicle == null) return ApiResponse<VehicleResp>.Failure(404, "vehicle.not_found", "Vehicle not found");

            return ApiResponse<VehicleResp>.Success(new VehicleResp(vehicle));
        }

    }
}
