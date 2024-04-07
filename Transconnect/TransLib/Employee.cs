using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Employee : Person
    {
        protected string id_employee;
        protected string position;
        protected float salary;
        protected DateTime hire_date;

        public string Id_employee { get => id_employee;}
        public string Position { get => position; set => position = value; }
        public float Salary { get => salary; set => salary = value; }
        public DateTime Hire_date { get => hire_date; }

        public Employee(string id_employee, string first_name, string last_name, string phone, string email, string address, DateTime birth_date, string position, float salary, DateTime hire_date) : base(first_name, last_name, phone, email, address, birth_date)
        {
            this.id_employee = id_employee;
            this.position = position;
            this.salary = salary;
            this.hire_date = hire_date;
        }

        public DateTime[] get_schedule()
        {
            throw new NotImplementedException();
        }
    }
}
