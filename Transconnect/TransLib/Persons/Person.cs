using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Text.Json.Serialization;
using TransLib.Auth;

namespace TransLib.Persons
{
    public abstract class Person
    {
        public int user_id { get; }
        public string first_name { get; }
        public string last_name { get; }
        public string phone { get; }
        public string email { get; }
        public string address { get; }
        public DateTime birth_date { get; }
        

        // ! internal fields, never to be sent over the wire !
        [JsonIgnore]
        protected string password_hash { get; private set; }

        public Person(int user_id, string first_name, string last_name, string phone, string email, string address, DateTime birth_date, string password_hash)
        {
            this.user_id = user_id;
            this.first_name = first_name;
            this.last_name = last_name;
            this.phone = phone;
            this.email = email;
            this.address = address;
            this.birth_date = birth_date;
            this.password_hash = password_hash;
        }

        public abstract MySqlCommand save_command();
        public abstract string user_type { get; }

        /// Returns an Employee object from a reader. If muliple rows are returned, only the first one is used.
        public async static Task<Person?> from_reader_async(DbDataReader? reader)
        {
            if (reader == null) throw new Exception("reader is null");
            using (reader)
            {
                await reader.ReadAsync();
                return cast_from_open_reader(reader);
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
                    Person? person = cast_from_open_reader(reader);
                    if (person != null)
                        persons.Append(person);
                }
                return persons;
            }
        }

        protected static Person? cast_from_open_reader(DbDataReader? reader)
        {
            if (reader == null) throw new Exception("reader is null");

            if (!reader.IsClosed)
            {
                switch (reader.GetString("user_type"))
                {
                    case "EMPLOYEE":
                        return Employee.cast_from_open_reader(reader);
                    case "CLIENT":
                        return Client.cast_from_open_reader(reader);
                    default:
                        throw new Exception("invalid user_type");
                }
            }

            return null;
        }

        public override string ToString()
        {
            return $"{first_name} {last_name}";
        }

        public async Task update_password(AppConfig cfg, string password)
        {
            this.password_hash = PasswordAuthenticator.hash_password(password);
            MySqlCommand cmd = new MySqlCommand("UPDATE person SET password_hash = @password_hash WHERE user_id = @user_id");
            cmd.Parameters.AddWithValue("@password_hash", this.password_hash);
            cmd.Parameters.AddWithValue("@user_id", this.user_id);
            await cfg.query(cmd);
        }

        public bool check_password(string password)
        {
            return PasswordAuthenticator.verify_password(password, this.password_hash);
        }

        public static async Task<Person?> get_person_by_email(AppConfig cfg, string email)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM person WHERE email = @email");
            cmd.Parameters.AddWithValue("@email", email);
            DbDataReader? reader = await cfg.query(cmd);
            if (reader == null) throw new Exception("reader is null");
            return await from_reader_async(reader);
        }

        public static async Task<Person?> get_person_by_id(AppConfig cfg, int user_id)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM person WHERE user_id = @user_id");
            cmd.Parameters.AddWithValue("@user_id", user_id);
            DbDataReader? reader = await cfg.query(cmd);
            if (reader == null) throw new Exception("reader is null");
            return await from_reader_async(reader);
        }
    }
}
