using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MySql.Data;
using TransLib.Maps;
using System.Data;
using TransLib.Vehicles;
using TransLib.Persons;
using Org.BouncyCastle.Asn1.Mozilla;

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

        protected int order_id;
        protected Client client;
        protected Vehicle vehicle;
        protected Employee driver;
        protected Route route;
        protected DateTime departure_date;
        protected DateTime arrival_date;
        protected string departure_city;
        protected string arrival_city;
        protected OrderStatus status;
        public float price_per_km;

        public float Price_per_km { get => this.price_per_km; set => this.price_per_km = value; }
        public int Order_id { get => this.order_id; }
        public Client Client { get => this.client; }
        public Vehicle Vehicle { get => this.vehicle; }
        public Employee Driver { get => this.driver; }
        public Route Route { get => this.route; }
        public DateTime Departure_date { get => this.departure_date; }
        public DateTime DateTime { get => this.arrival_date; }
        public string Departure_city { get => this.departure_city; }
        public string Arrival_city { get => this.arrival_city; }
        public OrderStatus Status { get => this.status; set => this.status = value; }

        public Order(int order_id, Client client, Vehicle vehicle, DateTime departure_time, string departure_city, string arrival_city)
        {
            this.order_id = order_id;
            this.client = client;
            this.vehicle = vehicle;
            this.departure_date = departure_time;
            this.departure_city = departure_city;
            this.arrival_city = arrival_city;

            Route.RouteType type = vehicle is Truck ? Route.RouteType.Truck : Route.RouteType.Driving;
            this.route = new Route(type, departure_city, arrival_city);
            route.process_async().Wait();

            this.arrival_date = departure_time.AddSeconds(route.get_duration());

            (this.driver, this.status) = find_driver();
            this.price_per_km = 0.8f + ((DateTime.Now.Year - driver.hire_date.Year)/4) * 0.1f; //Price per km increases by 0.1 every 4 years
        }

        public MySqlCommand save_command()
        {
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO orders (client_id, driver_id, vehicle_id, departure_date, arrival_date, departure_city, arrival_city, order_status) VALUES(@client_id, @driver_id, @vehicle_id, @departure_date, @arrival_date, @departure_city, @arrival_city, @order_status);");
            cmd.Parameters.AddWithValue("@client_id", this.client.user_id);
            cmd.Parameters.AddWithValue("@driver_id", this.driver.user_id);
            cmd.Parameters.AddWithValue("@vehicle_id", this.vehicle.license_plate);
            cmd.Parameters.AddWithValue("@departure_date", this.departure_date);
            cmd.Parameters.AddWithValue("@arrival_date", this.arrival_date);
            cmd.Parameters.AddWithValue("@departure_city", this.departure_city);
            cmd.Parameters.AddWithValue("@arrival_city", this.arrival_city);

            return cmd;
        }

        //finds the most appropriate driver for the order, returns null if no driver is available on this date
        //=> Calendar class to check driver availability
        private static (Employee, OrderStatus) find_driver()
        {
            throw new NotImplementedException();
        }

        //Estimates the distance between two cities without creating an order
        public async static Task<float> calculate_distance(string address1, string address2, Route.RouteType type = Route.RouteType.Driving)
        {
            Route route = new Route(type, address1, address2);
            await route.process_async();
            return route.get_distance();
        }

        public int get_duration()
        {
            return route.get_duration();
        }

        public DateTime get_arrival_time()
        {
            return departure_date.AddSeconds(get_duration());
        }

        public int calculate_price()
        {
            return (int)(route.get_distance() * this.price_per_km);
        }

        public async static Task<int> estimate_price(string departure_city, string arrival_city)
        {
            return (int)(await calculate_distance(departure_city, arrival_city) * DEFAULT_PRICE_PER_KM);
        }

        protected async static Task<Order> cast_from_open_reader(DbDataReader? reader)
        {
            if (reader == null) throw new Exception("reader is null");

            return new Order(reader.GetInt32("order_id"), await Client.from_reader_async(reader), await Vehicle.from_reader_async(reader), reader.GetDateTime("departure_date"), reader.GetString("departure_city"), reader.GetString("arrival_city"));
        }

        public async static Task<Order> from_reader_async(DbDataReader? reader)
        {
            if (reader == null) throw new Exception("reader is null");
            await reader.ReadAsync();
            return await cast_from_open_reader(reader);
        }

        public async static Task<List<Order>> from_reader_multiple_async(DbDataReader? reader)
        {
            List<Order> orders = new List<Order>();
            if (reader == null) throw new Exception("reader is null");
            while (await reader.ReadAsync())
            {
                orders.Append(await cast_from_open_reader(reader));
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