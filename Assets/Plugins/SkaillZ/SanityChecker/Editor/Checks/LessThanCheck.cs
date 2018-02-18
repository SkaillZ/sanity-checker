using System;
using System.Reflection;
using Skaillz.SanityChecker.Attributes;
using UnityEditor;

namespace Skaillz.SanityChecker.Editor.Checks
{
    public class LessThanCheck : ISanityCheck
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            SanityChecker.RegisterCheck<LessThanAttribute>(new LessThanCheck());
        }
        
        public void Check(object obj, FieldInfo field, Attribute attribute, object context)
        {
            var val = field.GetValue(obj);
            if (!(val is IConvertible))
                throw new InvalidOperationException(SanityChecker.CreateExceptionMessage(field, obj,
                    "is required to be less than a given value but its type cannot be compared to numbers.", context));

            double comparedValue = ((LessThanAttribute) attribute).Value;
            if (!(Convert.ToDouble(val) < comparedValue))
                throw new InvalidValueException(SanityChecker.CreateExceptionMessage(field, obj,
                    "must be less than " + comparedValue + ".", context));
        }
    }
}