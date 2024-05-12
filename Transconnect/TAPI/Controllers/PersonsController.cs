using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransLib;
using TransLib.Persons;


namespace TAPI.Controllers
{
    // [Route("api/[controller]")]
    // [ApiController]
    // public class ClientsController : ControllerBase
    // {
    //     [HttpGet(Name = "GetClients")]
    //     public async Task<List<Client>?> Get()
    //     {
    //         return await Config.transconnect.get_clients_list_async();
    //     }
    // }

    // [Route("api/[controller]")]
    // [ApiController]
    // public class EmployeesController : ControllerBase
    // {
    //     [HttpGet(Name = "GetEmployees")]
    //     public async Task<IEnumerable<Employee>?> Get()
    //     {
    //         List<Employee>? e_list = await Config.transconnect.get_employees_list_async();
    //         if (e_list != null) return e_list.ToArray();
    //         else return null;
    //     }
    // }


}
