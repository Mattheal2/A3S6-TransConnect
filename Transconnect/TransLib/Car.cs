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

        public int SEATS { get => seats; }

        public Car(string license_plate, string brand, string model, int seats) : base(license_plate, brand, model)
        {
            this.seats = seats;
        }
    }
}
