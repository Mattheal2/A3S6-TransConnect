using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPI.Auth;
using TransLib;
using TransLib.Auth;
using TransLib.Persons;

namespace TAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    public struct CreateEmployeeRequest {
        public required string first_name { get; set; }
        public required string last_name { get; set; }
        public required string phone { get; set; }
        public required string email { get; set; }
        public required string address { get; set; }
        public required string city { get; set; }
        public required int birth_date { get; set; }
        public required string password { get; set; }
        public required string position { get; set; }
        public required int salary { get; set; } // in cents
        public required string license_type { get; set; }
        public int supervisor_id { get; set; }
        public required bool show_on_org_chart { get; set; }
    }

    [HttpPost(Name = "CreateEmployee")]
    public async Task<ApiResponse<Employee>> CreateEmployee([FromBody] CreateEmployeeRequest body) {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<Employee>();
        
        string password_hash =  PasswordAuthenticator.hash_password(body.password);
        long hire_date = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Employee new_employee = new Employee(
            -1, body.first_name, body.last_name, body.phone, body.email, body.address, body.city,
            body.birth_date, false, password_hash, body.position, body.salary, hire_date, body.license_type,
            body.supervisor_id, body.show_on_org_chart
        );

        string? error = new_employee.validate();
        if (error != null) return ApiResponse<Employee>.Failure(400, "employee.invalid_employee", error);

        await new_employee.create(Config.cfg);

        return ApiResponse<Employee>.Success(new_employee);
    }

    [HttpGet(Name = "GetEmployees")]
    public async Task<ApiResponse<List<Employee>>> GetEmployees([FromQuery] string order_field, [FromQuery] string order_dir)
    {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<List<Employee>>();
        
        if (order_field != "city" && order_field != "last_name" && order_field != "total_spent") {
            return ApiResponse<List<Employee>>.Failure(400, "employee.invalid_order_field", "Invalid order field");
        }

        if (order_dir != "ASC" && order_dir != "DESC") {
            return ApiResponse<List<Employee>>.Failure(400, "employee.invalid_order_dir", "Invalid order direction");
        }
        
        var employees = await Employee.list_employees(Config.cfg, order_field, order_dir);
        return ApiResponse<List<Employee>>.Success(employees);
    }

    [HttpGet(Name = "GetEmployeeById")]
    public async Task<ApiResponse<Employee>> GetEmployeeById([FromQuery] int user_id)
    {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<Employee>();

        Person? person = await Person.get_person_by_id(Config.cfg, user_id);
        if (person == null || person is not Employee) return ApiResponse<Employee>.Failure(404, "employee.not_found", "Employee not found");

        return ApiResponse<Employee>.Success((Employee)person);
    }


    public struct UpdateEmployeeRequest {
        public required int user_id { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public long? birth_date { get; set; }
    }

    [HttpPost(Name = "UpdateEmployee")]
    public async Task<ApiResponse<Employee>> UpdateEmployee([FromBody] UpdateEmployeeRequest body)
    {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<Employee>();

        Person? person = await Person.get_person_by_id(Config.cfg, body.user_id);
        if (person == null || person is not Employee) return ApiResponse<Employee>.Failure(404, "employee.not_found", "Employee not found");

        Employee employee = (Employee)person;
        
        if (body.first_name != null)
            await employee.set_first_name(Config.cfg, body.first_name);
        if (body.last_name != null)
            await employee.set_last_name(Config.cfg, body.last_name);
        if (body.phone != null)
            await employee.set_phone(Config.cfg, body.phone);
        if (body.email != null)
            await employee.set_email(Config.cfg, body.email);
        if (body.address != null)
            await employee.set_address(Config.cfg, body.address);
        if (body.city != null)
            await employee.set_city(Config.cfg, body.city);
        if (body.birth_date != null)
            await employee.set_birth_date(Config.cfg, (long)body.birth_date);

        return ApiResponse<Employee>.Success(employee);
    }

    [HttpPost(Name = "DeleteEmployee")]
    public async Task<ApiResponse<bool>> DeleteEmployee([FromQuery] int user_id)
    {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<bool>();

        Person? person = await Person.get_person_by_id(Config.cfg, user_id);
        if (person == null || person is not Client) return ApiResponse<bool>.Failure(404, "employee.not_found", "Employee not found");

        Employee employee = (Employee)person;
        await employee.delete(Config.cfg);

        return ApiResponse<bool>.Success(true);
    }

    [HttpPost(Name = "GetEmployeesOrgChart")]
    public async Task<ApiResponse<MultiNodeTree<Employee>>> GetEmployeesOrgChart()
    {
        Authorization auth = await Authorization.obtain(Config.cfg, Request.HttpContext);
        if (!auth.is_employee()) return auth.get_unauthorized_error<MultiNodeTree<Employee>>();

        MultiNodeTree<Employee> employees = await Employee.get_org_chart(Config.cfg);
        return ApiResponse<MultiNodeTree<Employee>>.Success(employees);
    }
}