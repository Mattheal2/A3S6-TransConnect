using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TransLib.Persons
{
    public class Employee : Person
    {
        public string position { get; }
        public float salary { get; }
        public long hire_date { get; }
        public string license_type { get; }
        public int supervisor_id { get; set; }
        public bool show_on_org_chart { get; set; }

        public Employee(
            int id_employee, string first_name, string last_name, string phone, string email, string address, string city, long birth_date,
            string password_hash, string position, float salary, long hire_date, string license_type,
            int supervisor_id, bool show_on_org_chart) :
            base(id_employee, first_name, last_name, phone, email, address, city, birth_date, password_hash)
        {
            this.position = position;
            this.salary = salary;
            this.hire_date = hire_date;
            this.license_type = license_type;
            this.supervisor_id = supervisor_id;
            this.show_on_org_chart = show_on_org_chart;
        }

        public async Task CreateEmployee(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(
                @"INSERT INTO person (user_type, first_name, last_name, phone, email, address, city, birth_date, position, salary, hire_date, license_type, password_hash, supervisor_id, show_on_org_chart)
                VALUES(@user_type, @first_name, @last_name, @phone, @email, @address, @city, @birth_date, @position, @salary, @hire_date, @license_type, @password_hash, @supervisor_id, @show_on_org_chart);");
            
            cmd.Parameters.AddWithValue("@user_type", user_type);
            cmd.Parameters.AddWithValue("@first_name", first_name);
            cmd.Parameters.AddWithValue("@last_name", last_name);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Parameters.AddWithValue("@city", city);
            cmd.Parameters.AddWithValue("@birth_date", birth_date);
            cmd.Parameters.AddWithValue("@position", position);
            cmd.Parameters.AddWithValue("@salary", salary);
            cmd.Parameters.AddWithValue("@hire_date", hire_date);
            cmd.Parameters.AddWithValue("@license_type", license_type);
            cmd.Parameters.AddWithValue("@password_hash", password_hash);
            cmd.Parameters.AddWithValue("@supervisor_id", supervisor_id);
            cmd.Parameters.AddWithValue("@show_on_org_chart", show_on_org_chart);

            await cfg.execute(cmd);
            user_id = (int)cmd.LastInsertedId;
        }

        public override string user_type { get; } = "EMPLOYEE";

        /// Returns an Employee object from a reader. If muliple rows are returned, only the first one is used.
        public async static new Task<Employee?> from_reader_async(DbDataReader reader)
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return cast_from_open_reader(reader);
            }
        }

        /// Returns an Employee list from a reader.
        public async static new Task<List<Employee>> from_reader_multiple(DbDataReader reader)
        {
            using (reader)
            {
                List<Employee> persons = new List<Employee>();
                while (await reader.ReadAsync())
                {
                    Employee? person = cast_from_open_reader(reader);
                    if (person != null)
                        persons.Append(person);
                }
                return persons;
            }
        }

        protected static new Employee cast_from_open_reader(DbDataReader reader)
        {
            if (reader.GetString("user_type") == "EMPLOYEE")
            {
                return new Employee(
                    reader.GetInt32("user_id"),
                    reader.GetString("first_name"),
                    reader.GetString("last_name"),
                    reader.GetString("phone"),
                    reader.GetString("email"),
                    reader.GetString("address"),
                    reader.GetString("city"),
                    reader.GetInt64("birth_date"),
                    reader.GetString("password_hash"),
                    reader.GetString("position"),
                    reader.GetFloat("salary"),
                    reader.GetInt64("hire_date"),
                    reader.GetString("license_type"),
                    reader.GetInt32("supervisor_id"),
                    reader.GetBoolean("show_on_org_chart")
                );
            }
            else throw new Exception("invalid user_type");
        }


        public DateTime[] get_schedule()
        {
            throw new NotImplementedException();
        }

    }
}
