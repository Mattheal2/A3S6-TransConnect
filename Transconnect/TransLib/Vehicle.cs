using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public abstract class Vehicle
    {
        protected string brand;
        protected string model;
        protected string license_plate;

        public Vehicle(string brand, string model, string license_plate)
        {
            this.brand = brand;
            this.model = model;
            this.license_plate = license_plate;
        }
    }
}
