using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Employee : Person
    {
        protected string id_employee;
        protected string position;
        protected float salary;
        protected DateTime hire_date;

        public string Id_employee { get => id_employee;}
        public string Position { get => position; set => position = value; }
        public float Salary { get => salary; set => salary = value; }
        public DateTime Hire_date { get => hire_date; }

        public Employee(string id_employee, string first_name, string last_name, string phone, string email, string address, DateTime birth_date, string position, float salary, DateTime hire_date) : base(first_name, last_name, phone, email, address, birth_date)
        {
            this.id_employee = id_employee;
            this.position = position;
            this.salary = salary;
            this.hire_date = hire_date;
        }

        public override MySqlCommand save_command()
        {
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO person (user_id, user_type, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date, license_type) VALUES(@user_id, @user_type, @first_name, @last_name, @phone, @email, @address, @birth_date, @position, @salary, @hire_date, @license_type);");
            cmd.Parameters.AddWithValue("@user_id", this.id_employee);
            cmd.Parameters.AddWithValue("@user_type", "EMPLOYEE");
            cmd.Parameters.AddWithValue("@first_name", this.first_name);
            cmd.Parameters.AddWithValue("@last_name", this.last_name);
            cmd.Parameters.AddWithValue("@phone", this.phone);
            cmd.Parameters.AddWithValue("@email", this.email);
            cmd.Parameters.AddWithValue("@address", this.address);
            cmd.Parameters.AddWithValue("@birth_date", this.birth_date);
            cmd.Parameters.AddWithValue("@position", this.position);
            cmd.Parameters.AddWithValue("@salary", this.salary);
            cmd.Parameters.AddWithValue("@hire_date", this.hire_date);

            return cmd;
        }

        /// Returns an Employee object from a reader. If muliple rows are returned, only the first one is used.
        public static new Employee from_reader(DbDataReader reader)
        {
            return new Employee(reader.GetString("user_id"), reader.GetString("first_name"), reader.GetString("last_name"), reader.GetString("phone"), reader.GetString("email"), reader.GetString("address"), reader.GetDateTime("birth_date"), reader.GetString("position"), reader.GetFloat("salary"), reader.GetDateTime("hire_date"));
        }

        /// Returns an Employee list from a reader.
        public async static Task<List<Employee>> from_reader_mulitple_async(DbDataReader reader)
        {
            List<Employee> employees = new List<Employee>();
            while(await reader.ReadAsync())
            {
                 employees.Append(Employee.from_reader(reader));
            }
            return employees;
        }


        public DateTime[] get_schedule()
        {
            throw new NotImplementedException();
        }

    }
}
