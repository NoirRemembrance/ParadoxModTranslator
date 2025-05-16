using ModTranslator.BO.Objects.Requests;

namespace ModTranslator.BLL.Services.TranslateFiles
{
    public interface ITranslateFilesService
    {
        /// <summary>
        /// Translates multiple localization files using an external API and writes the translations to output files.
        /// It manages concurrency, avoids re-translating existing lines, and handles errors during translation.
        /// </summary>
        /// <param name="request">The translation request containing file paths and language details.</param>
        /// <returns>A tuple indicating success and a message describing the outcome.</returns>
        Task<(bool isSuccess, string message)> TranslateFiles(
            TranslationRequest request);
    }
}
