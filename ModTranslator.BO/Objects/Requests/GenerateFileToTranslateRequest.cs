namespace ModTranslator.BO.Objects.Requests
{
    public class GenerateFileToTranslateRequest
    {
        public required string FromLanguage { get; set; }

        public required string ToLanguage { get; set; }

        public required List<string> Files { get; set; }

        public required string FolderPath { get; set; }
    }
}
