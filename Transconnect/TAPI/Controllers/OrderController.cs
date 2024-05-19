using Microsoft.AspNetCore.Mvc;
using TAPI.Auth;
using TransLib;
using TransLib.Auth;


namespace TAPI.Controllers
{
    public class OrderController : Controller
    {
        public struct CreateOrderRequest
        {
            public int client_id { get; set; }
            public int driver_id { get; set; }
            public string vehicle_license_plate { get; set; }
            public long departure_time { get; set; }
            public long arrival_date { get; set; }
            public string departure_city { get; set; }
            public string arrival_city { get; set; }
            //public Order.OrderStatus status { get; set; }
        }

        [HttpPost(Name = "CreateOrder")]
        public async Task<ApiResponse<Order>> CreateOrder([FromBody] CreateOrderRequest body)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order>();

            Order new_order = new Order(Config.cfg, body.client_id, body.driver_id, body.vehicle_license_plate, body.departure_time, body.departure_city, body.arrival_city);
            return null;
        }
    }
}
