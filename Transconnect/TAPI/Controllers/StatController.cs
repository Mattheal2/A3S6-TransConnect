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
        /// <summary>
        /// Returns the deliveries made by each driver.
        /// </summary>
        /// <param name="driver_id"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetDeliveriesByDriver")]
        public async Task<ApiResponse<Order[]>> GetDeliveriesByDriver([FromQuery] int driver_id)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order[]>();

            return null;
        }

        /// <summary>
        /// Returns deliveries in a given time range.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetDeliveriesBetween")]
        public async Task<ApiResponse<Order[]>> GetDeliveriesBetween([FromQuery] long start, [FromQuery] long end)
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<Order[]>();

            return ApiResponse<Order[]>.Success((await Stats.deliveries_between(Config.cfg, start, end)).ToArray());
        }

        public struct StatsResponse
        {
            public int average_total_spent { get; set; }
            public int average_command_price { get; set; }
            public int total_deliveries_count { get; set; }
        }

        /// <summary>
        /// Returns various staistics.
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetStats")]
        public async Task<ApiResponse<StatsResponse>> GetStats()
        {
            Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
            if (!auth.is_employee()) return auth.get_unauthorized_error<StatsResponse>();

            return ApiResponse<StatsResponse>.Success(new StatsResponse
            {
                average_total_spent = await Stats.average_total_spent(Config.cfg),
                average_command_price = await Stats.average_command_price(Config.cfg),
                total_deliveries_count = await Stats.total_deliveries_count(Config.cfg)
            });
        }
    }
}