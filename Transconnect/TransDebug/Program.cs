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

            trans_connexion.hire_employee_async(new Driver("John", "Doe", "0651782912", "john.doe@edu.devinci.fr", "98 Avenue de Roissy", "1988-10-01", $"D{trans_connexion.get_available_id("D").Result}", "Driver", "2000", "2021-09-01", "CAR"));

            Console.WriteLine(trans_connexion.get_employee_id_by_name_async("John", "Doe").Result);
            Console.WriteLine(trans_connexion.get_employee_id_by_name_async("Jean", "Pierre").Result);
            Console.WriteLine(trans_connexion.get_employee_id_by_name_async("Karl", "Marx").Result);

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
