﻿using MySql.Data.MySqlClient;
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
        public int total_spent { get; private set; } // in cents
        public Client(
            int user_id, string first_name, string last_name, string phone, string email, string address, string city, long birth_date, bool deleted, string? password_hash, int total_spent
        ) : base(user_id, first_name, last_name, phone, email, address, city, birth_date, deleted, password_hash)
        {
            this.total_spent = total_spent;
        }

        public override async Task create(AppConfig cfg) {
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

        public async static new Task<Client?> from_reader_async(DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return cast_from_open_reader(reader, prefix);
            }
        }

        /// Returns an Employee list from a reader.
        public async static new Task<List<Client>> from_reader_multiple(DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                List<Client> persons = new List<Client>();
                while (await reader.ReadAsync())
                {
                    Client? person = cast_from_open_reader(reader, prefix);
                    if (person != null)
                        persons.Append(person);
                }
                return persons;
            }
        }

        protected static new Client cast_from_open_reader(DbDataReader reader, string prefix = "")
        {
            if (reader.GetString($"{prefix}user_type") == "CLIENT") {
                return new Client(
                    reader.GetInt32($"{prefix}user_id"),
                    reader.GetString($"{prefix}first_name"),
                    reader.GetString($"{prefix}last_name"),
                    reader.GetString($"{prefix}phone"),
                    reader.GetString($"{prefix}email"),
                    reader.GetString($"{prefix}address"),
                    reader.GetString($"{prefix}city"),
                    reader.GetInt64($"{prefix}birth_date"),
                    reader.GetBoolean($"{prefix}deleted"),
                    reader.GetString($"{prefix}password_hash"),
                    reader.GetInt32($"{prefix}total_spent")
                );
            } else throw new Exception("invalid user_type");
        }

        public string? validate()
        {
            long current_time = DateTimeOffset.Now.ToUnixTimeSeconds();

            if (current_time < birth_date) return "Birth date is in the future";

            return null;
        }

        public static async Task<List<Client>> list_clients(AppConfig cfg, string order_field, string order_dir, int limit, int offset)
        {
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM person WHERE user_type = 'CLIENT' AND NOT deleted ORDER BY {order_field} {order_dir} LIMIT {limit} OFFSET {offset};");
            using (DbDataReader reader = await cfg.query(cmd))
            {
                return await from_reader_multiple(reader);
            }
        }

        public override async Task delete(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                UPDATE person
                SET 
                    deleted = true, first_name = '', last_name = '', phone = '', email = '', address = '', city = '', birth_date = 0
                WHERE user_id = @user_id");
            cmd.Parameters.AddWithValue("@user_id", user_id);
            await cfg.query(cmd);
            this.deleted = true;
        }
    }
}
