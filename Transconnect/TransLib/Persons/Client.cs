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
        public int total_spent { get; private set; }
        public Client(
            int user_id, string first_name, string last_name, string phone, string email, string address, string city, long birth_date, string? password_hash, int total_spent
        ) : base(user_id, first_name, last_name, phone, email, address, city, birth_date, password_hash)
        {
            this.total_spent = total_spent;
        }

        public async Task Create(AppConfig cfg) {
            MySqlCommand cmd = new MySqlCommand(
                @"INSERT INTO person (user_type, first_name, last_name, phone, email, address, city, birth_date) 
                VALUES(@user_type, @first_name, @last_name, @phone, @email, @address, @city, @birth_date);");
            
            cmd.Parameters.AddWithValue("@user_type", user_type);
            cmd.Parameters.AddWithValue("@first_name", first_name);
            cmd.Parameters.AddWithValue("@last_name", last_name);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Parameters.AddWithValue("@city", city);
            cmd.Parameters.AddWithValue("@birth_date", birth_date);

            await cfg.execute(cmd);
            user_id = (int)cmd.LastInsertedId;
        }

        public override string user_type { get; } = "CLIENT";

        public async static new Task<Client?> from_reader_async(DbDataReader reader)
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return cast_from_open_reader(reader);
            }
        }

        /// Returns an Employee list from a reader.
        public async static new Task<List<Client>> from_reader_multiple(DbDataReader reader)
        {
            using (reader)
            {
                List<Client> persons = new List<Client>();
                while (await reader.ReadAsync())
                {
                    Client? person = cast_from_open_reader(reader);
                    if (person != null)
                        persons.Append(person);
                }
                return persons;
            }
        }

        protected static new Client cast_from_open_reader(DbDataReader reader)
        {
            if (reader.GetString("user_type") == "CLIENT") {
                return new Client(
                    reader.GetInt32("user_id"),
                    reader.GetString("first_name"),
                    reader.GetString("last_name"),
                    reader.GetString("phone"),
                    reader.GetString("email"),
                    reader.GetString("address"),
                    reader.GetString("city"),
                    reader.GetInt64("birth_date"),
                    reader.GetString("password_hash"),
                    reader.GetInt32("total_spent")
                );
            } else throw new Exception("invalid user_type");
        }

        public string? ValidateClient()
        {
            long current_time = DateTimeOffset.Now.ToUnixTimeSeconds();

            if (current_time < birth_date)
            {
                return "Birth date is in the future";
            }

            return null;
        }

        public static async Task<List<Client>> list_clients(AppConfig cfg, string order_field, string order_dir, int limit, int offset)
        {
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM person WHERE user_type = 'CLIENT' ORDER BY {order_field} {order_dir} LIMIT {limit} OFFSET {offset};");
            using (DbDataReader reader = await cfg.query(cmd))
            {
                return await from_reader_multiple(reader);
            }
        }

        public async Task delete(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand("DELETE FROM person WHERE user_id = @user_id;");
            cmd.Parameters.AddWithValue("@user_id", user_id);
            await cfg.execute(cmd);
        }
    }
}
