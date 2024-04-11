﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace TransLib
{
    public class Truck : Vehicle
    {
        protected int volume;
        protected string truck_type;

        public int VOLUME { get => volume; }
        public string TRUCK_TYPE { get => truck_type; }

        public Truck(string license_plate, string brand, string model, float price, int volume, string truck_type) : base(license_plate, brand, model, price)
        {
            this.volume = volume;
            this.truck_type = truck_type;
        }

        public override MySqlCommand save_command()
        {
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, volume, truck_type) VALUES(@license_plate, @brand, @model, @price, @vehicle_type, @volume, @truck_type);");
            cmd.Parameters.AddWithValue("@license_plate", this.license_plate);
            cmd.Parameters.AddWithValue("@brand", this.brand);
            cmd.Parameters.AddWithValue("@model", this.model);
            cmd.Parameters.AddWithValue("@price", this.price);
            cmd.Parameters.AddWithValue("@vehicle_type", "TRUCK");
            cmd.Parameters.AddWithValue("@volume", this.volume);
            cmd.Parameters.AddWithValue("@truck_type", this.truck_type);

            return cmd;
        }
    }
}
