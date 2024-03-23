using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public abstract class Vehicle
    {
        protected string vehicle_id;
        protected string brand;
        protected string model;
        protected string license_plate;

        public Vehicle(string vehicle_id, string brand, string model, string license_plate)
        {
            this.vehicle_id = vehicle_id;
            this.brand = brand;
            this.model = model;
            this.license_plate = license_plate;
        }
    }
}
