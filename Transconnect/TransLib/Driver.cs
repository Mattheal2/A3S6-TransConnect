using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Driver : Employee
    {
        string license_type;

        public string License_type
        { get  => license_type; set => license_type = value; }
        

        public Driver(string id_employee, string first_name, string last_name, string phone, string email, string address, DateTime birth_date, string position, float salary, DateTime hire_date, string license_type) : base(id_employee, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date)
        {
            this.license_type = license_type;
        }
    }
}
