using Microsoft.AspNetCore.Mvc;
using TAPI.Auth;
using TransLib;
using TransLib.Auth;
using TransLib.Vehicles;


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

        [HttpGet(Name = "ListOrders")]
        public async Task<ApiResponse<Order[]>> ListOrder([FromQuery] string filter = "", [FromQuery] int limit = 20, [FromQuery] int offset = 0, [FromQuery] string order_field = "departure_time", string order_dir = "DESC")
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order[]>();

            List<Order> orders = await Order.list_orders(Config.cfg, filter, limit, offset, order_field, order_dir);
            string? error = null;
            orders.ForEach(order => error += order.validate());
            if (error != null) return ApiResponse<Order[]>.Failure(400, "order.invalid_order", error);

            return ApiResponse<Order[]>.Success(orders.ToArray());
        }

        public struct OrderUpdateRequest
        {
            public int order_id { get; set; }
            public long? departure_time { get; set; }
            public string? departure_city { get; set; }
            public string? arrival_city { get; set; }
        }

        [HttpPost(Name = "UpdateOrder")]
        public async Task<ApiResponse<Order>> UpdateOrder([FromBody] OrderUpdateRequest body)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order>();
            
            Order? order = await Order.get_order(Config.cfg, body.order_id);
            if (order == null) return ApiResponse<Order>.Failure(404, "order.not_found", "Order not found");

            if (body.departure_time != null) 
                await order.set_departure_time(Config.cfg, body.departure_time.Value);
            if (body.departure_city != null) 
                await order.set_departure_city(Config.cfg, body.departure_city);
            if (body.arrival_city != null)
                await order.set_arrival_city(Config.cfg, body.arrival_city);

            string? error = order.validate();
            if (error != null) return ApiResponse<Order>.Failure(400, "order.invalid_order", error);

            return ApiResponse<Order>.Success(order);
        }

        [HttpPost(Name = "DeleteOrder")]
        public async Task<ApiResponse<Order>> DeleteOrder([FromBody] int order_id)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order>();

            Order? order = await Order.get_order(Config.cfg, order_id);
            if (order == null) return ApiResponse<Order>.Failure(404, "order.not_found", "Order not found");

            await order.delete(Config.cfg);
            return ApiResponse<Order>.Success(order);
        }

        [HttpGet(Name = "ListOrdersByClientId")]
        public async Task<ApiResponse<Order[]>> ListOrdersByClientId([FromQuery] int client_id)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order[]>();

            List<Order> orders = await Order.list_orders_by_client_id(Config.cfg, client_id);
            string? error = null;
            orders.ForEach(order => error += order.validate());
            if (error != null) return ApiResponse<Order[]>.Failure(400, "order.invalid_order", error);

            return ApiResponse<Order[]>.Success(orders.ToArray());
        }
    }
}
