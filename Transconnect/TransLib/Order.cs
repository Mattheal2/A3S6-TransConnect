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
using TransLib.Itinerary;
using System.Runtime.Serialization;
//using Google.Protobuf.WellKnownTypes;
using System.Security.Policy;

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
        public string? vehicle_license_plate { get; set; }
        public int? driver_id { get; set; }
        public long departure_time { get; set; }
        public long? arrival_time { get; set; }
        public string departure_city { get; set; }
        public string arrival_city { get; set; }
        public OrderStatus status { get; set; }
        public int price_per_km { get; set; }
        public int total_price { get; set; }
        public Itinerary.Itinerary? route { get; set; }

        /// <summary>
        /// New order constructor
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="departure_time"></param>
        /// <param name="departure_city"></param>
        /// <param name="arrival_city"></param>
        public Order(AppConfig cfg, int client_id, long departure_time, string departure_city, string arrival_city)
        {
            this.client_id = client_id;
            this.departure_time = departure_time;
            this.departure_city = departure_city;
            this.arrival_city = arrival_city;

            this.route = build_itinerary(cfg);
            this.arrival_time = calculate_arrival_time();
            this.total_price = calculate_price();

        }

        /// <summary>
        /// Existing order constructor
        /// </summary>
        /// <param name="order_id"></param>
        /// <param name="client_id"></param>
        /// <param name="vehicle_license_plate"></param>
        /// <param name="driver_id"></param>
        /// <param name="departure_time"></param>
        /// <param name="arrival_time"></param>
        /// <param name="departure_city"></param>
        /// <param name="arrival_city"></param>
        /// <param name="price_per_km"></param>
        /// <param name="status"></param>
        public Order(AppConfig cfg, int order_id, int client_id, string vehicle_license_plate, int? driver_id, long departure_time, long? arrival_time, string departure_city, string arrival_city, int price_per_km, int total_price, string status)
        {
            this.order_id = order_id;
            this.client_id = client_id;
            this.vehicle_license_plate = vehicle_license_plate;
            this.driver_id = driver_id;
            this.departure_time = departure_time;
            this.arrival_time = arrival_time;
            this.departure_city = departure_city;
            this.arrival_city = arrival_city;
            this.status = (OrderStatus)Enum.Parse(typeof(OrderStatus), status);
            this.price_per_km = price_per_km;
            this.total_price = total_price;

            try { this.route = build_itinerary(cfg); }
            catch (Exception e) { Console.WriteLine(e.Message); this.route = null; }
        }

        /// <summary>
        /// Creates the order in the database.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        public async Task create(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO orders (client_id, driver_id, vehicle_id, departure_time, arrival_time, departure_city, arrival_city, price_per_km, total_price, order_status)
                VALUES(@client_id, @driver_id, @vehicle_id, @departure_time, @arrival_time, @departure_city, @arrival_city, @price_per_km, @total_price, @order_status);
            ");
            cmd.Parameters.AddWithValue("@client_id", client_id);
            cmd.Parameters.AddWithValue("@driver_id", driver_id);
            cmd.Parameters.AddWithValue("@vehicle_id", vehicle_license_plate);
            cmd.Parameters.AddWithValue("@departure_time", departure_time);
            cmd.Parameters.AddWithValue("@arrival_time", arrival_time);
            cmd.Parameters.AddWithValue("@departure_city", departure_city);
            cmd.Parameters.AddWithValue("@arrival_city", arrival_city);
            cmd.Parameters.AddWithValue("@price_per_km", price_per_km);
            cmd.Parameters.AddWithValue("@order_status", status.ToString());
            cmd.Parameters.AddWithValue("@total_price", total_price);

            await cfg.execute(cmd);
            order_id = (int)cmd.LastInsertedId;
        }

        /// <summary>
        /// Deletes the order from the database.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
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
        /// Validates everything is ok.
        /// </summary>
        /// <returns></returns>
        public string? validate()
        {
            if (this.driver_id == null)
                return "No driver available for this order";
            if (this.vehicle_license_plate == null)
                return "No vehicle available for this order";
            if (this.route == null)
                return "No route available for this order";

            this.status = OrderStatus.InProgress;
            return null;
        }

        /// <summary>
        /// Finds the most appropriate driver for the order, returns null if no driver is available on this date
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public async Task find_driver(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT user_id, hire_time FROM person
                WHERE user_id not IN (
                    SELECT driver_id 
                    FROM orders
                    WHERE (@departure_time < arrival_time AND @arrival_time > departure_time)
                ) 
                AND user_type = 'EMPLOYEE'
                ORDER BY RAND()
                LIMIT 1;
            ");
            cmd.Parameters.AddWithValue("@departure_time", departure_time);
            cmd.Parameters.AddWithValue("@arrival_time", arrival_time);

            DbDataReader reader = await cfg.query(cmd);
            using (reader)
            {
                if (reader.HasRows)
                {
                    await reader.ReadAsync();
                    this.driver_id = reader.GetInt32("user_id");
                    this.status = OrderStatus.WaitingPayment;

                    long hire_time = reader.GetInt64("hire_time");
                    this.price_per_km = 80 + ((DateTime.Now.Year - DateTimeOffset.FromUnixTimeSeconds(hire_time).DateTime.Year) / 4) * 10; //Price per km increases by 10 every 4 years
                }
                else
                {
                    this.status = OrderStatus.Stuck;
                    this.driver_id = null;
                }
            }
        }

        /// <summary>
        /// Finds the most appropriate vehicle for the order, returns null if no vehicle is available on this date
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="vehicle_type"></param>
        /// <param name="truck_type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task find_vehicle(AppConfig cfg, string vehicle_type, string? truck_type)
        {
            vehicle_type = vehicle_type.ToLower();
            if (truck_type != null) truck_type = truck_type.ToLower();

            if (vehicle_type != "car" && vehicle_type != "van" && vehicle_type != "truck")
                throw new Exception("Invalid vehicle type");
            if (vehicle_type == "truck" && truck_type == null)
                throw new Exception("Truck type must be specified");

            MySqlCommand cmd = new MySqlCommand(@$"
                SELECT license_plate FROM vehicle
                WHERE license_plate not IN (
                    SELECT vehicle_id 
                    FROM orders
                    WHERE (@departure_time < arrival_time AND @arrival_time > departure_time)
                ) 
                AND vehicle_type = @vehicle_type
                {(vehicle_type == "truck" ? "AND (vehicle.truck_type = @truck_type)" : "")}
                ORDER BY RAND()
                LIMIT 1;
            ");
            cmd.Parameters.AddWithValue("@vehicle_type", vehicle_type);
            cmd.Parameters.AddWithValue("@departure_time", departure_time);
            cmd.Parameters.AddWithValue("@arrival_time", arrival_time);
            if (vehicle_type == "truck")
                cmd.Parameters.AddWithValue("@truck_type", truck_type);

            DbDataReader reader = await cfg.query(cmd);
            using (reader)
            {
                if (reader.HasRows)
                {
                    await reader.ReadAsync();
                    this.vehicle_license_plate = reader.GetString("license_plate");
                }
                else
                {
                    this.vehicle_license_plate = null;
                }
            }
        }

        /// <summary>
        /// Builds the itinerary.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// departure_city '{departure_city}' not found
        /// or
        /// arrival_city '{arrival_city}' not found
        /// or
        /// Unknown error ¯\\_(ツ)_/¯
        /// </exception>
        public Itinerary.Itinerary build_itinerary(AppConfig cfg)
        {
            RouteNode[] nodes = cfg.itinerary.GetNodes();

            RouteNode? start = null;
            RouteNode? end = null;

            foreach (RouteNode node in nodes)
            {
                if (start != null && end != null) break;

                if (node.city != null && node.city.name == departure_city) start = node;
                if (node.city != null && node.city.name == arrival_city) end = node;
            }
            if (start != null && end != null)
            {
                return cfg.itinerary.GetRoute(start, end, ItineraryService.EuclideanDistance, ItineraryService.DistanceCost);
            }
            if (start == null) throw new Exception($"departure_city '{departure_city}' not found");
            if (end == null) throw new Exception($"arrival_city '{arrival_city}' not found");
            throw new Exception("Unknown error ¯\\_(ツ)_/¯");

        }

        /// <summary>
        /// Calculates the arrival time.
        /// </summary>
        /// <returns></returns>
        public long calculate_arrival_time()
        {
            if(route == null) throw new Exception("No route set.");
            return departure_time + route.time;
        }

        /// <summary>
        /// Calculates the price of the order.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int calculate_price()
        {
            if (route == null) throw new Exception("No route set.");
            return route.distance * price_per_km + route.cost;
        }

        /// <summary>
        /// Casts an Order from open reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        protected static Order cast_from_open_reader(AppConfig cfg, DbDataReader reader, string prefix = "")
        {
            return new Order(
                cfg,
                reader.GetInt32("order_id"),
                reader.GetInt32("client_id"),
                reader.GetString("vehicle_id"),
                reader.GetInt32("driver_id"),
                reader.GetInt64("departure_time"),
                reader.GetInt64("arrival_time"),
                reader.GetString("departure_city"),
                reader.GetString("arrival_city"),
                reader.GetInt32("price_per_km"),
                reader.GetInt32("total_price"),
                reader.GetString("order_status")
            );
        }

        /// <summary>
        /// Returns an Order from a reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        public async static Task<Order?> from_reader(AppConfig cfg, DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return cast_from_open_reader(cfg, reader, prefix);
            }
        }

        /// <summary>
        /// Returns an Order list from a reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets an order by order_id
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="order_id">The order identifier.</param>
        /// <returns></returns>
        public static async Task<Order?> get_order_by_order_id(AppConfig cfg, int order_id)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT * FROM orders
                WHERE order_id = @order_id;
            ");
            cmd.Parameters.AddWithValue("@order_id", order_id);

            DbDataReader reader = await cfg.query(cmd);
            return await from_reader(cfg, reader);
        }

        /// <summary>
        /// Returns all the orders according to all filters.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="order_field">The order field.</param>
        /// <param name="order_dir">The order dir.</param>
        /// <returns></returns>
        public static async Task<List<Order>> list_orders(AppConfig cfg, string filter = "", int limit = 20, int offset = 0, string order_field = "departure_time", string order_dir = "DESC")
        {
            MySqlCommand cmd = new MySqlCommand(@$"
                SELECT * FROM orders
                WHERE client_id LIKE @filter OR departure_city LIKE @filter OR arrival_city LIKE @filter
                ORDER BY @order_field @order_dir
                LIMIT @limit 
                OFFSET @offset;
            ");
            cmd.Parameters.AddWithValue("@filter", $"%{filter}%");
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@order_field", order_field);
            cmd.Parameters.AddWithValue("@order_dir", order_dir);

            DbDataReader reader = await cfg.query(cmd);
            return await from_reader_multiple(cfg, reader);
        }

        /// <summary>
        /// Returns all the orders of a client.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="client_id">The client identifier.</param>
        /// <returns></returns>
        public static async Task<List<Order>> list_orders_by_client_id(AppConfig cfg, int client_id)
        {
               MySqlCommand cmd = new MySqlCommand(@"
                SELECT * FROM orders
                WHERE client_id = @client_id;
            ");
            cmd.Parameters.AddWithValue("@client_id", client_id);

            DbDataReader reader = await cfg.query(cmd);
            return await from_reader_multiple(cfg,reader);
        }

        /// <summary>
        /// Returns all the active orders of a driver.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="driver_id">The driver identifier.</param>
        public static async Task<List<Order>> list_active_orders_by_driver_id(AppConfig cfg, int driver_id)
        {
            int now = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT * FROM orders
                WHERE driver_id = @driver_id AND arrival_time > @now;
            ");
            cmd.Parameters.AddWithValue("@driver_id", driver_id);

            DbDataReader reader = await cfg.query(cmd);
            return await from_reader_multiple(cfg, reader);
        }

        /// <summary>
        /// Generic function to update a field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cfg">The CFG.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        public async Task update_field<T>(AppConfig cfg, string field, T value)
        {
            MySqlCommand cmd = new MySqlCommand(@$"
                UPDATE orders
                SET {field} = @value
                WHERE order_id = @order_id;
            ");
            cmd.Parameters.AddWithValue("@value", value);
            cmd.Parameters.AddWithValue("@order_id", order_id);
            await cfg.execute(cmd);

        }

        /// <summary>
        /// Sets the departure time and updates the arrival time.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="departure_time">The departure time.</param>
        public async Task set_departure_time(AppConfig cfg, long departure_time)
        {
            await update_field(cfg, "departure_time", departure_time);
            this.departure_time = departure_time;

            this.arrival_time = calculate_arrival_time();
            await update_field(cfg, "arrival_time", arrival_time);
        }

        /// <summary>
        /// Sets the departure city and updates the arrival time.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="departure_city">The departure city.</param>
        public async Task set_departure_city(AppConfig cfg, string departure_city)
        {
            await update_field(cfg, "departure_city", departure_city);
            this.departure_city = departure_city;

            this.arrival_time = calculate_arrival_time();
            await update_field(cfg, "arrival_time", arrival_time);
        }

        /// <summary>
        /// Sets the arrival city and updates the arrival time.
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        /// <param name="arrival_city">The arrival city.</param>
        public async Task set_arrival_city(AppConfig cfg, string arrival_city)
        {
            await update_field(cfg, "arrival_city", arrival_city);
            this.arrival_city = arrival_city;

            this.arrival_time = calculate_arrival_time();
            await update_field(cfg, "arrival_time", arrival_time);
        }
    }
}