using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TransLib.Maps
{
    public static class Map
    {
        public static string CREDENTIALS_PATH = "./data/credentials.json";

        public static string BING_KEY = read_bing_credentials_async();

        private static string read_bing_credentials_async()
        {
            using (StreamReader reader = new System.IO.StreamReader(CREDENTIALS_PATH))
            {
                Dictionary<string, string>? credentials_json = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd());
                if (credentials_json != null) return credentials_json["bing-maps-key"];
                else return "";
            }
        }

    }
}
