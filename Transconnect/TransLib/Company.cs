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
using Mysqlx.Crud;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace TransLib
{
    public class Company
    {
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
                try
                {
                    await c.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand($"SELECT user_id FROM person WHERE user_id LIKE \"{type}%\" ORDER BY CAST(SUBSTRING(user_id, 2) AS UNSIGNED) DESC LIMIT 1;", c);

                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            return 1 + int.Parse(((string)rdr.GetValue(0)).Substring(1));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return 0;
            }
        }

        /// Adds a new employee and returns true if the operation was successful
        public async Task<bool> hire_employee_async(Employee new_employee)
        {
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                try
                {
                    await c.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand($"INSERT INTO person (user_id, first_name, last_name, phone, email, address, " +
                        $"birth_date, position, salary, hire_date{(new_employee is Driver ? $", license_type" : "")}) VALUES(\"{new_employee.Id_employee}\", \"{new_employee.First_name}\", " +
                        $"\"{new_employee.Last_name}\", {new_employee.Phone}, \"{new_employee.Email}\", \"{new_employee.Address}\", " +
                        $"\"{new_employee.Birth_date}\", \"{new_employee.Position}\", {new_employee.Salary}, \"{new_employee.Hire_date}\"{(new_employee is Driver ? $", \"{((Driver)new_employee).License_type}\"" : "")});", c);

                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++) Console.Write(rdr[i] + "\t");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            return true;
        }

        /// Finds en employee using his first and last name and returns his ID.
        /// Returns an empty string if the employee doesn't exists.
        public async Task<string> get_employee_id_by_name_async(string first_name, string last_name)
        {
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                bool found = false;
                try
                {
                    await c.OpenAsync();
                    MySqlCommand get_count = new MySqlCommand($"" +
                        $"SELECT COUNT(*) " +
                        $"FROM person " +
                        $"WHERE first_name = \"{first_name}\" AND last_name = \"{last_name}\";", c);

                    using (DbDataReader rdr = await get_count.ExecuteReaderAsync())
                    {
                        await rdr.ReadAsync();
                        if (rdr.GetInt32(0) == 0) Console.WriteLine($"Error : No employee found with name : {first_name} {last_name}");
                        else if (rdr.GetInt32(0) > 1) Console.WriteLine($"Error : Multiple employees found with name : {first_name} {last_name}");
                        else found = true;
                    }
                    if (found == true)
                    {
                        MySqlCommand cmd = new MySqlCommand($"" +
                        $"SELECT user_id " +
                        $"FROM person " +
                        $"WHERE first_name = \"{first_name}\" AND last_name = \"{last_name}\";", c);

                        using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                        {
                            while (await rdr.ReadAsync())
                            {
                                return rdr.GetString(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return "";

        }

        /// Fires a new employee and returns true if the operation was successful.
        /// Should return false if the employee doesn't exists.
        public async Task<bool> fire_employee_by_id_async(string id)
        {
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                MySqlCommand cmd = new MySqlCommand($"DELETE FROM person WHERE user_id = '{id}'", c);

                try
                {
                    await c.OpenAsync();
                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++) Console.Write(rdr[i] + "\t");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            return true;
        }

        /// Try to remove an employee using it's name. If 2 employees with same name are found, operation is cancelled
        public async Task<bool> fire_employee_by_name_async(string first_name, string last_name)
        {
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                MySqlCommand count_query = new MySqlCommand($"SELECT COUNT(user_id) FROM person WHERE first_name = '{first_name}' AND last_name='{last_name}'", c);
                MySqlCommand delete_query = new MySqlCommand($"DELETE FROM person WHERE first_name = '{first_name}' AND last_name='{last_name}'", c);

                try
                {
                    await c.OpenAsync();
                    string buffer = "";
                    using (DbDataReader rdr = await count_query.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++) buffer += rdr[i];
                        }
                    }

                    if (int.Parse(buffer) > 1) throw new Exception("Error : can't delete multiple users with same name");
                    if (int.Parse(buffer) == 0) throw new Exception("Error : user not found");
                    using (DbDataReader rdr = await delete_query.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++) Console.Write(rdr[i] + "\t");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Vehicle management
        public async Task<bool> buy_vehicle_async(Vehicle new_vehicle)
        {
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                try
                {
                    await c.OpenAsync();
                    (string car_string_option, string car_string_arg) = new_vehicle is Car ? (", seats", $", {((Car)new_vehicle).SEATS}") : ("", "");
                    (string van_string_option, string van_string_arg) = new_vehicle is Van ? (", usage", $", '{((Van)new_vehicle).Usage}'") : ("", "");
                    (string truck_string_option, string truck_string_arg) = new_vehicle is Truck ? (", volume, truck_type", $", {((Truck)new_vehicle).VOLUME}, '{((Truck)new_vehicle).TRUCK_TYPE}'") : ("", "");
                    MySqlCommand cmd = new MySqlCommand($"INSERT INTO vehicle(license_plate, brand, model{car_string_option+van_string_option+truck_string_option}) VALUES('{new_vehicle.LICENSE_PLATE}', '{new_vehicle.BRAND}', '{new_vehicle.MODEL}'{car_string_arg + van_string_arg + truck_string_arg})", c);

                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++) Console.Write(rdr[i] + "\t");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            //this.money -= new_vehicle.Price //Il faut MAJ les tables et les classes pour tenir compte du prix => plus tard tkt
            return true;

        }

        public async Task<bool> sell_vehicle_async(string license_plate)
        {
            if(await delete_vehicle_async(license_plate))
            {
                //this.money += vehicle.price * 0.8 //à récupérer avec une query mais là j'ai trop la flemme (en vrai j'ai juste pas le temps hein)
                return true;
            }
            return false;
        }

        public async Task<bool> delete_vehicle_async(string license_plate)
        {
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                MySqlCommand cmd = new MySqlCommand($"DELETE FROM vehicle WHERE license_plate = '{license_plate}'", c);

                try
                {
                    await c.OpenAsync();
                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++) Console.Write(rdr[i] + "\t");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            return true;

        }
        #endregion
    }
}
