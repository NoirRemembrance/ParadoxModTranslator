using ModTranslator.BO.Objects.Requests;
using System.Text;
using System.Text.RegularExpressions;

namespace ModTranslator.BLL.Services.ValidateFiles
{
    public partial class ValidateFilesService : IValidateFilesService
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
        public async Task<(bool isSuccess, string message)> RunValidation(RunValidationRequest request)
        {
            string originalLanguageCode = request.FromLanguage.ToString();
            string newLanguageCode = request.ToLanguage.ToString();
            string filesFolder = Path.Combine(request.FolderPath, "Validations");

            List<string> originalFiles = [.. request.Files
                .Where(f => Path.GetFileNameWithoutExtension(f).Contains(originalLanguageCode))];

            List<string> newFiles = [.. request.Files
                .Where(f => Path.GetFileNameWithoutExtension(f).Contains(newLanguageCode))];

            List<string> outputFiles = [];

            foreach (string? originalFile in originalFiles)
            {
                string? matchingFile = FindMatchingFile(originalFile, newFiles, originalLanguageCode, newLanguageCode);
                if (matchingFile == null)
                {
                    continue;
                }

                Dictionary<string, string> originalDict = await LoadFileDictionary(originalFile, originalLanguageCode, newLanguageCode);
                Dictionary<string, string> newDict = await LoadFileDictionary(matchingFile, originalLanguageCode, newLanguageCode);

                StringBuilder differences = CompareDictionaries(originalDict, newDict, newLanguageCode);

                if (differences.Length > 0)
                {
                    string outputFile = await WriteDifferencesToFile(
                        differences,
                        filesFolder,
                        originalFile,
                        originalLanguageCode
                    );
                    outputFiles.Add(outputFile);
                }
            }

            return outputFiles.Count > 0
                ? ((bool isSuccess, string message))(false, "There could be some issues in the files, details were logged in a file inside the Validations folder.")
                : ((bool isSuccess, string message))(true, "No issues found.");
        }


        /// <summary>
        /// Compares two dictionaries (original and translated) and returns a formatted string of detected differences,
        /// including missing keys, extra keys, mismatched functions, icons, and unexpected Chinese characters.
        /// </summary>
        /// <param name="originalDict">The dictionary of original language key-value pairs.</param>
        /// <param name="newDict">The dictionary of translated language key-value pairs.</param>
        /// <param name="newLanguageCode">The language code of the translated language (e.g., "l_english").</param>
        /// <returns>A StringBuilder object containing the formatted differences between the dictionaries.</returns>
        private static StringBuilder CompareDictionaries(
            Dictionary<string, string> originalDict,
            Dictionary<string, string> newDict,
            string newLanguageCode)
        {
            StringBuilder result = new();

            foreach (KeyValuePair<string, string> originalItem in originalDict)
            {
                string originalKey = originalItem.Key;
                string originalValue = originalItem.Value;
                string newValue = newDict.TryGetValue(originalKey, out string? val) ? val : string.Empty;

                HashSet<string> originalKeys = ExtractKeys(originalValue);
                HashSet<string> newKeys = ExtractKeys(newValue);
                HashSet<string> originalFunctions = ExtractFunctions(originalValue);
                HashSet<string> newFunctions = ExtractFunctions(newValue);
                HashSet<string> originalIcons = ExtractIcons(originalValue);
                HashSet<string> newIcons = ExtractIcons(newValue);

                List<string> missingKeys = [.. originalKeys.Except(newKeys)];
                List<string> extraKeys = [.. newKeys.Except(originalKeys)];
                List<string> missingFunctions = [.. originalFunctions.Except(newFunctions)];
                List<string> extraFunctions = [.. newFunctions.Except(originalFunctions)];
                List<string> missingIcons = [.. originalIcons.Except(newIcons)];
                List<string> extraIcons = [.. newIcons.Except(originalIcons)];

                bool chineseCharactersFound = newValue.Any(IsChinese) && newLanguageCode != "l_simp_chinese";

                if (missingKeys.Count != 0 || extraKeys.Count != 0 ||
                    missingFunctions.Count != 0 || extraFunctions.Count != 0 ||
                    missingIcons.Count != 0 || extraIcons.Count != 0 || chineseCharactersFound)
                {
                    _ = result.AppendLine($"Difference found in key: {originalKey}");

                    if (missingKeys.Count != 0)
                    {
                        _ = result.AppendLine($" # Missing keys in new: {string.Join(", ", missingKeys)}");
                    }

                    if (extraKeys.Count != 0)
                    {
                        _ = result.AppendLine($" # Extra keys in new: {string.Join(", ", extraKeys)}");
                    }

                    if (missingFunctions.Count != 0)
                    {
                        _ = result.AppendLine($" # Missing functions in new: {string.Join(", ", missingFunctions)}");
                    }

                    if (extraFunctions.Count != 0)
                    {
                        _ = result.AppendLine($" # Extra functions in new: {string.Join(", ", extraFunctions)}");
                    }

                    if (missingIcons.Count != 0)
                    {
                        _ = result.AppendLine($" # Missing icons in new: {string.Join(", ", missingIcons)}");
                    }

                    if (extraIcons.Count != 0)
                    {
                        _ = result.AppendLine($" # Extra icons in new: {string.Join(", ", extraIcons)}");
                    }

                    if (chineseCharactersFound)
                    {
                        _ = result.AppendLine($" # Chinese characters found in the translation");
                    }

                    _ = result.AppendLine($" # Original value: {originalValue}");
                    _ = result.AppendLine($" # New value: {newValue}");
                    _ = result.AppendLine();
                }
            }

            return result;
        }


