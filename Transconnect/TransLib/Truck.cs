using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Truck : Vehicle
    {
        protected int volume;
        protected string truck_type;

        public int VOLUME { get => volume; }
        public string TRUCK_TYPE { get => truck_type; }

        public Truck(string license_plate, string brand, string model, int volume, string truck_type) : base(license_plate, brand, model)
        {
            this.volume = volume;
            this.truck_type = truck_type;
        }
    }
}
