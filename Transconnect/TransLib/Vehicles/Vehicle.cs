﻿using MySql.Data.MySqlClient;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib.Vehicles
{
    public abstract class Vehicle
    {
        public string license_plate  { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public float price { get; set; }

        public Vehicle(string license_plate, string brand, string model, float price)
        {
            this.license_plate = license_plate;
            this.brand = brand;
            this.model = model;
            this.price = price;
        }

        public abstract MySqlCommand save_command();

        public override string ToString()
        {
            return $"{license_plate} {brand} {model}";
        }

        //Casts the object in reader into correct vehicle
        public async static Task<Vehicle> from_reader_async(DbDataReader? reader)
        {
            if (reader == null) throw new Exception("reader is null");

            using (reader)
            {
                await reader.ReadAsync();
                return cast_from_open_reader(reader);
            }
        }

        public async static Task<List<Vehicle>> from_reader_multiple_async(DbDataReader? reader)
        {
            if (reader == null) throw new Exception("reader is null");

            List<Vehicle> vehicles = new List<Vehicle>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    vehicles.Append(cast_from_open_reader(reader));
                }
            }
            return vehicles;
        }

        protected static Vehicle cast_from_open_reader(DbDataReader? reader)
        {
            if (reader == null) throw new Exception("reader is null");

            switch (reader.GetString("vehicle_type"))
            {
                case "CAR":
                    return new Car(reader.GetString("license_plate"), reader.GetString("brand"), reader.GetString("model"), reader.GetFloat("price"), reader.GetInt32("seats"));
                case "VAN":
                    return new Van(reader.GetString("license_plate"), reader.GetString("brand"), reader.GetString("model"), reader.GetFloat("price"), reader.GetString("usage"));
                case "TRUCK":
                    return new Truck(reader.GetString("license_plate"), reader.GetString("brand"), reader.GetString("model"), reader.GetFloat("price"), reader.GetInt32("volume"), reader.GetString("truck_type"));
                default:
                    throw new Exception("invalid vehicule_type");
            }
        }
    }
}
