using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPI.Auth;
using TransLib;
using TransLib.Auth;
using TransLib.Persons;

namespace TAPI.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatController : ControllerBase
    {
        public async Task GetDeliveriesByDriver([FromQuery] int driver_id)
        {

        }

        public async Task GetDeliveriesBetween([FromQuery] long start, [FromQuery] long end)
        {

        }

        public async Task GetAverageTotalSpent()
        {

        }

        public async Task GetAverageCommandPrice()
        {

        }
    }
}