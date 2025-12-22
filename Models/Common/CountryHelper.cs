using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Invoice_Manager.Models.Common
{
    public static class CountryHelper
    {
        // S³ownik: KOd kraju NAzwa kraju
        private static readonly Dictionary<string, string> _countries = new Dictionary<string, string>
        {
            { "PL", "Polska" },          // Polska
            { "DE", "Deutschland" },     // Niemcy
            { "AT", "Österreich" },      // Austria
            { "CH", "Schweiz" },         // Szwajcaria
            { "FR", "France" },          // Francja
            { "IT", "Italia" },          // W³ochy
            { "ES", "Espa?a" },          // Hiszpania
            { "PT", "Portugal" },        // Portugalia
            { "NL", "Nederland" },       // Niderlandy (Holandia)
            { "BE", "België" },          // Belgia
            { "LU", "Luxembourg" },      // Luksemburg

            { "CZ", "Èesko" },           // Czechy
            { "SK", "Slovensko" },       // S³owacja
            { "HU", "Magyarország" },    // Wêgry

            { "GB", "United Kingdom" },  // Zjednoczone Królestwo (Wielka Brytania)
            { "IE", "Éire" },            // Irlandia

            { "SE", "Sverige" },         // Szwecja
            { "NO", "Norge" },           // Norwegia
            { "DK", "Danmark" },         // Dania
            { "FI", "Suomi" },           // Finlandia

            { "LT", "Lietuva" },         // Litwa
            { "LV", "Latvija" },         // £otwa
            { "EE", "Eesti" },           // Estonia

            { "RO", "România" },         // Rumunia
            { "BG", "????????" },        // Bu³garia
            { "HR", "Hrvatska" },        // Chorwacja
            { "SI", "Slovenija" }        // S³owenia

        };

        public static List<SelectListItem> GetCountriesList()
        {
            return _countries.Select(c => new SelectListItem
            {
                Value = c.Key, 
                Text = c.Value  
            }).ToList();
        }

        public static string GetCountryName(string code)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;

            return _countries.ContainsKey(code) ? _countries[code] : code;
        }
        
        public static bool IsValidCountryCode(string code)
        {
             return _countries.ContainsKey(code);
        }
    }
}