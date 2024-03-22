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

        public Van(string brand, string model, string license_plate, string usage) : base(brand, model, license_plate)
        {
            this.usage = usage;
        }
    }
}
