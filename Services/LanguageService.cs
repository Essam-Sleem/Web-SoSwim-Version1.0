namespace Web_SoSwim_Version1._0.Services
{
    public interface ILanguageService
    {
        string CurrentLanguage { get; }
        bool IsArabic { get; }
        bool IsEnglish { get; }
        string Direction { get; }
        string T(string arabicText, string englishText);
    }

    public class LanguageService : ILanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string CurrentLanguage
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return "ar";

                // 1. Query parameter override
                if (context.Request.Query.TryGetValue("lang", out var langQuery) && !string.IsNullOrWhiteSpace(langQuery))
                {
                    var q = langQuery.ToString().ToLowerInvariant();
                    if (q == "en" || q == "ar")
                    {
                        context.Response.Cookies.Append("soswim_lang", q, new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddYears(1),
                            HttpOnly = false,
                            SameSite = SameSiteMode.Lax
                        });
                        return q;
                    }
                }

                // 2. Cookie preference
                if (context.Request.Cookies.TryGetValue("soswim_lang", out var langCookie) && !string.IsNullOrWhiteSpace(langCookie))
                {
                    var c = langCookie.ToLowerInvariant();
                    if (c == "en" || c == "ar") return c;
                }

                // Default is Arabic
                return "ar";
            }
        }

        public bool IsArabic => CurrentLanguage == "ar";
        public bool IsEnglish => CurrentLanguage == "en";
        public string Direction => IsArabic ? "rtl" : "ltr";

        public string T(string arabicText, string englishText)
        {
            return IsEnglish ? englishText : arabicText;
        }
    }
}
