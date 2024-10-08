﻿using MySql.Data.MySqlClient;
using Mysqlx.Datatypes;
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
            int user_id, string first_name, string last_name, string phone, string email, string address, string city, long birth_time, string? password_hash, int total_spent
        ) : base(user_id, first_name, last_name, phone, email, address, city, birth_time, password_hash)
        {
            this.total_spent = total_spent;
        }

        /// <summary>
        /// Creates the specified client in the database.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        public override async Task create(AppConfig cfg) {
            MySqlCommand cmd = new MySqlCommand(
                @"INSERT INTO person (user_type, first_name, last_name, phone, email, address, city, birth_time) 
                VALUES(@user_type, @first_name, @last_name, @phone, @email, @address, @city, @birth_time);");
            
            cmd.Parameters.AddWithValue("@user_type", user_type);
            cmd.Parameters.AddWithValue("@first_name", first_name);
            cmd.Parameters.AddWithValue("@last_name", last_name);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Parameters.AddWithValue("@city", city);
            cmd.Parameters.AddWithValue("@birth_time", birth_time);

            await cfg.execute(cmd);
            user_id = (int)cmd.LastInsertedId;
        }

        public override string user_type { get; } = "CLIENT";

        /// <summary>
        /// Returns a Client from a reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public async static new Task<Client?> from_reader(DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return cast_from_open_reader(reader, prefix);
            }
        }

        /// <summary>
        /// Returns an Employee list from a reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Casts Client from open reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">invalid user_type</exception>
        public static new Client cast_from_open_reader(DbDataReader reader, string prefix = "")
        {
            if (reader.GetString($"{prefix}user_type") == "CLIENT") {
                string? pass = null;
                if (!reader.IsDBNull($"{prefix}password_hash")) pass = reader.GetString($"{prefix}password_hash");

                return new Client(
                    reader.GetInt32($"{prefix}user_id"),
                    reader.GetString($"{prefix}first_name"),
                    reader.GetString($"{prefix}last_name"),
                    reader.GetString($"{prefix}phone"),
                    reader.GetString($"{prefix}email"),
                    reader.GetString($"{prefix}address"),
                    reader.GetString($"{prefix}city"),
                    reader.GetInt64($"{prefix}birth_time"),
                    pass,
                    reader.GetInt32($"{prefix}total_spent")
                );
            } else throw new Exception("invalid user_type");
        }

        /// <summary>
        /// Validates everything is ok
        /// </summary>
        /// <returns></returns>
        public string? validate()
        {
            long current_time = DateTimeOffset.Now.ToUnixTimeSeconds();

            if (current_time < birth_time) return "Birth date is in the future";

            return null;
        }

        /// <summary>
        /// Lists the clients.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="order_field">The order field.</param>
        /// <param name="order_dir">The order dir.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static async Task<List<Client>> list_clients(AppConfig cfg, string order_field, string order_dir)
        {
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM person WHERE user_type = 'CLIENT' ORDER BY {order_field} {order_dir};");
            using (DbDataReader reader = await cfg.query(cmd))
            {
                return await from_reader_multiple(reader);
            }
        }
    }
}