        /// <summary>
        /// Finds the matching translated file for an original file by comparing file names without language codes or prefixes.
        /// </summary>
        /// <param name="originalFile">The path of the original language file.</param>
        /// <param name="newFiles">The list of translated files.</param>
        /// <param name="originalCode">The language code of the original language (e.g., "l_english").</param>
        /// <param name="newCode">The language code of the translated language (e.g., "l_spanish").</param>
        /// <returns>The matching translated file path, or null if no match is found.</returns>
        private static string? FindMatchingFile(
            string originalFile,
            List<string> newFiles,
            string originalCode,
            string newCode)
        {
            string originalName = Path.GetFileNameWithoutExtension(originalFile)
                .Replace(originalCode, "")
                .Replace("ModTranslator_Translated_", "")
                .Replace("ModTranslator_", "");

            return newFiles.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f)
                    .Replace(newCode, "")
                    .Replace("ModTranslator_Translated_", "")
                    .Replace("ModTranslator_", "") == originalName
            );
        }


        /// <summary>
        /// Loads the contents of a localization file into a dictionary,
        /// extracting key-value pairs while ignoring comments, empty lines, and language code headers.
        /// </summary>
        /// <param name="file">The path of the file to load.</param>
        /// <param name="originalCode">The language code of the original language (e.g., "l_english").</param>
        /// <param name="newCode">The language code of the translated language (e.g., "l_spanish").</param>
        /// <returns>A dictionary containing the key-value pairs from the file.</returns>
        private static async Task<Dictionary<string, string>> LoadFileDictionary(
            string file,
            string originalCode,
            string newCode)
        {
            Dictionary<string, string> dictionary = [];
            string[] lines = await File.ReadAllLinesAsync(file);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith(originalCode) || line.StartsWith(newCode))
                {
                    continue;
                }

                int separatorIndex = line.IndexOf(':');
                if (separatorIndex == -1)
                {
                    continue;
                }

                string key = line[..separatorIndex].Trim();
                string value = line[(separatorIndex + 1)..].Trim();

                Match match = ValueWithOptionalNumberRegex().Match(value);
                if (match.Success)
                {
                    value = match.Groups[2].Value.Trim(' ', '"');
                }

                dictionary[key] = value;
            }

            return dictionary;
        }


        /// <summary>
        /// Extracts placeholder keys (e.g., $key$) from a string using regular expressions.
        /// </summary>
        /// <param name="value">The string value to search for keys.</param>
        /// <returns>A HashSet of keys found in the string.</returns>
        private static HashSet<string> ExtractKeys(string value)
        {
            return [.. KeysRegex().Matches(value).Select(m => m.Value.Trim())];
        }


        /// <summary>
        /// Extracts function placeholders (e.g., [function]) from a string using regular expressions.
        /// </summary>
        /// <param name="value">The string value to search for functions.</param>
        /// <returns>A HashSet of functions found in the string.</returns>
        private static HashSet<string> ExtractFunctions(string value)
        {
            return [.. FunctionsRegex().Matches(value).Select(m => m.Value.Trim())];
        }


        /// <summary>
        /// Extracts icon placeholders (e.g., £icon£) from a string using regular expressions.
        /// </summary>
        /// <param name="value">The string value to search for icons.</param>
        /// <returns>A HashSet of icons found in the string.</returns>
        private static HashSet<string> ExtractIcons(string value)
        {
            return [.. IconsRegex().Matches(value).Select(m => m.Value.Trim())];
        }

        /// <summary>
        /// Checks if a given character is a Chinese character using a regular expression.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is a Chinese character, false otherwise.</returns>
        private static bool IsChinese(char c)
        {
            return ChineseCharactersRegex().IsMatch(c.ToString());
        }


        /// <summary>
        /// Writes the detected differences between original and translated files to a validation output file,
        /// creating the "Validations" folder if it does not exist.
        /// </summary>
        /// <param name="differences">A StringBuilder containing the formatted differences.</param>
        /// <param name="outputFolder">The folder path where the validation file should be created.</param>
        /// <param name="originalFile">The path of the original file being validated.</param>
        /// <param name="originalCode">The language code of the original language (e.g., "l_english").</param>
        /// <returns>A string representing the path of the generated validation file.</returns>
        private static async Task<string> WriteDifferencesToFile(
            StringBuilder differences,
            string outputFolder,
            string originalFile,
            string originalCode)
        {
            _ = Directory.CreateDirectory(outputFolder);
            string fileName = Path.GetFileNameWithoutExtension(originalFile)
                .Replace(originalCode, "");
            string outputFile = Path.Combine(outputFolder, $"{fileName}_ModTranslator_Validations.yml");

            await File.AppendAllTextAsync(outputFile, differences.ToString());
            return outputFile;
        }


        [GeneratedRegex(@"(?<!\$)\$(\w+)\$(?!\$)")]
        private static partial Regex KeysRegex();

        [GeneratedRegex(@"\[(\w+)\]")]
        private static partial Regex FunctionsRegex();

        [GeneratedRegex(@"£(\w+)£")]
        private static partial Regex IconsRegex();

        [GeneratedRegex(@"\p{IsCJKUnifiedIdeographs}")]
        private static partial Regex ChineseCharactersRegex();

        [GeneratedRegex(@"^(?:(\d+)\s*)?""?(.*)""?$")]
        private static partial Regex ValueWithOptionalNumberRegex();
    }
}
