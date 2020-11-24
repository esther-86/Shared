using System;

namespace Common.Utilities
{
    public static class NumericExtension
    {
        public static decimal TrimToSameDecimalCountAs(this decimal value, decimal valueToMatchDecimalCount)
        {
            // http://stackoverflow.com/questions/13477689/find-number-of-decimal-places-in-decimal-value-regardless-of-culture
            int places = BitConverter.GetBytes(decimal.GetBits((decimal)valueToMatchDecimalCount)[3])[2];
            // http://stackoverflow.com/questions/24158334/c-sharp-math-round-up
            decimal scale = (decimal)Math.Pow(10, places);
            decimal multiplied = value * scale;
            decimal ceiling = Math.Ceiling(multiplied);
            return ceiling / scale;
        }
    }
}
