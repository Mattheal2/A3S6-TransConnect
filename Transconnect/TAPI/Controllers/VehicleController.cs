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

        [HttpPost(Name = "CreateVehicle")]
        public async Task<ApiResponse<Vehicle>> CreateVehicle([FromBody] CreateVehicleRequest body)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Vehicle>();

            Vehicle new_vehicle = VehicleResponse.Parse(body);
            await new_vehicle.create(Config.cfg);

            return ApiResponse<Vehicle>.Success(new_vehicle);
        }

        [HttpGet(Name = "GetVehicles")]
        public async Task<ApiResponse<List<Vehicle>>> GetVehicle([FromQuery] string filter ="", [FromQuery] int limit = 20, [FromQuery] int offset = 0, [FromQuery] string order_field = "brand", [FromQuery] string order_dir = "ASC")
        {
            //Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            //if (!auth.is_employee()) return auth.get_unauthorized_error<List<Vehicle>>();

            filter = filter.ToLower();
            if(filter != null && filter != "" && filter != "car" && filter != "van" && filter != "truck")
            {
                return ApiResponse<List<Vehicle>>.Failure(400, "vehicle.invalid_filter", "Invalid filter");
            }

            if (order_field != "brand" && order_field != "model" && order_field != "price")
            {
                return ApiResponse<List<Vehicle>>.Failure(400, "vehicle.invalid_order_field", "Invalid order field");
            }

            if (order_dir != "ASC" && order_dir != "DESC")
            {
                return ApiResponse<List<Vehicle>>.Failure(400, "vehicle.invalid_order_dir", "Invalid order direction");
            }

            if (limit < 0)
            {
                return ApiResponse<List<Vehicle>>.Failure(400, "vehicle.invalid_limit", "Invalid limit");
            }

            if (offset < 0)
            {
                return ApiResponse<List<Vehicle>>.Failure(400, "vehicle.invalid_offset", "Invalid offset");
            }

            var vehicles = await Vehicle.list_vehicles(Config.cfg, "", order_field, order_dir, limit, offset);
            return ApiResponse<List<Vehicle>>.Success(vehicles);
        }

    }
}
