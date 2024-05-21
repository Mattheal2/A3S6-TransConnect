﻿using MySql.Data.MySqlClient;
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
            ItineraryService service = ItineraryService.Load("./Routing_maps/nodes.json");
            RouteNode[] nodes = service.GetNodes();

            string dep = "Colombes";
            string arr = "Noisy-le-Grand";

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
                Console.WriteLine(route.cost + " euros");
                Console.WriteLine(route.distance + " mètres");
                Console.WriteLine((float)(route.time)/60f + " minutes");
            }
        }
    }
}
