using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Client : Person
    {
        protected string client_id;
        
        public Client(string first_name, string last_name, string phone, string email, string address, DateTime birth_date, string id_client) : base(first_name, last_name, phone, email, address, birth_date)
        {
            this.client_id = id_client;
        }

        public override MySqlCommand save_command()
        {
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO person (user_id, user_type, first_name, last_name, phone, email, address, birth_date) VALUES(@user_id, @user_type, @first_name, @last_name, @phone, @email, @address, @birth_date);");
            cmd.Parameters.AddWithValue("@user_id", this.client_id);
            cmd.Parameters.AddWithValue("@user_type", "CLIENT");
            cmd.Parameters.AddWithValue("@first_name", this.first_name);
            cmd.Parameters.AddWithValue("@last_name", this.last_name);
            cmd.Parameters.AddWithValue("@phone", this.phone);
            cmd.Parameters.AddWithValue("@email", this.email);
            cmd.Parameters.AddWithValue("@address", this.address);
            cmd.Parameters.AddWithValue("@birth_date", this.birth_date);

            return cmd;
        }

        public static Client client_from_reader(DbDataReader reader)
        {

        }

        public bool new_order(Order new_order)
        {
            throw new NotImplementedException();
        }
    }
}
