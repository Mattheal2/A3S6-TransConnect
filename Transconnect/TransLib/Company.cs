using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TransLib
{
    public class Company
    {
        static private int employee_id_counter = 0;
        static private int client_id_counter = 0;
        protected string name;
        protected string address;
        protected string db_connection_string;

        public string DB_CONNECTION_STRING { get => db_connection_string; }

        public Company(string name, string address, string server = "localhost", string port = "3306", string database_name = "transcodb", string uid = "root", string pwd = "")
        {
            this.name = name;
            this.address = address;

            this.db_connection_string = $"server={server};Port={port};database={database_name};uid={uid};pwd={pwd};";
        }

        /// Display the company informations
        public void display()
        {
            throw new NotImplementedException();
        }


        #region Staff management
        /// Gets the highest employee ID in the database and returns it. Type cab be "C" for client, "D" for driver etc...
        public async Task<int> get_available_id(string type)
        {
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT MAX(user_id) FROM person WHERE user_id LIKE \"{type}%\";", c);
                try
                {
                    await c.OpenAsync();
                    Console.WriteLine("Connexion with database established successfully.");

                    DbDataReader rdr = await cmd.ExecuteReaderAsync();
                    while (await rdr.ReadAsync())
                    {
                        return 1 + int.Parse(((string)rdr.GetValue(0)).Substring(1));
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return 0;
            }
        }

        /// Adds a new employee and returns true if the operation was successful
        public async Task<bool> hire_employee(Employee new_employee)
        {
            Company.employee_id_counter++;
            Console.WriteLine($"Adding new employee {employee_id_counter} {new_employee.First_name} {new_employee.Last_name}...");
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO personne (user_id, first_name, last_name, phone, email, address, " +
                $"birth_date, position, salary, hire_date) VALUES({Company.client_id_counter}, \"{new_employee.First_name}\", " +
                $"\"{new_employee.Last_name}\", {new_employee.Phone}, \"{new_employee.Email}\", \"{new_employee.Address}\", " +
                $"\"{new_employee.Birth_date}\", \"{new_employee.Position}\", {new_employee.Salary}, \"{new_employee.Hire_date}\");");
            try
            {
                DbDataReader rdr = await cmd.ExecuteReaderAsync();
                while (rdr.Read())
                {
                    Console.WriteLine($"{rdr["first_name"]} {rdr["last_name"]}");
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        /// Fires a new employee and returns true if the operation was successful.
        /// Should return false if the employee doesn't exists.
        public bool fire_employee(string id)
        {
            throw new NotImplementedException();

        }

        /// Finds en employee using his first and last name and returns his ID.
        /// Returns an empty string if the employee doesn't exists.
        public string get_employee_id_by_name(string first_name, string last_name)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Vehicle management
        public bool buy_vehicle(Vehicle new_vehicle)
        {
            throw new NotImplementedException();
        }

        public bool sell_vehicle(string licence_plate)
        {
            throw new NotImplementedException();
        }

        public bool delete_vehicle(string licence_plate)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
