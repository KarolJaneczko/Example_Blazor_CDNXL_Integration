using cdn_api;
using EBCI_BackEnd.Classes.Enums;

namespace EBCI_BackEnd.Services {
    public class XLService {
        private readonly string _user, _password, _databaseName, _licenseServer;
        private int SessionId;
        public int Version { get; }

        public XLService() {
            Version = 20251;
            SessionId = -1;
        }

        public XLService(string user, string password, string databaseName, string licenseServer) : this() {
            _user = user;
            _password = password;
            _databaseName = databaseName;
            _licenseServer = licenseServer;
        }

        public bool Login(out string message) {
            message = null;
            var result = false;

            var loginInfo = new XLLoginInfo_20251 {
                Wersja = Version,
                ProgramID = "EBCI_BackEnd",
                OpeIdent = _user,
                OpeHaslo = _password,
                Baza = _databaseName,
                SerwerKlucza = _licenseServer,
                UtworzWlasnaSesje = 1,
                Winieta = -1,
                TrybWsadowy = 1
            };
            var sessionId = -1;
            var apiResult = cdn_api.cdn_api.XLLogin(loginInfo, ref sessionId);
            if (apiResult == 0) {
                SessionId = sessionId;
                result = true;
            } else {
                switch (apiResult) {
                    case -9:
                        message = "operator nie jest podpięty do żadnego centrum";
                        break;
                    case -8:
                        message = "nie podano nazwy bazy";
                        break;
                    case -7:
                        message = "baza niezarejestrowana w systemie";
                        break;
                    case -6:
                        message = "nie podano hasła lub brak operatora";
                        break;
                    case -5:
                        message = "nieprawidłowe hasło";
                        break;
                    case -4:
                        message = "konto operatora zablokowane";
                        break;
                    case -3:
                        message = "nie podano nazwy programu (pole ProgramID)";
                        break;
                    case -2:
                        message = "błąd otwarcia pliku tekstowego, do którego mają być zapisywane komunikaty. Nie znaleziono ścieżki lub nazwa pliku jest nieprawidłowa.";
                        break;
                    case -1:
                        message = "podano niepoprawną wersję API";
                        break;
                    case 1:
                        message = "inicjalizacja nie powiodła się";
                        break;
                    case 2:
                        message = "występuje w przypadku, gdy istnieje już jedna instancja programu i nastąpi ponowne logowanie (z tego samego komputera i na tego samego operatora)";
                        break;
                    case 3:
                        message = "występuje w przypadku, gdy istnieje już jedna instancja programu i nastąpi ponowne logowanie z innego komputera i na tego samego operatora, ale operator nie posiada prawa do wielokrotnego logowania";
                        break;
                    case 5:
                        message = "występuje przy pracy terminalowej w przypadku, gdy operator nie ma prawa do wielokrotnego logowania i na pytanie czy usunąć istniejące sesje terminalowe wybrano odpowiedź ‘Nie’.";
                        break;
                    case 61:
                        message = "błąd zakładania nowej sesji";
                        break;
                    default:
                        message = $"Nie zdefiniowano komunikatu, wynik: {apiResult}, metoda: {nameof(Login)}";
                        break;
                }
            }

            return result;
        }

        public bool Logout(out string message) {
            message = null;
            var result = false;

            var apiResult = cdn_api.cdn_api.XLLogout(SessionId);
            if (apiResult == 0) {
                result = true;
            } else {
                switch (apiResult) {
                    case -2:
                        message = "błąd otwarcia pliku tekstowego, do którego mają być zapisywane komunikaty";
                        break;
                    case -1:
                        message = "nie zawołano poprawnie XLLogin";
                        break;
                    case 2:
                        message = "występuje w przypadku, gdy istnieje już jedna instancja programu i nastąpi ponowne logowanie (z tego samego komputera i na tego samego operatora)";
                        break;
                    case 3:
                        message = "występuje w przypadku, gdy istnieje już jedna instancja programu i nastąpi ponowne logowanie z innego komputera i na tego samego operatora, ale operator nie posiada prawa do wielokrotnego logowania";
                        break;
                    case 5:
                        message = "występuje przy pracy terminalowej w przypadku, gdy operator nie ma prawa do wielokrotnego logowania i na pytanie czy usunąć istniejące sesje terminalowe wybrano odpowiedź ‘Nie’.";
                        break;
                    default:
                        message = $"Nie zdefiniowano komunikatu, wynik: {apiResult}, metoda: {nameof(Logout)}";
                        break;
                }
            }

            return result;
        }

