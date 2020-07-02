using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMS
{
    public class Utility
    {
        public static bool isIntLength4(string text)
        // checks to see if the text is an integer
        {
            if (text.Length != 4)
                return false;

            string numbers = "0123456789";

            foreach (char c in text)
            {
                if (!numbers.Contains(c))
                    return false;
            }

            return true;
        }
    }
}
