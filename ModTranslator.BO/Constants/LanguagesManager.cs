namespace ModTranslator.BO.Constants
{
    public class LanguagesManager()
    {
        public static string GetLanguageKey(string code)
        {
            return LanguageRecords.FirstOrDefault(x => x.Value == code).Key ?? "";
        }

        public static readonly Dictionary<string, string> LanguageRecords = new()
        {
            { "Simplified Chinese", "l_simp_chinese" },
            { "English", "l_english" },
            { "Portuguese", "l_braz_por" },
            { "French", "l_french" },
            { "German", "l_german" },
            { "Japanese", "l_japanese" },
            { "Korean", "l_korean" },
            { "Polish", "l_polish" },
            { "Russian", "l_russian" },
            { "Spanish", "l_spanish" },
        };
    }
}