        public bool CreateDocument(ref int documentId, XLDokumentNagInfo_20251 xlDokumentNagInfo, out XLDokumentNagInfo_20251 xlDokumentNagInfoResult, out string message) {
            xlDokumentNagInfoResult = null;
            message = null;
            var result = false;
            xlDokumentNagInfo.Wersja = Version;

            var apiResult = cdn_api.cdn_api.XLNowyDokument(SessionId, ref documentId, xlDokumentNagInfo);
            if (apiResult == 0) {
                xlDokumentNagInfoResult = xlDokumentNagInfo;
                result = true;
            } else {
                GetErrorDescription(new XLKomunikatInfo_20251 { Blad = apiResult }, XLMethodType.NowyDokument, out var getErrorMessage);
                message = getErrorMessage;
            }

            return result;
        }

        public bool NewPosition(int documentId, XLDokumentElemInfo_20251 xlDokumentElemInfo, out XLDokumentElemInfo_20251 xlDokumentElemInfoResult, out string message) {
            xlDokumentElemInfoResult = null;
            message = null;
            var result = false;
            xlDokumentElemInfo.Wersja = Version;

            var apiResult = cdn_api.cdn_api.XLDodajPozycje(documentId, xlDokumentElemInfo);
            if (apiResult == 0) {
                xlDokumentElemInfoResult = xlDokumentElemInfo;
                result = true;
            } else {
                GetErrorDescription(new XLKomunikatInfo_20251 { Blad = apiResult }, XLMethodType.DodajPozycje, out var getErrorMessage);
                message = getErrorMessage;

            }

            return result;
        }

        public bool CloseDocument(int documentId, ref XLZamkniecieDokumentuInfo_20251 xlZamkniecieDokumentuInfo, out string message) {
            message = null;
            var result = false;
            xlZamkniecieDokumentuInfo.Wersja = Version;

            var apiResult = cdn_api.cdn_api.XLZamknijDokument(documentId, xlZamkniecieDokumentuInfo);
            if (apiResult == 0) {
                result = true;
            } else {
                GetErrorDescription(new XLKomunikatInfo_20251 { Blad = apiResult }, XLMethodType.ZamknijDokument, out var getErrorMessage);
                message = getErrorMessage;
            }

            return result;
        }

        public bool GetErrorDescription(XLKomunikatInfo_20251 xlKomunikatInfo, XLMethodType xlMethodType, out string message) {
            var result = false;
            xlKomunikatInfo.Wersja = Version;
            xlKomunikatInfo.Funkcja = (int)xlMethodType;

            var apiResult = cdn_api.cdn_api.XLOpisBledu(xlKomunikatInfo);
            if (apiResult == 0) {
                message = xlKomunikatInfo.OpisBledu;
                result = true;
            } else if (xlKomunikatInfo.Tryb == 0 || xlKomunikatInfo.Tryb == 1) {
                message = $"Nie udało się odczytać treści błędu: {xlKomunikatInfo.Blad}, dla metody: {xlMethodType}";
            } else {
                message = $"Nie udało się odczytać treści błędu: {xlKomunikatInfo.Blad} oraz ostrzeżenia: {xlKomunikatInfo.Ostrzezenie}, dla metody: {xlMethodType}";
            }

            return result;
        }

        public bool GetDocumentNumber(XLNumerDokumentuInfo_20251 xlNumerDokumentuInfo, out string message) {
            var result = false;
            message = null;

            xlNumerDokumentuInfo.Wersja = Version;

            var apiResult = cdn_api.cdn_api.XLPobierzNumerDokumentu(xlNumerDokumentuInfo);
            if (apiResult == 0) {
                result = true;
            } else {
                switch (apiResult) {
                    case -1:
                        message = "nieaktywna sesja API lub nieprawidłowy numer wersji";
                        break;
                    case 1:
                        message = "nie znaleziono dokumentu";
                        break;
                    case 2:
                        message = "podano nieobsługiwany GIDTyp";
                        break;
                    default:
                        message = $"Nie zdefiniowano komunikatu, wynik: {apiResult}, metoda: {nameof(GetDocumentNumber)}";
                        break;
                }
            }

            return result;
        }
    }
}