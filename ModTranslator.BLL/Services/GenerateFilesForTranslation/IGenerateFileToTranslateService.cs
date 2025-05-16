using ModTranslator.BO.Objects.Requests;

namespace ModTranslator.BLL.Services.GenerateFilesForTranslation
{
    public interface IGenerateFileToTranslateService
    {
        /// <summary>
        /// Generates translation files by comparing original and new language localization files,
        /// extracting missing keys, and creating YAML files for translation.
        /// </summary>
        /// <param name="request">The request containing folder path, files, and language details.</param>
        /// <returns>A string summarizing any warnings and the total amount of lines to translate.</returns>
        Task<string> GenerateFile(
            GenerateFileToTranslateRequest request);
    }
}
