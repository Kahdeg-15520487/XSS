using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simple_interpreter
{
    static class ExtensionMethod
    {
        public static bool IsIdent(this char c)
        {
            // Return true if the character is between 0 or 9 inclusive or is an uppercase or
            // lowercase letter or underscore

            return ((c >= '0' && c <= '9') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z') ||
                     c == '_' ||
                     c == '$' ||
                     c == ':');
        }

        public static bool IsNumeric(this char c)
        {
            return (c >= '0' && c <= '9');
        }

        public static bool IsHexNumeric(this char c)
        {
            return IsNumeric(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        }
    }
}
