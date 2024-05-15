using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Text.Json.Serialization;
using TransLib.Auth;

namespace TransLib.Persons
{
    public abstract class Person
    {
        public int user_id { get; protected set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public long birth_date { get; set; }
        public bool deleted { get; set; }
        

        // ! internal fields, never to be sent over the wire !
        [JsonIgnore]
        protected string? password_hash { get; private set; }

        public Person(int user_id, string first_name, string last_name, string phone, string email, string address, string city, long birth_date, bool deleted, string? password_hash)
        {
            this.user_id = user_id;
            this.first_name = first_name;
            this.last_name = last_name;
            this.phone = phone;
            this.email = email;
            this.address = address;
            this.city = city;
            this.birth_date = birth_date;
            this.deleted = deleted;
            this.password_hash = password_hash;
        }

        public abstract string user_type { get; }
        public abstract Task create(AppConfig cfg);
        public abstract Task delete(AppConfig cfg);

        /// <summary>
        /// Returns an Employee object from a reader. If muliple rows are returned, only the first one is used.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public async static Task<Person?> from_reader(DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                bool more = await reader.ReadAsync();
                if (!more) return null;
                return cast_from_open_reader(reader, prefix);
            }
        }

        /// <summary>
        /// Returns an Employee list from a reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public async static Task<List<Person>> from_reader_multiple(DbDataReader reader, string prefix = "")
        {
            using (reader)
            {
                List<Person> persons = new List<Person>();
                while (await reader.ReadAsync())
                {
                    Person? person = cast_from_open_reader(reader, prefix);
                    if (person != null)
                        persons.Append(person);
                }
                return persons;
            }
        }

        protected static Person cast_from_open_reader(DbDataReader reader, string prefix = "") {
            switch (reader.GetString($"{prefix}user_type"))
            {
                case "EMPLOYEE":
                    return Employee.cast_from_open_reader(reader);
                case "CLIENT":
                    return Client.cast_from_open_reader(reader);
                default:
                    throw new Exception("invalid user_type");
            }
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
            if (this.password_hash == null) return false;
            return PasswordAuthenticator.verify_password(password, this.password_hash);
        }

        public static async Task<Person?> get_person_by_email(AppConfig cfg, string email)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM person WHERE email = @email AND NOT deleted");
            cmd.Parameters.AddWithValue("@email", email);
            DbDataReader reader = await cfg.query(cmd);
            return await from_reader(reader);
        }

        public static async Task<Person?> get_person_by_id(AppConfig cfg, int user_id)
        {
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM person WHERE user_id = @user_id AND NOT deleted");
            cmd.Parameters.AddWithValue("@user_id", user_id);
            DbDataReader reader = await cfg.query(cmd);
            return await from_reader(reader);
        }

        protected async Task update_field<T>(AppConfig cfg, string field, T value)
        {
            if (value == null) return;
            MySqlCommand cmd = new MySqlCommand($"UPDATE person SET {field} = @value WHERE user_id = @user_id");
            cmd.Parameters.AddWithValue("@value", value);
            cmd.Parameters.AddWithValue("@user_id", user_id);
            await cfg.query(cmd);
        }

        public async Task set_first_name(AppConfig cfg, string first_name)
        {
            await update_field(cfg, "first_name", first_name);
            this.first_name = first_name;
        }

        public async Task set_last_name(AppConfig cfg, string last_name)
        {
            await update_field(cfg, "last_name", last_name);
            this.last_name = last_name;
        }

        public async Task set_phone(AppConfig cfg, string phone)
        {
            await update_field(cfg, "phone", phone);
            this.phone = phone;
        }

        public async Task set_email(AppConfig cfg, string email)
        {
            await update_field(cfg, "email", email);
            this.email = email;
        }

        public async Task set_address(AppConfig cfg, string address)
        {
            await update_field(cfg, "address", address);
            this.address = address;
        }

        public async Task set_city(AppConfig cfg, string city)
        {
            await update_field(cfg, "city", city);
            this.city = city;
        }

        public async Task set_birth_date(AppConfig cfg, long birth_date)
        {
            await update_field(cfg, "birth_date", birth_date);
            this.birth_date = birth_date;
        }
    }
}
