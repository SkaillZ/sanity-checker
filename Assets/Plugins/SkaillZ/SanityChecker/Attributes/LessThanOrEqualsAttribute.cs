namespace Skaillz.SanityChecker.Attributes
{
    public class LessThanOrEqualsAttribute : System.Attribute
    {
        public double Value { get; }

        public LessThanOrEqualsAttribute(double value)
        {
            Value = value;
        }
    }
}
