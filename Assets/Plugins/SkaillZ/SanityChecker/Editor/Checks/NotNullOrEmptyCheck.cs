using System;
using System.Reflection;
using Skaillz.SanityChecker.Attributes;
using UnityEditor;

namespace Skaillz.SanityChecker.Editor.Checks
{
    public class NotNullOrEmptyCheck : ISanityCheck
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            SanityChecker.RegisterCheck<NotNullOrEmptyAttribute>(new NotNullOrEmptyCheck());
        }

        public void Check(object obj, FieldInfo field, Attribute attribute, object context)
        {
            var val = field.GetValue(obj);
            if (val == null)
                throw new InvalidValueException(SanityChecker.CreateExceptionMessage(field, obj,
                    "must not be null. Please assign a value to the string.", context));
            
            if (!(val is string))
                throw new InvalidOperationException(SanityChecker.CreateExceptionMessage(field, obj,
                    "is not a string.", context));

            if (string.IsNullOrEmpty(val.ToString()))
                throw new InvalidValueException(SanityChecker.CreateExceptionMessage(field, obj,
                    "must not be empty. Please assign a value to the string.", context));
        }
    }
}
