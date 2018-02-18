namespace Skaillz.SanityChecker.Attributes
{
    public class GreaterThanOrEqualsAttribute : System.Attribute
    {
        public double Value { get; }

        public GreaterThanOrEqualsAttribute(double value)
        {
            Value = value;
        }
    }
}
