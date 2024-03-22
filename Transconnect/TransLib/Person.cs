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
