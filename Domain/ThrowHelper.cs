using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain
{
    public static class ThrowHelper
    {
        public static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        public static Exception ArgumentError(string message)
        {
            return new ArgumentException(message);
        }

        public static Exception InvalidOperation(string message)
        {
            return new InvalidOperationException(message);
        }

        public static Exception FormatError(string message)
        {
            return new FormatException(message);
        }
    }
}
