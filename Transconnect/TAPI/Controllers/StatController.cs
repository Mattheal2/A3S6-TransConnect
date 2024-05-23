using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TAPI.Auth;
using TransLib;
using TransLib.Auth;
using TransLib.Persons;
using TransLib.Miscellaneous;

namespace TAPI.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatController : ControllerBase
    {
        public async Task<ApiResponse<Order[]>> GetDeliveriesByDriver([FromQuery] int driver_id)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order[]>();

            return null;
        }

        public async Task<ApiResponse<Order[]>> GetDeliveriesBetween([FromQuery] long start, [FromQuery] long end)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order[]>();

            return null;
        }

        public async Task<ApiResponse<int>> GetAverageTotalSpent()
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<int>();

            return ApiResponse<int>.Success(await Stats.average_total_spent(Config.cfg));
        }

        public async Task<ApiResponse<int>> GetAverageCommandPrice()
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<int>();

            return ApiResponse<int>.Success(await Stats.average_command_price(Config.cfg));
        }
    }
}