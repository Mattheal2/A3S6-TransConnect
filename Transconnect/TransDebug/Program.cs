using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using TransLib;
using TransLib.Itinerary;
using TransLib.Persons;
using System.Runtime.CompilerServices;
using System.IO;
using TransLib.Auth;
namespace TransDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            string json = File.ReadAllText("../Routing_maps/nodes.json");
            ItineraryService service = ItineraryService.Load("../Routing_maps/nodes.json");
            RouteNode[] nodes = service.GetNodes();

            string dep = "Nanterre";
            string arr = "Pau";

            RouteNode? start = null;
            RouteNode? end = null;

            foreach(RouteNode node in nodes)
            {
                if (start != null && end != null) break;

                if (node.city != null && node.city.name == dep) start = node;
                if(node.city != null && node.city.name == arr) end = node;
            }
            if (start != null && end != null)
            {
                Itinerary route = service.GetRoute(start, end, ItineraryService.EuclideanDistance, ItineraryService.DistanceCost);
                Console.WriteLine("Route from " + start.city.name + " to " + end.city.name + ":");
                Console.WriteLine((float)route.cost/100f + " euros");
                Console.WriteLine(route.distance + " mètres");
                Console.WriteLine((float)(route.time)/60f + " minutes");
            }

            Console.WriteLine(TransLib.Auth.PasswordAuthenticator.hash_password("password"));
        }
    }
}
