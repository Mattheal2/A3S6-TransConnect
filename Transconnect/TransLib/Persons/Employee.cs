﻿using MySql.Data.MySqlClient;
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
        public float salary { get; } // in cents
        public long hire_date { get; }
        public string license_type { get; }
        public int supervisor_id { get; set; }
        public bool show_on_org_chart { get; set; }

        public Employee(
            int id_employee, string first_name, string last_name, string phone, string email, string address, string city, long birth_date, bool deleted,
            string password_hash, string position, float salary, long hire_date, string license_type,
            int supervisor_id, bool show_on_org_chart) :
            base(id_employee, first_name, last_name, phone, email, address, city, birth_date, deleted, password_hash)
        {
            this.position = position;
            this.salary = salary;
            this.hire_date = hire_date;
            this.license_type = license_type;
            this.supervisor_id = supervisor_id;
            this.show_on_org_chart = show_on_org_chart;
        }

        public override async Task create(AppConfig cfg)
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
        public async static new Task<Employee?> from_reader_async(DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return cast_from_open_reader(reader);
            }
        }

        /// Returns an Employee list from a reader.
        public async static new Task<List<Employee>> from_reader_multiple(DbDataReader reader, string prefix = "")
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

        protected static new Employee cast_from_open_reader(DbDataReader reader, string prefix = "")
        {
            if (reader.GetString($"{prefix}user_type") == "EMPLOYEE")
            {
                return new Employee(
                    reader.GetInt32($"{prefix}user_id"),
                    reader.GetString($"{prefix}first_name"),
                    reader.GetString($"{prefix}last_name"),
                    reader.GetString($"{prefix}phone"),
                    reader.GetString($"{prefix}email"),
                    reader.GetString($"{prefix}address"),
                    reader.GetString($"{prefix}city"),
                    reader.GetInt64($"{prefix}birth_date"),
                    reader.GetBoolean($"{prefix}deleted"),
                    reader.GetString($"{prefix}password_hash"),
                    reader.GetString($"{prefix}position"),
                    reader.GetFloat($"{prefix}salary"),
                    reader.GetInt64($"{prefix}hire_date"),
                    reader.GetString($"{prefix}license_type"),
                    reader.GetInt32($"{prefix}supervisor_id"),
                    reader.GetBoolean($"{prefix}show_on_org_chart")
                );
            }
            else throw new Exception("invalid user_type");
        }


        public DateTime[] get_schedule()
        {
            throw new NotImplementedException();
        }

        public string? validate()
        {
            long current_time = DateTimeOffset.Now.ToUnixTimeSeconds();

            if (current_time < birth_date) return "Birth date is in the future";
            if (salary < 0) return "Salary cannot be negative";

            return null;
        }

        public static async Task<List<Employee>> list_employees(AppConfig cfg, string order_field, string order_dir, int limit, int offset)
        {
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM person WHERE user_type = 'EMPLOYEE' AND NOT deleted ORDER BY {order_field} {order_dir} LIMIT {limit} OFFSET {offset};");
            using (DbDataReader reader = await cfg.query(cmd))
            {
                return await from_reader_multiple(reader);
            }
        }

        public override async Task delete(AppConfig cfg)
        {
            // Move all subordinates to supervisor
            await move_all_subordinates_to_supervisor(cfg);
            MySqlCommand cmd = new MySqlCommand(@"
                UPDATE person
                SET 
                    deleted = true, first_name = '', last_name = '', phone = '', email = '', address = '', city = '', birth_date = 0,
                    position = '', salary = 0, hire_date = 0, license_type = '', supervisor_id = 0, show_on_org_chart = false
                WHERE user_id = @user_id");
            cmd.Parameters.AddWithValue("@user_id", user_id);
            await cfg.query(cmd);

            
            this.deleted = true;
        }

        private async Task move_all_subordinates_to_supervisor(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                UPDATE person
                SET supervisor_id = @supervisor_id
                WHERE supervisor_id = @user_id AND user_type = 'EMPLOYEE' AND NOT deleted;");
            cmd.Parameters.AddWithValue("@supervisor_id", supervisor_id);
            cmd.Parameters.AddWithValue("@user_id", user_id);
            await cfg.query(cmd);
        }
    }
}
