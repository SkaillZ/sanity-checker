namespace Skaillz.SanityChecker
{
    public class InvalidValueException : System.Exception {
        public InvalidValueException(string message) : base(message) {}
    }
}