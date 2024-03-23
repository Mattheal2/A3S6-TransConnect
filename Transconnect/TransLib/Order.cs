using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Order
    {
        public enum OrderStatus
        {
            Pending,
            InProgress,
            Stuck,
            WaitingPayment,
            Closed
        }

        protected string order_id;
        protected Client client;
        protected Vehicle vehicle;
        protected Driver driver;
        protected DateTime departure_date;
        protected string departure_city;
        protected string arrival_city;
        protected OrderStatus status;

        //gets the price from delivery price calculation function -> peut etre redondant ?
        public int Price { get => throw new NotImplementedException(); }

        public Order(string order_id, Client client, Vehicle vehicle, DateTime departure_date, string departure_city, string arrival_city)
        {
            this.client = client;
            this.vehicle = vehicle;
            this.departure_date = departure_date;
            this.departure_city = departure_city;
            this.arrival_city = arrival_city;

            (this.driver, this.status) = find_driver(); //+ approprié de trouver un driver automatiquement
        }

        //finds the most appropriate driver for the order, returns null if no driver is available on this date
        private (Driver, OrderStatus) find_driver()
        {
            throw new NotImplementedException();
        }

        public int calculate_distance()
        {
            throw new NotImplementedException();
        }

        public int calculate_duration()
        {
            throw new NotImplementedException();
        }

        public DateTime get_arrival_time()
        {
            throw new NotImplementedException();
        }

        public int calculate_price()
        {
            throw new NotImplementedException();
        }

        public static int estimate_price(string departure_city, string arrival_city)
        {
            throw new NotImplementedException();
        }

        //checks some things and updates the status if necessary
        public void update_status()
        {
            throw new NotImplementedException();
        }
    }
}