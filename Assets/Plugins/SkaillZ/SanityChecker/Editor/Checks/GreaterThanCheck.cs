using System;
using System.Reflection;
using Skaillz.SanityChecker.Attributes;
using UnityEditor;

namespace Skaillz.SanityChecker.Editor.Checks
{
    public class GreaterThanCheck : ISanityCheck
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            SanityChecker.RegisterCheck<GreaterThanAttribute>(new GreaterThanCheck());
        }
        
        public void Check(object obj, FieldInfo field, Attribute attribute, object context)
        {
            var val = field.GetValue(obj);
            if (!(val is IConvertible))
                throw new InvalidOperationException(SanityChecker.CreateExceptionMessage(field, obj,
                    "is required to be greater than a given value but its type cannot be compared to numbers.", context));

            double comparedValue = ((GreaterThanAttribute) attribute).Value;
            if (!(Convert.ToDouble(val) > comparedValue))
                throw new InvalidValueException(SanityChecker.CreateExceptionMessage(field, obj,
                    "must be greater than " + comparedValue + ".", context));
        }
    }
}