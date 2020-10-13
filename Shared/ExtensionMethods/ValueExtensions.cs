using System;
using MasterCSharp.Shared.RazorCompileService;

namespace MasterCSharp.Shared.ExtensionMethods
{
    public static class ValueExtensions
    {
        public static string AsString(this Enum value)
        {
            string output = null;
            var type = value.GetType();
            var fi = type.GetField(value.ToString());
            var attrs = fi.GetCustomAttributes(typeof(EnumString), false) as EnumString[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }
    }
}
