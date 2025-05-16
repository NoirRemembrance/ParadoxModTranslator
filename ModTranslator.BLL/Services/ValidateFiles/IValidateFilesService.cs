using ModTranslator.BO.Objects.Requests;

namespace ModTranslator.BLL.Services.ValidateFiles
{
    public interface IValidateFilesService
    {
        /// <summary>
        /// Validates the translation of files between two languages by comparing the contents of each file pair (original and translated).
        /// If differences are found (missing keys, extra keys, or discrepancies), they are logged in a separate validation file.
        /// </summary>
        /// <param name="request">The request object containing the folder path, list of files, and the languages to validate (source and target).</param>
        /// <returns>
        /// A tuple (bool isSuccess, string message):
        /// - <c>isSuccess</c>: True if no differences were found between the original and translated files, false otherwise.
        /// - <c>message</c>: A message indicating the result of the validation (either success or description of issues).
        /// </returns>
        Task<(bool isSuccess, string message)> RunValidation(
            RunValidationRequest request);
    }
}
