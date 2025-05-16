namespace ModTranslator.BO.Objects.Requests
{
    public class TranslationRequest
    {
        public required List<string> Files { get; set; }

        public required string CurrentPath { get; set; }
    }
}
