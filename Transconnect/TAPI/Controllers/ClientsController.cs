using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPI.Auth;
using TransLib;
using TransLib.Auth;
using TransLib.Persons;

namespace TAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ClientsController : ControllerBase
{
    public struct CreateClientRequest {
        public required string first_name { get; set; }
        public required string last_name { get; set; }
        public required string phone { get; set; }
        public required string email { get; set; }
        public required string address { get; set; }
        public required string city { get; set; }
        public required int birth_date { get; set; }
    }

    [HttpPost(Name = "CreateClient")]
    public async Task<ApiResponse<Client>> CreateClient([FromBody] CreateClientRequest body) {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<Client>();

        Client new_client = new Client(
            -1, body.first_name, body.last_name, body.phone, body.email, body.address, body.city, body.birth_date, null, 0
        );

        string? error = new_client.validate();
        if (error != null) return ApiResponse<Client>.Failure(400, "client.invalid_client", error);

        await new_client.create(Config.cfg);

        return ApiResponse<Client>.Success(new_client);
    }

    [HttpGet(Name = "GetClients")]
    public async Task<ApiResponse<List<Client>>> GetClients([FromQuery] int limit, [FromQuery] int offset, [FromQuery] string order_field, [FromQuery] string order_dir)
    {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<List<Client>>();
        
        if (order_field != "city" && order_field != "last_name" && order_field != "total_spent") {
            return ApiResponse<List<Client>>.Failure(400, "client.invalid_order_field", "Invalid order field");
        }

        if (order_dir != "ASC" && order_dir != "DESC") {
            return ApiResponse<List<Client>>.Failure(400, "client.invalid_order_dir", "Invalid order direction");
        }

        if (limit < 0) {
            return ApiResponse<List<Client>>.Failure(400, "client.invalid_limit", "Invalid limit");
        }

        if (offset < 0) {
            return ApiResponse<List<Client>>.Failure(400, "client.invalid_offset", "Invalid offset");
        }
        
        var clients = await Client.list_clients(Config.cfg, order_field, order_dir, limit, offset);
        return ApiResponse<List<Client>>.Success(clients);
    }

    [HttpGet(Name = "GetClientById")]
    public async Task<ApiResponse<Client>> GetClientById([FromQuery] int user_id)
    {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<Client>();

        Person? person = await Person.get_person_by_id(Config.cfg, user_id);
        if (person == null || person is not Client) return ApiResponse<Client>.Failure(404, "client.not_found", "Client not found");

        return ApiResponse<Client>.Success((Client)person);
    }


    public struct UpdateClientRequest {
        public required int user_id { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public long? birth_date { get; set; }
    }

    [HttpPost(Name = "UpdateClient")]
    public async Task<ApiResponse<Client>> UpdateClient([FromBody] UpdateClientRequest body)
    {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<Client>();

        Person? person = await Person.get_person_by_id(Config.cfg, body.user_id);
        if (person == null || person is not Client) return ApiResponse<Client>.Failure(404, "client.not_found", "Client not found");

        Client client = (Client)person;
        
        if (body.first_name != null)
            await client.set_first_name(Config.cfg, body.first_name);
        if (body.last_name != null)
            await client.set_last_name(Config.cfg, body.last_name);
        if (body.phone != null)
            await client.set_phone(Config.cfg, body.phone);
        if (body.email != null)
            await client.set_email(Config.cfg, body.email);
        if (body.address != null)
            await client.set_address(Config.cfg, body.address);
        if (body.city != null)
            await client.set_city(Config.cfg, body.city);
        if (body.birth_date != null)
            await client.set_birth_date(Config.cfg, (long)body.birth_date);

        return ApiResponse<Client>.Success(client);
    }

    [HttpPost(Name = "DeleteClient")]
    public async Task<ApiResponse<bool>> DeleteClient([FromQuery] int user_id)
    {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<bool>();

        Person? person = await Person.get_person_by_id(Config.cfg, user_id);
        if (person == null || person is not Client) return ApiResponse<bool>.Failure(404, "client.not_found", "Client not found");

        Client client = (Client)person;
        await client.delete(Config.cfg);

        return ApiResponse<bool>.Success(true);
    }
}