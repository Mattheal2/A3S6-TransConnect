namespace TransLib
{
    public abstract class Person
    {
        protected string first_name;
        protected string last_name;
        protected string phone;
        protected string email;
        protected string address;
        protected string birth_date;

        public string First_name { get => first_name;}
        public string Last_name { get => last_name;}
        public string Phone { get => phone; set => this.phone = value; }
        public string Email { get => email; set => this.email = value; }
        public string Address { get => address; set => this.address = value; }
        public string Birth_date { get => birth_date; set => this.birth_date = value; }


        public Person(string first_name, string last_name, string phone, string email, string address, string birth_date)
        {
            this.first_name = first_name;
            this.last_name = last_name;
            this.phone = phone;
            this.email = email;
            this.address = address;
            this.birth_date = birth_date;
        }
    }
}
