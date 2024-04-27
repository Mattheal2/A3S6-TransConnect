using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using TransLib.Persons;
using TransLib.Vehicles;
using TransLib.Maps;
using System.Runtime.CompilerServices;

namespace TransLib.Schedule
{
    //This class has 2 main purposes : 
    //Handle the schedule for drivers & vehicles reservations
    //Provides the schedule ready for the UI to display
    public class Schedule
    {
        public struct ScheduleEntry
        {
            public DateTime start;
            public DateTime end;
            public int order_id;
            public int driver_id;
            public string vehicle_id;
        }

        private List<ScheduleEntry> reservations;
        private string connection_string;

        public List<ScheduleEntry> Reservations { get => reservations; }    
        public Schedule(string connection_string)
        {
            this.reservations = new List<ScheduleEntry>();
            this.connection_string = connection_string;
        }

        public static Schedule from_database(string connection_string)
        {
            Schedule schedule = new Schedule(connection_string);

            using (MySqlConnection conn = new MySqlConnection(connection_string))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT departure_date, departure_city, arrival_city, order_id, driver_id, vehicle_id, vehicle_type FROM orders INNER JOIN vehicle ON orders.vehicle_id = vehicle.license_plate;", conn);
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            Route r = new Route(reader.GetString(6) == "TRUCK" ? Route.RouteType.Truck : Route.RouteType.Driving, reader.GetString(1), reader.GetString(2));
                            r.process_async().Wait();
                            DateTime start = reader.GetDateTime(0);
                            DateTime end = start.AddSeconds(r.get_duration());
                            schedule.reservations.Append(new ScheduleEntry
                            {
                                start = start,
                                end = end,
                                order_id = reader.GetInt32(3),
                                driver_id = reader.GetInt32(4),
                                vehicle_id = reader.GetString(5)
                            });
                        }
                        catch (Exception) { throw new Exception("Error while reading database."); }
                    }
                }
            }
            return schedule;

        }

        public string to_json()
        {
            string json = "[";
            //puisque foreach ne marche pas sur les classes mal faites >:(
            for(int i = 0; i<this.reservations.Length;i++)
            {
                //ajouter les reservations au json
                json += "{";
                json += $"\"start\":\"{this.reservations.Get(i).start.ToString("yyyy-MM-dd HH:mm:ss")}\",";
                json += $"\"end\":\"{this.reservations.Get(i).end.ToString("yyyy-MM-dd HH:mm:ss")}\",";
                json += $"\"order_id\":\"{this.reservations.Get(i).order_id}\",";
                json += $"\"driver_id\":\"{this.reservations.Get(i).driver_id}\",";
                json += $"\"vehicle_id\":\"{this.reservations.Get(i).vehicle_id}\"";
                json += "},";
            }
            json += "]";
            return json;
        }

        public Employee? find_driver(DateTime start, DateTime? end = null)
        {
            using(MySqlConnection conn = new MySqlConnection(this.connection_string))
            {
                conn.Open();
                //Finds the newest driver that is not busy during the given period
                MySqlCommand cmd = new MySqlCommand("SELECT user_id, user_type, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date FROM person WHERE user_id NOT IN (SELECT driver_id FROM orders WHERE departure_date BETWEEN @start AND @end OR arrival_date BETWEEN @start AND @end OR DATE(departure_date) = DATE(@start) OR DATE(arrival_date) = DATE(@end)) ORDER BY hire_date DESC;", conn);
                
                cmd.Parameters.AddWithValue("@start", start.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@end", end?.ToString("yyyy-MM-dd HH:mm:ss"));
                using(DbDataReader reader = cmd.ExecuteReader())
                {
                    //La fonction 'from_reader_async' n'est pas prévue pour retourner null si le reader est vide... (to fix)
                    try { return Employee.from_reader_async(reader).Result; }
                    catch (Exception) { return null; }
                }
            }
        }

        public Vehicle? find_vehicle(string vehicle_type, DateTime start, DateTime? end = null)
        {
            using(MySqlConnection conn = new MySqlConnection(this.connection_string))
            {
                conn.Open();
                //Finds a vehicle that is not busy during the given period
                MySqlCommand cmd = new MySqlCommand("SELECT license_plate, brand, model FROM vehicle WHERE vehicle_type = @vehicle_type AND license_plate NOT IN (SELECT vehicle_id FROM orders WHERE departure_date BETWEEN @start AND @end OR arrival_date BETWEEN @start AND @end OR DATE(departure_date) = DATE(@start) OR DATE(arrival_date) = DATE(@end));", conn);
                
                cmd.Parameters.AddWithValue("@vehicle_type", vehicle_type);
                cmd.Parameters.AddWithValue("@start", start.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@end", end?.ToString("yyyy-MM-dd HH:mm:ss"));
                using(DbDataReader reader = cmd.ExecuteReader())
                {
                    //La fonction 'from_reader_async' n'est pas prévue pour retourner null si le reader est vide... (to fix)
                    try { return Vehicle.from_reader_async(reader).Result; }
                    catch (Exception) { return null; }
                }
            }
        }


    }
}
