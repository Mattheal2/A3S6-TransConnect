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

namespace TransDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            string json = File.ReadAllText("./Routing_maps/nodes.json");
            ItineraryService service = ItineraryService.Load("./Routing_maps/nodes.json");
            RouteNode[] nodes = service.GetNodes();

            string dep = "Colombes";
            string arr = "Paris";

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
                Console.WriteLine(route.time + " minutes");
            }
        }
    }
}
