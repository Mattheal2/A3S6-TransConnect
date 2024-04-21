using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace TransLib
{
    public abstract class Person
    {
        public int USER_ID { get; }
        public string FIRST_NAME { get; }
        public string LAST_NAME { get; }
        public string PHONE { get; }
        public string EMAIL { get; }
        public string ADDRESS { get; }
        public DateTime BIRTH_DATE { get; }

        public Person(int user_id, string first_name, string last_name, string phone, string email, string address, DateTime birth_date)
        {
            this.USER_ID = user_id;
            this.FIRST_NAME = first_name;
            this.LAST_NAME = last_name;
            this.PHONE = phone;
            this.EMAIL = email;
            this.ADDRESS = address;
            this.BIRTH_DATE = birth_date;
        }

        public abstract MySqlCommand save_command();

        /// Returns an Employee object from a reader. If muliple rows are returned, only the first one is used.
        public async static Task<Person> from_reader_async(DbDataReader reader)
        {
            using (reader)
            {
                if (reader == null) throw new Exception("reader is null");

                await reader.ReadAsync();
                return Person.cast_from_open_reader(reader);
            }
        }

        /// Returns an Employee list from a reader.
        public async static Task<List<Person>> from_reader_mulitple_async(DbDataReader reader)
        {
            using (reader)
            {
                if (reader == null) throw new Exception("reader is null");

                List<Person> persons = new List<Person>();
                while (await reader.ReadAsync())
                {
                    persons.Append(Person.cast_from_open_reader(reader));
                }
                return persons;
            }
        }

        protected static Person cast_from_open_reader(DbDataReader reader)
        {
                if (reader == null) throw new Exception("reader is null");

                if (!reader.IsClosed)
                {
                    switch (reader.GetString("user_type"))
                    {
                        case "EMPLOYEE":
                            return new Employee(reader.GetInt32("user_id"), reader.GetString("first_name"), reader.GetString("last_name"), reader.GetString("phone"), reader.GetString("email"), reader.GetString("address"), reader.GetDateTime("birth_date"), reader.GetString("position"), reader.GetFloat("salary"), reader.GetDateTime("hire_date"));
                        case "CLIENT":
                            return new Client(reader.GetInt32("user_id"), reader.GetString("first_name"), reader.GetString("last_name"), reader.GetString("phone"), reader.GetString("email"), reader.GetString("address"), reader.GetDateTime("birth_date"));
                        default:
                            throw new Exception("invalid user_type");
                    }
                }

                else
                {
                    throw new Exception("unable to read closed reader");
                }
        }

        public override string ToString()
        {
            return $"{FIRST_NAME} {LAST_NAME}";
        }
    }
}
