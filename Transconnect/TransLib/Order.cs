using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Order
    {
        protected Client client;
        protected Vehicle vehicle;
        protected Driver driver;
        protected DateTime arrrival_date;
        protected int price;
        protected string departure_city;
        protected string arrival_city;

        public Order(Client client, Vehicle vehicle, Driver driver, int price, string departure_city, string arrival_city, DateTime arrrival_date)
        {
            this.client = client;
            this.vehicle = vehicle;
            this.driver = driver;
            this.price = price;
            this.departure_city = departure_city;
            this.arrival_city = arrival_city;
            this.arrrival_date = arrrival_date;
        }
    }
}
