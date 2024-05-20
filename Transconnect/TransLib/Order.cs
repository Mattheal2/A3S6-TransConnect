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
        //public Route route { get; set; }
        public long departure_time { get; set; }
        public long? arrival_time { get; set; }
        public string departure_city { get; set; }
        public string arrival_city { get; set; }
        public OrderStatus status { get; set; }
        public int price_per_km { get; set; }

        /// <summary>
        /// New order constructor
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="departure_time"></param>
        /// <param name="departure_city"></param>
        /// <param name="arrival_city"></param>
        public Order(int client_id, long departure_time, string departure_city, string arrival_city)
        {
            this.client_id = client_id;
            this.departure_time = departure_time;
            this.departure_city = departure_city;
            this.arrival_city = arrival_city;

            //Route.RouteType type = vehicle is Truck ? Route.RouteType.Truck : Route.RouteType.Driving;
            //this.route = new Route(type, departure_city, arrival_city);
            //route.process_async().Wait();

            this.arrival_time = calculate_arrival_time();

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
        public Order(int order_id, int client_id, string vehicle_license_plate, int? driver_id, long departure_time, long? arrival_time, string departure_city, string arrival_city, int price_per_km, string status)
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
        }

        public async Task create(AppConfig cfg)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                INSERT INTO orders (client_id, driver_id, vehicle_id, departure_time, arrival_time, departure_city, arrival_city, price_per_km, order_status)
                VALUES(@client_id, @driver_id, @vehicle_id, @departure_time, @arrival_time, @departure_city, @arrival_city, @price_per_km, @order_status);
            ");
            cmd.Parameters.AddWithValue("@client_id", client_id);
            cmd.Parameters.AddWithValue("@driver_id", driver_id);
            cmd.Parameters.AddWithValue("@vehicle_id", vehicle_license_plate);
            cmd.Parameters.AddWithValue("@departure_time", departure_time);
            cmd.Parameters.AddWithValue("@arrival_time", calculate_arrival_time());
            cmd.Parameters.AddWithValue("@departure_city", departure_city);
            cmd.Parameters.AddWithValue("@arrival_city", arrival_city);
            cmd.Parameters.AddWithValue("@price_per_km", price_per_km);
            cmd.Parameters.AddWithValue("@order_status", status.ToString());

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

        public string? validate()
        {
            if (this.driver_id == null)
                return "No driver available for this order";
            if (this.vehicle_license_plate == null)
                return "No vehicle available for this order";

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
                SELECT user_id, hire_date FROM person
                WHERE user_id not IN (
                    SELECT driver_id 
                    FROM orders
                    WHERE (@departure_time < arrival_time AND @arrival_time > departure_time)
                ) 
                AND NOT deleted AND user_type = 'EMPLOYEE'
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

                    long hire_date = reader.GetInt64("hire_date");
                    this.price_per_km = 80 + ((DateTime.Now.Year - DateTimeOffset.FromUnixTimeSeconds(hire_date).DateTime.Year) / 4) * 10; //Price per km increases by 10 every 4 years
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
                AND NOT deleted AND vehicle_type = @vehicle_type
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

        protected static Order cast_from_open_reader(DbDataReader reader, string prefix = "")
        {
            return new Order(
                reader.GetInt32("order_id"),
                reader.GetInt32("client_id"),
                reader.GetString("vehicle_id"),
                reader.GetInt32("driver_id"),
                reader.GetInt64("departure_time"),
                reader.GetInt64("arrival_time"),
                reader.GetString("departure_city"),
                reader.GetString("arrival_city"),
                reader.GetInt32("price_per_km"),
                reader.GetString("order_status")
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

        public static async Task<Order?> get_order(AppConfig cfg, int order_id)
        {
            MySqlCommand cmd = new MySqlCommand(@"
                SELECT * FROM orders
                WHERE order_id = @order_id;
            ");
            cmd.Parameters.AddWithValue("@order_id", order_id);

            DbDataReader reader = await cfg.query(cmd);
            return await from_reader(reader);
        }

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
            return await from_reader_multiple(reader);
        }
        
        public static async Task<List<Order>> list_orders_by_client_id(AppConfig cfg, int client_id)
        {
               MySqlCommand cmd = new MySqlCommand(@"
                SELECT * FROM orders
                WHERE client_id = @client_id;
            ");
            cmd.Parameters.AddWithValue("@client_id", client_id);

            DbDataReader reader = await cfg.query(cmd);
            return await from_reader_multiple(reader);
        }

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

        public async Task set_departure_time(AppConfig cfg, long departure_time)
        {
            await update_field(cfg, "departure_time", departure_time);
            this.departure_time = departure_time;

            this.arrival_time = calculate_arrival_time();
            await update_field(cfg, "arrival_time", arrival_time);
        }

        public async Task set_departure_city(AppConfig cfg, string departure_city)
        {
            await update_field(cfg, "departure_city", departure_city);
            this.departure_city = departure_city;

            this.arrival_time = calculate_arrival_time();
            await update_field(cfg, "arrival_time", arrival_time);
        }

        public async Task set_arrival_city(AppConfig cfg, string arrival_city)
        {
            await update_field(cfg, "arrival_city", arrival_city);
            this.arrival_city = arrival_city;

            this.arrival_time = calculate_arrival_time();
            await update_field(cfg, "arrival_time", arrival_time);
        }

        //checks some things and updates the status if necessary
        public void update_status()
        {
            throw new NotImplementedException();
        }

    }
}