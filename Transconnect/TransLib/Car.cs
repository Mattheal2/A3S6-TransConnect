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

        public Car(string vehicle_id, string brand, string model, string license_plate, int seats) : base(vehicle_id, brand, model, license_plate)
        {
            this.seats = seats;
        }
    }
}
