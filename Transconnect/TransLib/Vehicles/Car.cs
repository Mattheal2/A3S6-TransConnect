using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MySql.Data.MySqlClient;

namespace TransLib.Vehicles
{
    public class Car : Vehicle
    {
        public int seats { get; set; }


        public Car(string license_plate, string brand, string model, float price, int seats) : base(license_plate, brand, model, price)
        {
            this.seats = seats;
        }

        public override string vehicle_type { get; } = "CAR";

        /// <summary>
        /// Saves the instance in the database
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public async override Task create(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) 
                VALUES(@license_plate, @brand, @model, @price, @vehicle_type, @seats);
            ");
            cmd.Parameters.AddWithValue("@license_plate", license_plate);
            cmd.Parameters.AddWithValue("@brand", brand);
            cmd.Parameters.AddWithValue("@model", model);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@vehicle_type", vehicle_type);
            cmd.Parameters.AddWithValue("@seats", seats);

            await cfg.execute(cmd);
        }

        /// <summary>
        /// Deletes the object from the database by deleting all informations excepted primary key. 
        /// Keep track of the vehicle is necessary while orders may be still linked to it.
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public async override Task delete(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                UPDATE vehicle 
                SET deleted = TRUE, brand = '', model = '', price = 0, seats = 0
                WHERE license_plate = @license_plate;
            ");
            cmd.Parameters.AddWithValue("@license_plate", license_plate);

            await cfg.execute(cmd);
        }

        public static new Car cast_from_open_reader(DbDataReader reader, string prefix = "")
        {
            if (reader.GetString($"{prefix}vehicle_type") == "CAR")
            {
                return new Car(
                    reader.GetString("license_plate"),
                    reader.GetString("brand"),
                    reader.GetString("model"),
                    reader.GetFloat("price"),
                    reader.GetInt32("seats")
                 );
            }
            else throw new Exception("invalid vehicle_type");
        }

        public async Task set_seats(AppConfig cfg, int seats)
        {
            await update_field(cfg, "seats", seats);
            this.seats = seats;
        }
    }
}
