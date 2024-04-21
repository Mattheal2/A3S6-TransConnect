using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace TransLib.Maps
{
    public class Route
    {
        public enum RouteType
        {
            Driving,
            Truck
        }

        protected string address1;
        protected string address2;
        protected RouteType type;
        protected string request;
        protected JsonDocument? result;
        
        public string Address1 { get => address1; }
        public string Address2 { get => address2; }
        public RouteType Type { get => type; }
        public string Request { get => request; }
        public JsonDocument? Result { get => result; }

        public Route(RouteType type, string address1, string address2, params string[] options)
        {
            this.address1 = address1;
            this.address2 = address2;

            Array.ForEach(options, x => x = "&" + x);
            this.request = $"http://dev.virtualearth.net/REST/v1/Routes/{type}?o=json&wp.0={address1}&wp.1={address2}&key={Map.BING_KEY}&distanceUnit=km" + string.Join("", options);

            this.result = null;
        }

        public async Task process_async()
        {
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                try
                {
                    this.result = JsonDocument.Parse(await client.GetStringAsync(request));
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public float get_distance()
        {
            if (this.Result == null) throw new InvalidOperationException("Route not processed yet.");
            else
            {
                return JsonSerializer.Deserialize<float>(this.Result.RootElement.GetProperty("resourceSets")[0].GetProperty("resources")[0].GetProperty("travelDistance").GetDecimal());
            }
        }

        //returns the duration of the route in seconds
        public int get_duration()
        {
            if (this.Result == null) throw new InvalidOperationException("Route not processed yet.");
            else
            {
                return JsonSerializer.Deserialize<int>(this.Result.RootElement.GetProperty("resourceSets")[0].GetProperty("resources")[0].GetProperty("travelDuration").GetInt32());
            }
        }


    }
}
