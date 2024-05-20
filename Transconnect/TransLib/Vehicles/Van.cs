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
    public class Van : Vehicle
    {
        public string usage { get; set; }


        public Van(string license_plate, string brand, string model, int price, string usage) : base(license_plate, brand, model, price)
        {
            this.usage = usage;
        }

        public override string vehicle_type { get; } = "VAN";

        /// <summary>
        /// Saves the instance in the database
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public async override Task create(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, usage) 
                VALUES(@license_plate, @brand, @model, @price, @vehicle_type, @usage);
            ");
            cmd.Parameters.AddWithValue("@license_plate", license_plate);
            cmd.Parameters.AddWithValue("@brand", brand);
            cmd.Parameters.AddWithValue("@model", model);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@vehicle_type", vehicle_type);
            cmd.Parameters.AddWithValue("@usage", usage);

            await cfg.execute(cmd);
        }
        
        public static new Van cast_from_open_reader(DbDataReader reader, string prefix = "")
        {
            if (reader.GetString($"{prefix}vehicle_type") == "VAN")
            {
                return new Van(reader.GetString("license_plate"),
                    reader.GetString("brand"),
                    reader.GetString("model"),
                    reader.GetInt32("price"),
                    reader.GetString("usage")
                    );
            }
            else throw new Exception("invalid vehicle_type");
        }

        public async Task set_usage(AppConfig cfg, string usage)
        {
            await update_field(cfg, "usage", usage);
            this.usage = usage;
        }
    }
}
