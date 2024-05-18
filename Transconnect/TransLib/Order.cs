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

namespace TransLib
{
    public class Order
    {
        public static readonly float DEFAULT_PRICE_PER_KM = 0.8f;
        public enum OrderStatus
        {
            Pending,
            InProgress,
            Stuck,
            WaitingPayment,
            Closed
        }

        public int order_id { get; set; }
        public Client client { get; set; }
        public Vehicle vehicle { get; set; }
        public Employee driver { get; set; }
        //public Route route { get; set; }
        public long departure_date { get; set; }
        public long? arrival_date { get; set; }
        public string departure_city { get; set; }
        public string arrival_city { get; set; }
        public OrderStatus status { get; set; }
        public float price_per_km { get; set; }

        public Order(int order_id, Client client, Vehicle vehicle, long departure_time, string departure_city, string arrival_city)
        {
            this.order_id = order_id;
            this.client = client;
            this.vehicle = vehicle;
            this.departure_date = departure_time;
            this.departure_city = departure_city;
            this.arrival_city = arrival_city;

            //Route.RouteType type = vehicle is Truck ? Route.RouteType.Truck : Route.RouteType.Driving;
            //this.route = new Route(type, departure_city, arrival_city);
            //route.process_async().Wait();
            
            //this.arrival_date = departure_time.AddSeconds(route.get_duration());

            (this.driver, this.status) = find_driver();
            //this.price_per_km = 0.8f + ((DateTime.Now.Year - driver.hire_date.Year) / 4) * 0.1f; //Price per km increases by 0.1 every 4 years
        }

        public async Task create(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO orders (client_id, driver_id, vehicle_id, departure_date, departure_city, arrival_city)
                VALUES(@client_id, @driver_id, @vehicle_id, @departure_date, @departure_city, @arrival_city);
            ");
            cmd.Parameters.AddWithValue("@client_id", client.user_id);
            cmd.Parameters.AddWithValue("@driver_id", driver.user_id);
            cmd.Parameters.AddWithValue("@vehicle_id", vehicle.license_plate);
            cmd.Parameters.AddWithValue("@departure_date", departure_date);
            cmd.Parameters.AddWithValue("@arrival_date", arrival_date);
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

            await cfg.execute(cmd);
        }

        /// <summary>
        /// Finds the most appropriate driver for the order, returns null if no driver is available on this date
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static (Employee, OrderStatus) find_driver()
        {
            throw new NotImplementedException();
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

        public long get_duration()
        {
            throw new NotImplementedException();
        }

        public long get_arrival_time()
        {
            return departure_date + get_duration();
        }

        public int calculate_price()
        {
            throw new NotImplementedException();
            //return (int)(route.get_distance() * this.price_per_km);
        }

        public async static Task<int> estimate_price(string departure_city, string arrival_city)
        {
            return (int)(await calculate_distance(departure_city, arrival_city) * DEFAULT_PRICE_PER_KM);
        }

        protected static Order cast_from_open_reader(DbDataReader reader, string prefix = "")
        {
            return new Order(
                reader.GetInt32("order_id"),
                Client.cast_from_open_reader(reader),
                Vehicle.cast_from_open_reader(reader),
                reader.GetInt64("departure_date"),
                reader.GetString("departure_city"),
                reader.GetString("arrival_city")
            );
        }

        public async static Task<Order?> from_reader(DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return cast_from_open_reader(reader, prefix);
            }
        }

        public async static Task<List<Order>> from_reader_multiple(DbDataReader reader, string prefix = "")
        {
            List<Order> orders = new List<Order>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    orders.Append(cast_from_open_reader(reader));
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