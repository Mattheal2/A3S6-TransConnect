using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TransLib.Miscellaneous
{
    public class Stats
    {
        /// <summary>
        /// struct for the response of deliveries_by_driver
        /// </summary>
        public struct DeliveriesByDriverResponse
        {
            public int driver_id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public int count { get; set; }
        }

        /// <summary>
        /// Returns the number of deliveries made by each driver.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No driver found</exception>
        public async static Task<List<DeliveriesByDriverResponse>> deliveries_by_driver(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT person.user_id, person.first_name, person.last_name, COUNT(orders.order_id) AS deliveries_count
                FROM person
                LEFT JOIN orders ON person.user_id = orders.driver_id
                WHERE person.user_type = 'EMPLOYEE'
	                AND LOWER(person.position) = 'driver'
                GROUP BY person.user_id, person.first_name, person.last_name;
            ");
            DbDataReader reader = await cfg.query(cmd);
            List<DeliveriesByDriverResponse> result = new List<DeliveriesByDriverResponse>();

            using (reader)
            {
                if (!reader.HasRows) throw new Exception("No driver found");
                while (await reader.ReadAsync())
                {
                    result.Append(new DeliveriesByDriverResponse
                    {
                        driver_id = reader.GetInt32("user_id"),
                        first_name = reader.GetString("first_name"),
                        last_name = reader.GetString("last_name"),
                        count = reader.GetInt32("deliveries_count")
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the deliveries between the specified time frame.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public async static Task<List<Order>> deliveries_between(AppConfig cfg, long start, long end)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT *
                FROM orders
                WHERE departure_time >= @departure_time AND departure_time <= @arrival_time;
            ");
            cmd.Parameters.AddWithValue("@departure_time", start);
            cmd.Parameters.AddWithValue("@arrival_time", end);

            DbDataReader reader = await cfg.query(cmd);
            return await Order.from_reader_multiple(cfg,reader);

        }

        /// <summary>
        /// Returns the average total spent by a client.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No client found</exception>
        public async static Task<int> average_total_spent(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"SELECT AVG(person.total_spent) AS average_client_account
                FROM person
                WHERE person.user_type = 'CLIENT';
            ");

            DbDataReader reader = await cfg.query(cmd);
            using (reader)
            {
                if (!reader.HasRows) throw new Exception("No client found");
                await reader.ReadAsync();
                return reader.GetInt32("average_client_account");
            }
        }

        /// <summary>
        /// Returns the average command price.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No command found</exception>
        public async static Task<int> average_command_price(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT AVG(orders.total_price) AS average_command_price
                FROM orders;
            ");

            DbDataReader reader = await cfg.query(cmd);
            using (reader)
            {
                if (!reader.HasRows) throw new Exception("No command found");
                await reader.ReadAsync();
                return reader.GetInt32("average_command_price");
            }
        }

        public async static Task<int> total_deliveries_count(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT COUNT(orders.order_id) AS total
                FROM orders;
            ");
            DbDataReader reader = await cfg.query(cmd);
            using (reader)
            {
                await reader.ReadAsync();
                return reader.GetInt32("total");
            }
        }
    }
}
