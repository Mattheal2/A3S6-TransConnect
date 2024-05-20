using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TransLib.Persons;
using TransLib.Vehicles;

namespace TransLib
{
    public class Company
    {
        protected string name;
        protected string address;

        public Company(string name, string address)
        {
            this.name = name;
            this.address = address;
        }   
    }
}
