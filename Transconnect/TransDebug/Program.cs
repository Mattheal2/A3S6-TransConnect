using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Data;
using System.Data.Common;
using TransLib;

namespace TransDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            Company trans_connexion = new Company("Transgxe", "12 rue des pommes");
            var x = trans_connexion.get_employees_list_async().Result;
            var y = trans_connexion.get_vehicles_list_async().Result;

            Console.WriteLine("Liste d'employés :");
            if(x != null) x.ForEach(a => Console.WriteLine(a));
            Console.WriteLine("Liste de véhicules :");
            if (y != null) y.ForEach(a => Console.WriteLine(a));

            while (true) ;
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
