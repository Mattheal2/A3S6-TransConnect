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

        public Truck(string brand, string model, string license_plate, int volume) : base(brand, model, license_plate)
        {
            this.volume = volume;
        }
    }
}
