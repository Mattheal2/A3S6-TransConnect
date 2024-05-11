using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TransLib.Vehicles
{
    public class Van : Vehicle
    {
        public string usage { get; set; }


        public Van(string license_plate, string brand, string model, float price, string usage) : base(license_plate, brand, model, price)
        {
            this.usage = usage;
        }

        public override MySqlCommand save_command()
        {
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, usage) VALUES(@license_plate, @brand, @model, @price, @vehicle_type, @usage);");
            cmd.Parameters.AddWithValue("@license_plate", license_plate);
            cmd.Parameters.AddWithValue("@brand", brand);
            cmd.Parameters.AddWithValue("@model", model);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@vehicle_type", "VAN");
            cmd.Parameters.AddWithValue("@usage", usage);

            return cmd;
        }
    }
}
