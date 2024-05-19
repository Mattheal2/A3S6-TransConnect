using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransLib.Persons;
using TransLib;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace TransLib.Miscellaneous
{
    public static class Other
    {
        public static async Task<Employee> fire_wheel(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT * FROM person
                WHERE user_type = 'EMPLOYEE' 
                ORDER BY RAND()
                LIMIT 1;
            ");

            DbDataReader reader = await cfg.query(cmd);
            Employee? rand_employee = await Employee.from_reader(reader);

            if(rand_employee == null) throw new Exception("No employees found");
            return rand_employee;
        }
    }
}
