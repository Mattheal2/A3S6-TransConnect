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


            string query = $"INSERT INTO person(user_id, first_name, last_name, phone, email, address, birth_date) VALUES(\"E{trans_connexion.get_available_id("E").Result}\", \"Jean\", \"Pierre le second\", 0612326754, \"example@mail.com\", \"1 rue de Paris\", \"2002-07-07\");";
            MySqlCommand cmd = new MySqlCommand(query);//$"INSERT INTO person (user_id, first_name, last_name, phone, email, address, birth_date) VALUES(\"E{trans_connexion.get_available_id("E")}\", \"Jean\", \"Pierre le second\", 0612326754, \"example@mail.com\", \"1 rue de Paris\", \"2002-07-07\");");
            display_async_query(cmd, trans_connexion.DB_CONNECTION_STRING);
            Console.WriteLine("-");
            cmd = new MySqlCommand($"SELECT * FROM person;");
            display_async_query(cmd, trans_connexion.DB_CONNECTION_STRING);

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
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            // Afficher la valeur de la colonne à l'index i
                            Console.Write(rdr[i] + "\t");
                        }
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
