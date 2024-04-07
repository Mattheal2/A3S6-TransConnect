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
        protected float money;
        protected string db_connection_string;

        public string DB_CONNECTION_STRING { get => db_connection_string; }

        public Company(string name, string address, int money = 0, string server = "localhost", string port = "3306", string database_name = "transcodb", string uid = "root", string pwd = "")
        {
            this.name = name;
            this.address = address;
            this.money = money;

            this.db_connection_string = $"server={server};Port={port};database={database_name};uid={uid};pwd={pwd};";

            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                try
                {
                    c.Open();
                    MySqlCommand cmd = new MySqlCommand($"INSERT INTO company VALUES ('{this.name}', '{this.address}', {this.money});", c);
                    cmd.ExecuteReader();
                }
                catch (MySqlException ex)
                {
                    if(!ex.Message.Contains("Duplicate entry")) Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }

        /// Display the company informations
        public void display()
        {
            throw new NotImplementedException();
        }

        #region Company management

        ///Adds or removes money from company account. Doesn't check if there's enough money left.
        public async Task<bool> update_money(float amount)
        {
            this.money += amount;
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                try
                {
                    await c.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand($"UPDATE company SET money = money + ({amount}) WHERE company_name = '{this.name}';", c);
                    await cmd.ExecuteReaderAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

        }
        
        #endregion
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
        public async Task<bool> hire_employee_async(Employee new_employee) //A reviser : enlever classe Driver et tout faire en fn de position & ID (1re lettre)
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
        private async Task<bool> add_vehicle_async(Vehicle new_vehicle)
        {
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                try
                {
                    await c.OpenAsync();
                    (string car_string_option, string car_string_arg) = new_vehicle is Car ? (", 'CAR', seats", $", {((Car)new_vehicle).SEATS}") : ("", "");
                    (string van_string_option, string van_string_arg) = new_vehicle is Van ? (", 'VAN', usage", $", '{((Van)new_vehicle).Usage}'") : ("", "");
                    (string truck_string_option, string truck_string_arg) = new_vehicle is Truck ? (", 'TRUCK', volume, truck_type", $", {((Truck)new_vehicle).VOLUME}, '{((Truck)new_vehicle).TRUCK_TYPE}'") : ("", "");
                    MySqlCommand cmd = new MySqlCommand($"INSERT INTO vehicle(license_plate, brand, model, price{car_string_option + van_string_option + truck_string_option}) VALUES('{new_vehicle.LICENSE_PLATE}', '{new_vehicle.BRAND}', '{new_vehicle.MODEL}', {new_vehicle.Price}{car_string_arg + van_string_arg + truck_string_arg})", c);

                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            for (int i = 0; i < rdr.FieldCount; i++) Console.Write(rdr[i] + "\t");
                            Console.WriteLine();
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

        }

        public async Task<bool> buy_vehicle_async(Vehicle new_vehicle)
        {
            if (await add_vehicle_async(new_vehicle))
            {
                Console.Write(await update_money(new_vehicle.Price) ? "" : "Error : vehicle added, but failed update money.\n");
                return true;
            }
            else return false;
        }

        public async Task<float> get_vehicle_price(string license_plate)
        {
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                try
                {
                    await c.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand($"SELECT price FROM vehicle WHERE license_plate = '{license_plate}'", c);

                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        await rdr.ReadAsync();
                        return rdr.GetFloat(0);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return -1f;
                }
            }

        }

        private async Task<bool> delete_vehicle_async(string license_plate)
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

        public async Task<bool> sell_vehicle_async(string license_plate)
        {
            float price = get_vehicle_price(license_plate).Result;
            if (await delete_vehicle_async(license_plate))
            {
                Console.Write(await update_money(price * 0.8f) ? "" : "Error : vehicle removed, but failed update money.\n");
                return true;
            }
            else return false;
        }
        #endregion

        #region Monitoring functions

        public async Task<List<Employee>> get_employees_list_async()
        {
            List<Employee> employees = new List<Employee>();
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM person WHERE user_id LIKE 'E%'", c);

                try
                {
                    await c.OpenAsync();
                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            employees.Add(new Employee(rdr.GetString("user_id"), rdr.GetString("first_name"), rdr.GetString("last_name"), rdr.GetString("phone"), rdr.GetString("email"), rdr.GetString("address"), rdr.GetDateTime("birth_date"), rdr.GetString("position"), rdr.GetFloat("salary"), rdr.GetDateTime("hire_date")));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return employees;
            }
        }

        public async Task<List<Employee>> get_clients_list_async()
        {
            List<Employee> employees = new List<Employee>();
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM person WHERE user_id LIKE 'C%'", c);

                try
                {
                    await c.OpenAsync();
                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            employees.Add(new Employee(rdr.GetString("user_id"), rdr.GetString("first_name"), rdr.GetString("last_name"), rdr.GetString("phone"), rdr.GetString("email"), rdr.GetString("address"), rdr.GetDateTime("birth_date"), rdr.GetString("position"), rdr.GetFloat("salary"), rdr.GetDateTime("hire_date")));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return employees;
            }
        }

        public async Task<List<Vehicle>> get_vehicles_list_async() //peut être agrémentée d'un filtre en arg (car / van / truck)
        {
            List<Vehicle> vehicles = new List<Vehicle>();
            using (MySqlConnection c = new MySqlConnection(this.db_connection_string))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM vehicle", c);

                try
                {
                    await c.OpenAsync();
                    using (DbDataReader rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            switch (rdr.GetString("vehicle_type")) {
                                case "CAR":
                                    vehicles.Add(new Car(rdr.GetString("license_plate"), rdr.GetString("brand"), rdr.GetString("model"), rdr.GetFloat("price"), rdr.GetInt32("seats")));
                                    break;
                                case "VAN":
                                    vehicles.Add(new Van(rdr.GetString("license_plate"), rdr.GetString("brand"), rdr.GetString("model"), rdr.GetFloat("price"), rdr.GetString("usage")));
                                    break;
                                case "TRUCK":
                                    vehicles.Add(new Truck(rdr.GetString("license_plate"), rdr.GetString("brand"), rdr.GetString("model"), rdr.GetFloat("price"), rdr.GetInt32("volume"), rdr.GetString("truck_type")));
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return vehicles;
            }
        }

        #endregion
    }
}
