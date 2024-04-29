using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using TransLib;
using TransLib.Maps;
using TransLib.Schedule;
using TransLib.Persons;
using System.Runtime.CompilerServices;

namespace TransDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            //test_schedule();
            //test_route().Wait();
            //test_get_db().Wait();
            Console.WriteLine(test_get_clients().Result);
            while (true) ;
        }

        public async static Task<List<Client>?> test_get_clients()
        {
            Company transconnect = new Company("TransConnect", "12 rue de ESLIV");
            return await transconnect.get_clients_list_async();
        }

        public async static Task test_route()
        {
            Route route = new Route(Route.RouteType.Driving, "Chatou", "Paris");
            await route.process_async();
            Console.WriteLine("Itinerary type : " + route.Type);
            Console.WriteLine("Distance : " + route.get_distance());
            Console.WriteLine("Duration : " + route.get_duration());

        }

        public static void test_schedule()
        {
            Schedule schedule = Schedule.from_database($"server=localhost;Port=3306;database=transcodb;uid=root;pwd=;");
            Employee? d1 = schedule.find_driver(new DateTime(2025, 10, 21, 9, 0, 0));
            if(d1 != null) Console.WriteLine(d1.FIRST_NAME + " " + d1.LAST_NAME);
            else Console.WriteLine("No driver available");
            Console.WriteLine(schedule.to_json());
        }

        public async static Task test_get_db()
        {
            Company trans_connexion = new Company("Transgxe", "12 rue des pommes");
            var x = await trans_connexion.get_employees_list_async();
            var y = await trans_connexion.get_vehicles_list_async();

            Console.WriteLine("Liste d'employés :");
            if (x != null) x.ForEach(a => Console.WriteLine(a));
            Console.WriteLine("Liste de véhicules :");
            if (y != null) y.ForEach(a => Console.WriteLine(a));
        }

        public static async void display_async_query(MySqlCommand cmd, string connection_string)
        {

            using (MySqlConnection db_connection = new MySqlConnection(connection_string))
            {

                try
                {   
                    await db_connection.OpenAsync();
                    cmd.Connection = db_connection;

                    Console.WriteLine($"Connection to the database established. SQL version : {db_connection.ServerVersion}");

                    DbDataReader rdr = await cmd.ExecuteReaderAsync();
                    while (await rdr.ReadAsync())
                    {
                        for (int i = 0; i < rdr.FieldCount; i++) Console.Write(rdr[i] + "\t");
                        Console.WriteLine();
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
