using Humanizer;
using System;
using System.Globalization;

namespace Invoice_Manager.Models.Common
{
    public static class AmountToWordsHelper
    {
        public static string ConvertToWords(decimal amount)
        {
            if (amount < 0) return "minus " + ConvertToWords(Math.Abs(amount));

            int zlotys = (int)amount;
            int groszy = (int)((amount - zlotys) * 100);


            var culture = new CultureInfo("pl-PL");

            string zlotysWords = zlotys.ToWords(culture);
            
            string currencyName = "z³otych";
            if (zlotys == 1) currencyName = "z³oty";
            else if (zlotys % 10 >= 2 && zlotys % 10 <= 4 && (zlotys % 100 < 10 || zlotys % 100 >= 20)) currencyName = "z³ote";
            
            return $"{zlotysWords} {currencyName} {groszy}/100";
        }
    }
}