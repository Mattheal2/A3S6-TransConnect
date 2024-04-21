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
using Org.BouncyCastle.Crypto;
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
                    if (!ex.Message.Contains("Duplicate entry")) Console.WriteLine(ex.Message);
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

        #region Database connection
        /// Connects to the database, query and returns the reader
        private async Task<DbDataReader?> query(MySqlCommand command)
        {
            MySqlConnection connection = new MySqlConnection(this.db_connection_string);
            await connection.OpenAsync();
            try
            {
                command.Connection = connection;
                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private async Task<string> reader_to_string(DbDataReader? reader)
        {
            if (reader == null) return "";
            string buffer = "";
            while (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++) buffer += reader[i] + " ";
                buffer += "\n";
            }
            return buffer;
        }
        #endregion

        #region Company management

        ///Adds or removes money from company account. Doesn't check if there's enough money left.
        public async Task<bool> update_money(float amount)
        {
            this.money += amount;

            try
            {
                DbDataReader? reader = await query(new MySqlCommand($"UPDATE company SET money = money + ({amount}) WHERE company_name = '{this.name}';"));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        #endregion

        #region Staff management
        /// Gets the highest employee ID in the database and returns it. Type cab be "C" for client, "D" for driver etc...
        /// Deprecated (id auto increment in database)
        /*public async Task<int> get_available_id(string type)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT user_id FROM person WHERE user_id LIKE \"{type}%\" ORDER BY CAST(SUBSTRING(user_id, 2) AS UNSIGNED) DESC LIMIT 1;");

                using (DbDataReader rdr = await query(cmd))
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
        }*/

        /// Adds a new employee and returns true if the operation was successful
        public async Task<bool> hire_employee_async(Employee new_employee) //A reviser : enlever classe Driver et tout faire en fn de position & ID (1re lettre)
        {
            try
            {
                Console.WriteLine(reader_to_string(await query(new_employee.save_command())));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// Finds en employee using his first and last name and returns his ID.
        /// Returns an empty string if the employee doesn't exists.
        public async Task<string> get_employee_id_by_name_async(string first_name, string last_name)
        {
            bool found = false;
            try
            {
                MySqlCommand get_count_query = new MySqlCommand($"" +
                    $"SELECT COUNT(*) " +
                    $"FROM person " +
                    $"WHERE first_name = \"{first_name}\" AND last_name = \"{last_name}\";");

                using (DbDataReader? rdr = await query(get_count_query))
                {
                    if (rdr == null) throw new Exception("reader is null");
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
                    $"WHERE first_name = \"{first_name}\" AND last_name = \"{last_name}\";");

                    using (DbDataReader? rdr = await query(cmd))
                    {
                        if (rdr == null) throw new Exception("reader is null");
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

            return "";

        }

        /// Fires a new employee and returns true if the operation was successful.
        /// Should return false if the employee doesn't exists.
        public async Task<bool> fire_employee_by_id_async(string id)
        {
            MySqlCommand cmd = new MySqlCommand($"DELETE FROM person WHERE user_id = '{id}'");

            try
            {
                Console.WriteLine(reader_to_string(await query(cmd)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        /// Try to remove an employee using it's name. If 2 employees with same name are found, operation is cancelled
        public async Task<bool> fire_employee_by_name_async(string first_name, string last_name)
        {
            MySqlCommand get_count_query = new MySqlCommand($"SELECT COUNT(user_id) FROM person WHERE first_name = '{first_name}' AND last_name='{last_name}'");
            MySqlCommand delete_query = new MySqlCommand($"DELETE FROM person WHERE first_name = '{first_name}' AND last_name='{last_name}'");

            try
            {
                string output = await reader_to_string(await query(get_count_query));

                if (int.Parse(output) > 1) throw new Exception("Error : can't delete multiple users with same name");
                if (int.Parse(output) == 0) throw new Exception("Error : user not found");

                Console.WriteLine(await reader_to_string(await query(delete_query)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        #endregion

        #region Vehicle management
        private async Task<bool> add_vehicle_async(Vehicle new_vehicle)
        {
            try
            {
                MySqlCommand cmd = new_vehicle.save_command();
                Console.WriteLine(reader_to_string(await query(cmd)));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> buy_vehicle_async(Vehicle new_vehicle)
        {
            if (await add_vehicle_async(new_vehicle))
            {
                Console.Write(await update_money(-new_vehicle.Price) ? "" : "Error : vehicle added, but failed update money.\n");
                return true;
            }
            else return false;
        }

        public async Task<float> get_vehicle_price(string license_plate)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT price FROM vehicle WHERE license_plate = '{license_plate}'");

                using (DbDataReader? rdr = await query(cmd))
                {
                    if (rdr == null) throw new Exception("reader is null");
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

        private async Task<bool> delete_vehicle_async(string license_plate)
        {
            MySqlCommand cmd = new MySqlCommand($"DELETE FROM vehicle WHERE license_plate = '{license_plate}'");

            try
            {
                using (DbDataReader? rdr = await query(cmd))
                {
                    if (rdr == null) throw new Exception("reader is null");
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

        #region Orders management
        public async Task<bool> new_order(Order new_order)
        {
            try
            {
                MySqlCommand cmd = new_order.save_command();
                Console.WriteLine(reader_to_string(await query(cmd)));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        #endregion
        #region Monitoring functions

        public async Task<List<Employee>?> get_employees_list_async()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM person WHERE user_type = 'EMPLOYEE'");

            try
            {
                return await Employee.from_reader_mulitple_async(await query(cmd));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<List<Client>?> get_clients_list_async()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM person WHERE user_type = 'CLIENT'");

            try
            {
                return await Client.from_reader_mulitple_async(await query(cmd));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<List<Vehicle>?> get_vehicles_list_async() //peut être agrémentée d'un filtre en arg (car / van / truck)
        {
            List<Vehicle> vehicles = new List<Vehicle>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM vehicle");

            try
            {
                return await Vehicle.from_reader_multiple_async(await query(cmd));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return vehicles;
        }

        public async Task<List<Order>?> get_orders_list_async()
        {
            List<Order> orders = new List<Order>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM orders");

            try
            {
                return await Order.from_reader_multiple_async(await query(cmd));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return orders;
        }

        #endregion

        #region Test zone


        #endregion
    }
}
