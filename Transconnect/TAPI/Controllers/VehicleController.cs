using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    public class VehicleController : Controller
    {
        public struct CreateVehicleRequest
        {
            public string license_plate { get; set; }
            public string brand { get; set; }
            public string model { get; set; }
            public int price { get; set; }
        }
    }
}
