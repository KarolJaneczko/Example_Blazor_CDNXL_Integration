namespace EBCI_BackEnd.Classes.Dictionaries {
    public static class QueryDictionary {
        public const string GetQuerySuppliers = @"SELECT knt.Knt_Akronim
FROM CDN.KntKarty knt
JOIN CDN.KntGrupy kng ON kng.KnG_GIDTyp = knt.Knt_GIDTyp AND kng.KnG_GIDNumer = knt.Knt_GIDNumer
JOIN CDN.KntGrupy kng2 ON kng2.KnG_GIDTyp = kng.KnG_GrOTyp AND kng2.KnG_GIDNumer = kng.KnG_GrONumer
LEFT JOIN CDN.KntGrupy kng3 ON kng3.KnG_GIDTyp = kng2.KnG_GrOTyp AND kng3.KnG_GIDNumer = kng2.KnG_GrONumer
WHERE kng3.KnG_Akronim = 'DOSTAWCY'";

        public const string GetQueryWarehouses = "SELECT MAG_Kod FROM CDN.Magazyny";

        public const string GetQueryProducts = "SELECT Twr_Kod FROM CDN.TwrKarty";
    }
}