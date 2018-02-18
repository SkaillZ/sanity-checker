using System;
using System.Reflection;

namespace Skaillz.SanityChecker
{
    public interface ISanityCheck
    {
        void Check(object obj, FieldInfo field, Attribute attribute, object context);
    }
}