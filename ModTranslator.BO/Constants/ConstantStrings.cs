namespace ModTranslator.BO.Constants
{
    public class ConstantStrings
    {
        public const string InvalidLanguageValueInFirstLine = "The language value in the first line of the file is invalid.";

        public const string ErrorTranslationAPI = "Error from translation API";

        public const string ErrorNullResponseFromAPI = "ModTranslator Error: Unexpected response from the AI.";

        public const string ErrorNullUploadedFile = "The file uploaded is null.";

        public const string DescriptorFileTemplate = "version=\"1.0\"\r\ntags={\r\n\t\"Translation\"\r\n}\r\nname=\"Replace this with the name of the mod\"\r\nsupported_version=\"Replace this with current game version\"";
    }
}
