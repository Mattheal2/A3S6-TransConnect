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

        public Driver(string first_name, string last_name, string phone, string email, string address, string birth_date, string id_employee, string position, string salary, string hire_date, string license_type) : base(first_name, last_name, phone, email, address, birth_date, id_employee, position, salary, hire_date)
        {
            this.license_type = license_type;
        }
    }
}
