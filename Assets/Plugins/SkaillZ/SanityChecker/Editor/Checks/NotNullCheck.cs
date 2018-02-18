using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Skaillz.SanityChecker.Editor.Checks
{
    public class NotNullCheck : ISanityCheck
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            SanityChecker.RegisterCheck<NotNullAttribute>(new NotNullCheck());
        }

        public void Check(object obj, FieldInfo field, Attribute attribute, object context)
        {
            var val = field.GetValue(obj);
            if (val != null && val.GetType().IsSubclassOf(typeof(UnityEngine.Object))) // Handle Unity's pseudo null values
            {
                if ((UnityEngine.Object) val == null)
                    throw new MissingReferenceException(SanityChecker.CreateExceptionMessage(field, obj, "is missing a reference. Please assign it in the editor.", context));
            }

            if (val == null)
                throw new MissingReferenceException(SanityChecker.CreateExceptionMessage(field, obj, "is missing a reference. Please assign it in the editor.", context));
        }
    }
}