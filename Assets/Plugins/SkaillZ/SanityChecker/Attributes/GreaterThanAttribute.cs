namespace Skaillz.SanityChecker.Attributes
{
    public class GreaterThanAttribute : System.Attribute
    {
        public double Value { get; }

        public GreaterThanAttribute(double value)
        {
            Value = value;
        }
    }
}
