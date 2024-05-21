using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransLib.Persons;

namespace TransLib.Vehicles
{
    /// <summary>
    /// A vehicle.
    /// </summary>
    public abstract class Vehicle
    {
        public string license_plate { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public int price { get; set; } //in cents

        public Vehicle(string license_plate, string brand, string model, int price)
        {
            this.license_plate = license_plate;
            this.brand = brand;
            this.model = model;
            this.price = price;
        }

        public abstract string vehicle_type { get; }
        public abstract Task create(AppConfig cfg);

        /// <summary>
        /// Casts the object in reader into correct vehicle
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async static Task<Vehicle?> from_reader(DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return Vehicle.cast_from_open_reader(reader, prefix);
            }
        }

        /// <summary>
        /// Returns a Vehicle list from reader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public async static Task<List<Vehicle>> from_reader_multiple(DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                List<Vehicle> vehicles = new List<Vehicle>();
                while (await reader.ReadAsync())
                {
                    Vehicle? vehicle = cast_from_open_reader(reader, prefix);
                    if (vehicle != null)
                        vehicles.Append(vehicle);
                }
                return vehicles;
            }
        }

        /// <summary>
        /// Casts any kind of Vehicle from open reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">invalid user_type</exception>
        public static Vehicle cast_from_open_reader(DbDataReader reader, string prefix = "")
        {
            switch (reader.GetString($"{prefix}vehicle_type"))
            {
                case "CAR":
                    return Car.cast_from_open_reader(reader, prefix);
                case "VAN":
                    return Van.cast_from_open_reader(reader, prefix);
                case "TRUCK":
                    return Truck.cast_from_open_reader(reader, prefix);
                default:
                    throw new Exception("invalid user_type");
            }
        }

        /// <summary>
        /// Generic method to update a field in the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cfg">The CFG.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        protected async Task update_field<T>(AppConfig cfg, string field, T value)
        {
            MySqlCommand cmd = new MySqlCommand(@$"
                UPDATE vehicle
                SET {field} = @value
                WHERE license_plate = @license_plate;
            ");
            cmd.Parameters.AddWithValue("@value", value);
            cmd.Parameters.AddWithValue("@license_plate", license_plate);
            await cfg.execute(cmd);
        }

        /// <summary>
        /// Sets the brand.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="brand">The brand.</param>
        public async Task set_brand(AppConfig cfg, string brand)
        {
            await update_field(cfg, "brand", brand);
            this.brand = brand;
        }

        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="model">The model.</param>
        public async Task set_model(AppConfig cfg, string model)
        {
            await update_field(cfg, "model", model);
            this.model = model;
        }

        /// <summary>
        /// Updates the price, input in cents
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public async Task set_price(AppConfig cfg, int price)
        {
            await update_field(cfg, "price", price);
            this.price = price;
        }

        /// <summary>
        /// Updates the price, input in euros
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public async Task set_price(AppConfig cfg, double price)
        {
            int price_in_cents = (int)double.Truncate(price);
            await update_field(cfg, "price", price_in_cents);
        }

        /// <summary>
        /// Returns a list of vehicles with optionnal ordering and filtering 
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="filter"></param>
        /// <param name="order_field"></param>
        /// <param name="order_dir"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static async Task<List<Vehicle>> list_vehicles(AppConfig cfg, string filter = "", string order_field = "license_plate", string order_dir = "ASC", int limit = 100, int offset = 0)
        {
            if (filter != "") filter = $"WHERE {filter}";
            MySqlCommand cmd = new MySqlCommand(@$"
                SELECT * FROM vehicle
                {filter}
                ORDER BY {order_field} {order_dir}
                LIMIT {limit}
                OFFSET {offset}        
            ;");
            using (DbDataReader reader = await cfg.query(cmd))
                return await from_reader_multiple(reader);
        }

        /// <summary>
        /// Gets a vehicle by license plate.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="license_plate">The license plate.</param>
        /// <returns></returns>
        public static async Task<Vehicle?> get_vehicle_by_license_plate(AppConfig cfg, string license_plate)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM vehicle WHERE license_plate = @license_plate");
            cmd.Parameters.AddWithValue("@license_plate", license_plate);

            DbDataReader reader = await cfg.query(cmd);
            return await from_reader(reader);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{license_plate} {brand} {model}";
        }

        /// <summary>
        /// Deletes the object from the database by deleting all informations excepted primary key. 
        /// Keep track of the vehicle is necessary while orders may be still linked to it.
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public async Task delete(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                DELETE FROM vehicle WHERE license_plate = @license_plate;
            ");
            cmd.Parameters.AddWithValue("@license_plate", license_plate);

            await cfg.execute(cmd);
        }
    }
}
