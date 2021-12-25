namespace Lib
{
    public interface IAccessField { }

    public class Index : IAccessField
    {
        public int Value;

        public Index(int index) { Value = index; }
    }

    public class Field : IAccessField
    {
        public string Name;

        public Field(string name) { Name = name; }
    }
}
