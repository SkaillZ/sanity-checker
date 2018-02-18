namespace Skaillz.SanityChecker.Attributes
{
    public class LessThanAttribute : System.Attribute
    {
        public double Value { get; }

        public LessThanAttribute(double value)
        {
            Value = value;
        }
    }
}
