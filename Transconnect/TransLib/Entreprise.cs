using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Entreprise
    {
        protected string name;
        protected string address;

        public Entreprise(string name, string address)
        {
            this.name = name;
            this.address = address;
        }

        /// Display the company informations
        public void display()
        {
            throw new NotImplementedException();
        }

        #region Staff management
        /// Adds a new employee and returns true if the operation was successful
        public bool hire_employee(Employee new_employee)
        {
            throw new NotImplementedException();
        }

        /// Fires a new employee and returns true if the operation was successful.
        /// Should return false if the employee doesn't exists.
        public bool fire_employee(string id)
        {
            throw new NotImplementedException();
        }

        /// Finds en employee using his first and last name and returns his ID.
        /// Returns an empty string if the employee doesn't exists.
        public string get_employee_id_by_name(string first_name, string last_name)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Vehicle management
        public bool buy_vehicle(Vehicle new_vehicle)
        {
            throw new NotImplementedException();
        }

        public bool sell_vehicle(string licence_plate)
        {
            throw new NotImplementedException();
        }

        public bool delete_vehicle(string licence_plate)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
