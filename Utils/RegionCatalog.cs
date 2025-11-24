// Sitiowebb/Utils/RegionCatalog.cs
using System;
using System.Collections.Generic;

namespace Sitiowebb.Utils
{
    public static class RegionCatalog
    {
        // Grupos: NA = North America, CAM = Central America, SAM = South America,
        // EU = Europe, APAC = Asia-Pacific, OCE = Oceania (Australia/NZ)
        public const string NA   = "NA";
        public const string CAM  = "CAM";
        public const string SAM  = "SAM";
        public const string EU   = "EU";
        public const string APAC = "APAC";
        public const string OCE  = "OCE";

        // Mapa ISO2 -> Grupo
        private static readonly Dictionary<string,string> CountryToGroup = new(StringComparer.OrdinalIgnoreCase)
        {
            // North America
            ["US"]=NA, ["CA"]=NA, ["MX"]=NA,

            // Central America
            ["GT"]=CAM, ["SV"]=CAM, ["HN"]=CAM, ["NI"]=CAM, ["CR"]=CAM, ["PA"]=CAM, ["BZ"]=CAM,

            // South America
            ["AR"]=SAM, ["BO"]=SAM, ["BR"]=SAM, ["CL"]=SAM, ["CO"]=SAM, ["EC"]=SAM, ["GY"]=SAM,
            ["PY"]=SAM, ["PE"]=SAM, ["SR"]=SAM, ["UY"]=SAM, ["VE"]=SAM,

            // Europe (muestra algunos)
            ["ES"]=EU, ["PT"]=EU, ["FR"]=EU, ["DE"]=EU, ["IT"]=EU, ["GB"]=EU, ["IE"]=EU, ["NL"]=EU,

            // APAC (Asia-Pacific)
            ["JP"]=APAC, ["KR"]=APAC, ["CN"]=APAC, ["IN"]=APAC, ["SG"]=APAC, ["HK"]=APAC, ["TW"]=APAC,

            // Oceania (Australia/NZ)
            ["AU"]=OCE, ["NZ"]=OCE,
        };

        public static string? GetGroupForCountry(string? iso2)
        {
            if (string.IsNullOrWhiteSpace(iso2)) return null;
            return CountryToGroup.TryGetValue(iso2.Trim(), out var g) ? g : null;
        }

        public static string DisplayForGroup(string? group) => group?.ToUpperInvariant() switch
        {
            NA   => "North America",
            CAM  => "Central America",
            SAM  => "South America",
            EU   => "Europe",
            APAC => "Asia-Pacific",
            OCE  => "Oceania",
            _    => "Other",
        };

        public static string DisplayForCountry(string? iso2)
        {
            if (string.IsNullOrWhiteSpace(iso2)) return "";
            var up = iso2.Trim().ToUpperInvariant();
            return up switch
            {
                "US" => "USA",
                "GB" => "UK",
                _    => up
            };
        }
    }
}
