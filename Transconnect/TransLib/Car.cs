using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TransLib
{
    public class Car : Vehicle
    {
        protected int seats;

        public int SEATS { get => seats; }

        public Car(string license_plate, string brand, string model, float price, int seats) : base(license_plate, brand, model, price)
        {
            this.seats = seats;
        }

        public override MySqlCommand save_command()
        {
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES(@license_plate, @brand, @model, @price, @vehicle_type, @seats);");
            cmd.Parameters.AddWithValue("@license_plate", this.license_plate);
            cmd.Parameters.AddWithValue("@brand", this.brand);
            cmd.Parameters.AddWithValue("@model", this.model);
            cmd.Parameters.AddWithValue("@price", this.price);
            cmd.Parameters.AddWithValue("@vehicle_type", "CAR");
            cmd.Parameters.AddWithValue("@seats", this.seats);

            return cmd;
        }
    }
}
