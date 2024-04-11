using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public abstract class Vehicle
    {
        protected string license_plate;
        protected string brand;
        protected string model;
        protected float price;

        public string LICENSE_PLATE { get => license_plate; }
        public string MODEL { get => model; }
        public string BRAND { get => brand; }
        public float Price { get => price; set => price = value; }

        public Vehicle(string license_plate, string brand, string model, float price)
        {
            this.license_plate = license_plate;
            this.brand = brand;
            this.model = model;
            this.price = price;
        }

        public abstract MySqlCommand save_command();
        public 
    }
}
