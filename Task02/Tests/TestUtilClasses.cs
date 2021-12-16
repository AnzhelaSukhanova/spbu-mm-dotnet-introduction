namespace Tests
{
    public class Car
    {
        public Person Owner;

        public Car(Person owner)
        {
            Owner = owner;
        }
    }

    public class Person
    {
        public Identifier Id;

        public Person(Identifier id)
        {
            Id = id;
        }
    }

    public class Identifier
    {
        public string FirstName;
        private string LastName;

        public Identifier(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}