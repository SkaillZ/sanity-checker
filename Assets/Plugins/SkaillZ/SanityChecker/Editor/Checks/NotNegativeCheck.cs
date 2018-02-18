using System;
using System.Reflection;
using Skaillz.SanityChecker.Attributes;
using UnityEditor;

namespace Skaillz.SanityChecker.Editor.Checks
{
    public class NotNegativeCheck : ISanityCheck
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            SanityChecker.RegisterCheck<NotNegativeAttribute>(new NotNegativeCheck());
        }

        public void Check(object obj, FieldInfo field, Attribute attribute, object context)
        {
            var val = field.GetValue(obj);
            if (!(val is IConvertible))
                throw new InvalidOperationException(SanityChecker.CreateExceptionMessage(field, obj,
                    "is required to be non-negative but its type cannot be compared to numbers.", context));

            if (Convert.ToDouble(val) < 0.0)
                throw new InvalidValueException(SanityChecker.CreateExceptionMessage(field, obj,
                    "must not be negative.", context));
            
        }
    }
}