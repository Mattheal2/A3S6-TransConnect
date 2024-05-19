using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MySql.Data;
using System.Data;
using TransLib.Vehicles;
using TransLib.Persons;
using Org.BouncyCastle.Asn1.Mozilla;
using System.Reflection.Metadata.Ecma335;
using System.Net.NetworkInformation;
using Google.Protobuf.WellKnownTypes;

namespace TransLib
{
    public class Order
    {
        public static readonly int DEFAULT_PRICE_PER_KM = 80; //in cents
        public enum OrderStatus
        {
            Pending,
            InProgress,
            Stuck,
            WaitingPayment,
            Closed
        }

        public int order_id { get; set; }
        public int client_id { get; set; }
        public string vehicle_license_plate { get; set; }
        public int? driver_id { get; set; }
        //public Route route { get; set; }
        public long departure_time { get; set; }
        public long? arrival_time { get; set; }
        public string departure_city { get; set; }
        public string arrival_city { get; set; }
        public OrderStatus status { get; set; }
        public int price_per_km { get; set; }

        public Order(AppConfig cfg, int client_id, int driver_id, string vehicle_license_plate, long departure_time, string departure_city, string arrival_city)
        {
            this.order_id = order_id;
            this.client_id = client_id;
            this.vehicle_license_plate = vehicle_license_plate;
            this.departure_time = departure_time;
            this.departure_city = departure_city;
            this.arrival_city = arrival_city;

            //Route.RouteType type = vehicle is Truck ? Route.RouteType.Truck : Route.RouteType.Driving;
            //this.route = new Route(type, departure_city, arrival_city);
            //route.process_async().Wait();

            this.arrival_time = calculate_arrival_time();

            find_driver(cfg).Wait();
            this.price_per_km = 80 + ((DateTime.Now.Year - DateTimeOffset.FromUnixTimeSeconds(Employee.get_employee_by_id(cfg, driver_id).Result.hire_date).DateTime.Year) / 4) * 10; //Price per km increases by 10 every 4 years
        }

        public async Task create(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO orders (client_id, driver_id, vehicle_id, departure_date, departure_city, arrival_city)
                VALUES(@client_id, @driver_id, @vehicle_id, @departure_date, @departure_city, @arrival_city);
            ");
            cmd.Parameters.AddWithValue("@client_id", client_id);
            cmd.Parameters.AddWithValue("@driver_id", driver_id);
            cmd.Parameters.AddWithValue("@vehicle_id", vehicle_license_plate);
            cmd.Parameters.AddWithValue("@departure_date", departure_time);
            cmd.Parameters.AddWithValue("@arrival_date", calculate_arrival_time());
            cmd.Parameters.AddWithValue("@departure_city", departure_city);
            cmd.Parameters.AddWithValue("@arrival_city", arrival_city);

            await cfg.execute(cmd);
            order_id = (int)cmd.LastInsertedId;
        }

        public async Task delete(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                DELETE FROM orders
                WHERE order_id = @order_id;
            ");
            cmd.Parameters.AddWithValue("@order_id", order_id);

            await cfg.execute(cmd);
        }

        /// <summary>
        /// Finds the most appropriate driver for the order, returns null if no driver is available on this date
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task find_driver(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT driver_id 
                FROM orders 
                WHERE departure_date > @arrival_time AND arrival_date < @departure_time
                LIMIT 1;
            ");

            DbDataReader reader = await cfg.query(cmd);
            using (reader)
            {
                if (reader.HasRows)
                {
                    await reader.ReadAsync();
                    this.driver_id = reader.GetInt32("driver_id");
                    this.status = OrderStatus.WaitingPayment;
                }
                else
                {
                    this.status = OrderStatus.Stuck;
                    this.driver_id = null;
                }
            }
        }

        public long calculate_arrival_time()
        {
            return departure_time + 3600; //by default 1 hour ¯\_(ツ)_/¯
            //return departure_time + route.get_duration();
        }

        public int calculate_price()
        {
            throw new NotImplementedException();
            //return (int)(route.get_distance() * this.price_per_km);
        }

        /// <summary>
        /// Estimates the distance between two cities without creating an order
        /// </summary>
        /// <param name="address1"></param>
        /// <param name="address2"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async static Task<float> calculate_distance(string address1, string address2)
        {
            throw new NotImplementedException();
        }

        public async static Task<int> estimate_price(string departure_city, string arrival_city)
        {
            return (int)(await calculate_distance(departure_city, arrival_city) * DEFAULT_PRICE_PER_KM);
        }

        protected static Order cast_from_open_reader(AppConfig cfg, DbDataReader reader, string prefix = "")
        {
            return new Order(
                cfg,
                reader.GetInt32("client_id"),
                reader.GetInt32("driver_id"),
                reader.GetString($"vehicle_id"),
                reader.GetInt64("departure_date"),
                reader.GetString("departure_city"),
                reader.GetString("arrival_city")
            );
        }

        public async static Task<Order?> from_reader(AppConfig cfg, DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return cast_from_open_reader(cfg, reader, prefix);
            }
        }

        public async static Task<List<Order>> from_reader_multiple(AppConfig cfg, DbDataReader reader, string prefix = "")
        {
            List<Order> orders = new List<Order>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    orders.Append(cast_from_open_reader(cfg, reader));
                }
            }
            return orders;
        }

        //checks some things and updates the status if necessary
        public void update_status()
        {
            throw new NotImplementedException();
        }

    }
}