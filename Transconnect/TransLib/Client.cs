using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Client : Person
    {
        protected string id_client;
        
        public Client(string first_name, string last_name, string phone, string email, string address, string birth_date, string id_client) : base(first_name, last_name, phone, email, address, birth_date)
        {
            this.id_client = id_client;
        }

        public bool new_order(Order new_order)
        {
            throw new NotImplementedException();
        }
    }
}
