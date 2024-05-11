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
        public int id_employee { get; }
        public string position { get; }
        public float salary { get; }
        public DateTime hire_date { get; }

        public Employee(int id_employee, string first_name, string last_name, string phone, string email, string address, DateTime birth_date, string position, float salary, DateTime hire_date) : base(id_employee, first_name, last_name, phone, email, address, birth_date)
        {
            this.id_employee = id_employee;
            this.position = position;
            this.salary = salary;
            this.hire_date = hire_date;
        }

        public override MySqlCommand save_command()
        {
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO person (user_id, user_type, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date, license_type) VALUES(@user_id, @user_type, @first_name, @last_name, @phone, @email, @address, @birth_date, @position, @salary, @hire_date, @license_type);");
            cmd.Parameters.AddWithValue("@user_id", id_employee);
            cmd.Parameters.AddWithValue("@user_type", "EMPLOYEE");
            cmd.Parameters.AddWithValue("@first_name", first_name);
            cmd.Parameters.AddWithValue("@last_name", last_name);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Parameters.AddWithValue("@birth_date", birth_date);
            cmd.Parameters.AddWithValue("@position", position);
            cmd.Parameters.AddWithValue("@salary", salary);
            cmd.Parameters.AddWithValue("@hire_date", hire_date);

            return cmd;
        }

        /// Returns an Employee object from a reader. If muliple rows are returned, only the first one is used.
        public async new static Task<Employee> from_reader_async(DbDataReader? reader)
        {
            using (reader)
            {
                if (reader == null) throw new Exception("reader is null");
                await reader.ReadAsync();
                return cast_from_open_reader(reader);
            }
        }

        public async static new Task<List<Employee>> from_reader_mulitple_async(DbDataReader? reader)
        {
            using (reader)
            {
                if (reader == null) throw new Exception("reader is null");
                List<Employee> employees = new List<Employee>();
                while (await reader.ReadAsync())
                {
                    employees.Append(cast_from_open_reader(reader));
                }
                return employees;
            }
        }

        protected static new Employee cast_from_open_reader(DbDataReader? reader)
        {

            if (reader == null) throw new Exception("reader is null");
            if (!reader.IsClosed)
            {
                if (reader.GetString("user_type") == "EMPLOYEE")
                {
                    return new Employee(reader.GetInt32("user_id"), reader.GetString("first_name"), reader.GetString("last_name"), reader.GetString("phone"), reader.GetString("email"), reader.GetString("address"), reader.GetDateTime("birth_date"), reader.GetString("position"), reader.GetFloat("salary"), reader.GetDateTime("hire_date"));
                }
                else throw new Exception("invalid user_type");
            }
            else
            {
                throw new Exception("unable to read closed reader");
            }

        }


        public DateTime[] get_schedule()
        {
            throw new NotImplementedException();
        }

    }
}
