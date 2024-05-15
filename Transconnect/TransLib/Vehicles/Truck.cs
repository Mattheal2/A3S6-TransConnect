using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TransLib.Vehicles
{
    public class Truck : Vehicle
    {
        public int volume { get; set; }
        public string truck_type { get; set; }

        public Truck(string license_plate, string brand, string model, float price, int volume, string truck_type) : base(license_plate, brand, model, price)
        {
            this.volume = volume;
            this.truck_type = truck_type;
        }

        public override string vehicle_type { get; } = "TRUCK";

        /// <summary>
        /// Saves the instance in the database
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public async override Task create(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, volume, truck_type) 
                VALUES(@license_plate, @brand, @model, @price, @vehicle_type, @volume, @truck_type);
            ");
            cmd.Parameters.AddWithValue("@license_plate", license_plate);
            cmd.Parameters.AddWithValue("@brand", brand);
            cmd.Parameters.AddWithValue("@model", model);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@vehicle_type", vehicle_type);
            cmd.Parameters.AddWithValue("@volume", volume);
            cmd.Parameters.AddWithValue("@truck_type", truck_type);

            await cfg.execute(cmd);
        }

        /// <summary>
        /// Deletes the object from the database by deleting all informations excepted primary key. 
        /// Keep track of the vehicle is necessary while orders may be still linked to it.
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async override Task delete(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                UPDATE vehicle 
                SET deleted = TRUE, brand = '', model = '', price = 0, volume = 0
                WHERE license_plate = @license_plate;
            ");
            cmd.Parameters.AddWithValue("@license_plate", license_plate);

            await cfg.execute(cmd);
        }

        protected static new Truck cast_from_open_reader(DbDataReader reader, string prefix = "")
        {
            if (reader.GetString($"{prefix}vehicle_type") == "TRUCK")
            {
                return new Truck(reader.GetString("license_plate"),
                    reader.GetString("brand"),
                    reader.GetString("model"),
                    reader.GetFloat("price"),
                    reader.GetInt32("volume"),
                    reader.GetString("truck_type")
                    );
            }
            else throw new Exception("invalid vehicle_type");
        }

        public async Task set_volume(AppConfig cfg, int volume)
        {
            await update_field(cfg, "volume", volume);
            this.volume = volume;
        }

        public async Task set_truck_type(AppConfig cfg, string truck_type)
        {
            await update_field(cfg, "truck_type", truck_type);
            this.truck_type = truck_type;s
        }
    }
}
