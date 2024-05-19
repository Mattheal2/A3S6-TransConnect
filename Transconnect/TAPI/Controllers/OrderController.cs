using Microsoft.AspNetCore.Mvc;
using TAPI.Auth;
using TransLib;
using TransLib.Auth;


namespace TAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    public class OrderController : Controller
    {
        public struct CreateOrderRequest
        {
            public int client_id { get; set; }
            public long departure_time { get; set; }
            public string departure_city { get; set; }
            public string arrival_city { get; set; }
            public string vehicle_type { get; set; }

            public string? truck_type { get; set; } //optionnal for truck
        }

        [HttpPost(Name = "CreateOrder")]
        public async Task<ApiResponse<Order>> CreateOrder([FromBody] CreateOrderRequest body)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order>();

            Order new_order = new Order(body.client_id, body.departure_time, body.departure_city, body.arrival_city);
            await new_order.find_driver(Config.cfg);
            await new_order.find_vehicle(Config.cfg, body.vehicle_type, body.truck_type);

            string? error = new_order.validate();
            if (error != null) return ApiResponse<Order>.Failure(400, "order.invalid_order", error);
            
            await new_order.create(Config.cfg);
            return ApiResponse<Order>.Success(new_order);
        }

        [HttpGet(Name = "GetOrder")]
        public async Task<ApiResponse<Order>> GetOrder([FromQuery] int order_id)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order>();

            Order? order = await Order.get_order(Config.cfg, order_id);
            if (order == null) return ApiResponse<Order>.Failure(404, "order.not_found", "Order not found");

            return ApiResponse<Order>.Success(order);
        }
    }
}
