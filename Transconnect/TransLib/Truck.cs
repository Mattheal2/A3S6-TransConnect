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

        public Truck(string vehicle_id, string brand, string model, string license_plate, int volume) : base(vehicle_id, brand, model, license_plate)
        {
            this.volume = volume;
        }
    }
}
