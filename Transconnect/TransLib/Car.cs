using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Car : Vehicle
    {
        protected int seats;

        public Car(string brand, string model, string license_plate, int seats) : base(brand, model, license_plate)
        {
            this.seats = seats;
        }
    }
}
