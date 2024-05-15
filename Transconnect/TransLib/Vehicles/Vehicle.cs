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
        public abstract Task delete(AppConfig cfg);

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
                    Vehicle? person = cast_from_open_reader(reader, prefix);
                    if (person != null)
                        vehicles.Append(person);
                }
                return vehicles;
            }
        }

        protected static Vehicle cast_from_open_reader(DbDataReader reader, string prefix = "")
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

        public async Task set_brand_and_model(AppConfig cfg, string brand, string model)
        {
            await update_field(cfg, "brand", brand);
            this.brand = brand;

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

        public override string ToString()
        {
            return $"{license_plate} {brand} {model}";
        }
    }
}
