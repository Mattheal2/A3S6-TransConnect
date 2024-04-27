using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib.Persons
{
    public class Client : Person
    {
        public Client(int user_id, string first_name, string last_name, string phone, string email, string address, DateTime birth_date) : base(user_id, first_name, last_name, phone, email, address, birth_date)
        {
        }

        public override MySqlCommand save_command()
        {
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO person (user_id, user_type, first_name, last_name, phone, email, address, birth_date) VALUES(@user_id, @user_type, @first_name, @last_name, @phone, @email, @address, @birth_date);");
            cmd.Parameters.AddWithValue("@user_id", USER_ID);
            cmd.Parameters.AddWithValue("@user_type", "CLIENT");
            cmd.Parameters.AddWithValue("@first_name", FIRST_NAME);
            cmd.Parameters.AddWithValue("@last_name", LAST_NAME);
            cmd.Parameters.AddWithValue("@phone", PHONE);
            cmd.Parameters.AddWithValue("@email", EMAIL);
            cmd.Parameters.AddWithValue("@address", ADDRESS);
            cmd.Parameters.AddWithValue("@birth_date", BIRTH_DATE);

            return cmd;
        }

        public async static new Task<Client> from_reader_async(DbDataReader? reader)
        {
            using (reader)
            {
                if (reader == null) throw new Exception("reader is null");

                await reader.ReadAsync();
                return cast_from_open_reader(reader);
            }
        }

        /// Returns an Employee list from a reader.
        public async static new Task<List<Client>> from_reader_mulitple_async(DbDataReader? reader)
        {
            using (reader)
            {
                if (reader == null) throw new Exception("reader is null");

                List<Client> clients = new List<Client>();
                while (await reader.ReadAsync())
                {
                    clients.Append(cast_from_open_reader(reader));
                }
                return clients;
            }
        }

        protected static new Client cast_from_open_reader(DbDataReader? reader)
        {
            if (reader == null) throw new Exception("reader is null");

            if (!reader.IsClosed)
            {
                if (reader.GetString("user_type") == "CLIENT")
                {

                    return new Client(reader.GetInt32("user_id"), reader.GetString("first_name"), reader.GetString("last_name"), reader.GetString("phone"), reader.GetString("email"), reader.GetString("address"), reader.GetDateTime("birth_date"));
                }
                else throw new Exception("invalid user_type");
            }
            else
            {
                throw new Exception("unable to read closed reader");
            }

        }

        public bool new_order(Order new_order)
        {
            throw new NotImplementedException();
        }
    }
}
