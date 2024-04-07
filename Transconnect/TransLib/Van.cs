using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Van : Vehicle
    {
        protected string usage;

        public string Usage { get =>  usage; set => usage = value; }

        public Van(string license_plate, string brand, string model, float price, string usage) : base(license_plate, brand, model, price)
        {
            this.usage = usage;
        }
    }
}
