using System;

namespace CommonAPI
{
    public static class GeneralExtensions
    {
        public static String[] units = {"", "k", "M", "G", "T"};
        
        public static String FormatNumber(this int number)
        {
            if (number == 0) return "0";
            bool sign = false;
            if (number <= 0)
            {
                number = Math.Abs(number);
                sign = true;
            }
            int digitGroups = (int) (Math.Log10(number) / Math.Log10(1000));
            return $"{(sign ? "-" : "")}{number / Math.Pow(1000, digitGroups):0.#}" + units[digitGroups];
        }
    }
}