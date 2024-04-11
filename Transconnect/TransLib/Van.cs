using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TransLib
{
    public class Van : Vehicle
    {
        protected string usage;

        public string Usage { get =>  usage; set => usage = value; }

        public Van(string license_plate, string brand, string model, float price, string usage) : base(license_plate, brand, model, price)
        {
            this.usage = usage;
        }
        
        public override MySqlCommand save_command()
        {
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, usage) VALUES(@license_plate, @brand, @model, @price, @vehicle_type, @usage);");
            cmd.Parameters.AddWithValue("@license_plate", this.license_plate);
            cmd.Parameters.AddWithValue("@brand", this.brand);
            cmd.Parameters.AddWithValue("@model", this.model);
            cmd.Parameters.AddWithValue("@price", this.price);
            cmd.Parameters.AddWithValue("@vehicle_type", "VAN");
            cmd.Parameters.AddWithValue("@usage", this.usage);

            return cmd;
        }
    }
}
