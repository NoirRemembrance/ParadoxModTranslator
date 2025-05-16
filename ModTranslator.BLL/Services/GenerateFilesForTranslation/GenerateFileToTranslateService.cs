using ModTranslator.BO.Objects.Requests;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace ModTranslator.BLL.Services.GenerateFilesForTranslation
{
    public partial class GenerateFileToTranslateService : IGenerateFileToTranslateService
    {
        private readonly ConcurrentDictionary<string, string> OriginalLanguageDictionary = [];
        private readonly ConcurrentDictionary<string, string> NewLanguageDictionary = [];
        private Dictionary<string, string> FinalDictionary = [];
        private string OriginalLanguageCode = "";
        private string NewLanguageCode = "";

        /// <summary>
        /// Generates translation files by comparing original and new language localization files,
        /// extracting missing keys, and creating YAML files for translation.
        /// </summary>
        /// <param name="request">The request containing folder path, files, and language details.</param>
        /// <returns>A string summarizing any warnings and the total amount of lines to translate.</returns>
        public async Task<string> GenerateFile(
            GenerateFileToTranslateRequest request)
        {
            string outputMessage = "";
            int amountOfLinesToTranslate = 0;

            // Get the values of the langueage to translate from and to
            OriginalLanguageCode = request.FromLanguage.ToString();
            NewLanguageCode = request.ToLanguage.ToString();

            string filesFolder = request.FolderPath;
            List<string> yamlFiles = request.Files;
            List<string> filesFromOriginalLanguage = [];
            List<string> filesFromNewLanguage = [];

            foreach (string file in yamlFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                if (fileName.Contains(OriginalLanguageCode))
                {
                    filesFromOriginalLanguage.Add(file);
                }

                if (fileName.Contains(NewLanguageCode))
                {
                    filesFromNewLanguage.Add(file);
                }
            }

            foreach (string file in filesFromOriginalLanguage)
            {
                OriginalLanguageDictionary.Clear();
                NewLanguageDictionary.Clear();
                FinalDictionary.Clear();

                string? matchingFileFromNewLanguage = filesFromNewLanguage
                    .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x)
                    .Replace(NewLanguageCode, "")
                    .Replace("ModTranslator_Translated_", "")
                    .Replace("ModTranslator_", "")
                        == Path.GetFileNameWithoutExtension(file)
                    .Replace(OriginalLanguageCode, "")
                    .Replace("ModTranslator_Translated_", "")
                    .Replace("ModTranslator_", ""));

                // Scan the files and fill the dictionaries with the text lines
                await FillDictionaries(file, true);
                if (matchingFileFromNewLanguage != null)
                {
                    await FillDictionaries(matchingFileFromNewLanguage, false);
                }

                if (NewLanguageDictionary.Count > OriginalLanguageDictionary.Count)
                {
                    outputMessage += $"Warning: file {Path.GetFileName(matchingFileFromNewLanguage)} has more lines than {Path.GetFileName(file)}.\n";
                }

                // Populate the final dictionary with the data
                FinalDictionary = GetMissingKeysOnly();

                // Sort the final dictionary by keys
                FinalDictionary = FinalDictionary.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

                // If there is data to translate, generate the file
                if (FinalDictionary.Any())
                {
                    string newYamlContent = FormatDictionaryToYaml();

                    string fileName = Path.GetFileNameWithoutExtension(file);
                    fileName = fileName.Replace(OriginalLanguageCode, "");

                    string folderPath = Path.Combine(filesFolder, "ToBeTranslated");
                    string outputFilePath = Path.Combine(folderPath, $"{fileName}ModTranslator_ToBeTranslated_{NewLanguageCode}.yml");

                    byte[] utf8WithBom = [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(newYamlContent)];

                    _ = Directory.CreateDirectory(folderPath);
                    await File.WriteAllBytesAsync(outputFilePath, utf8WithBom);

                    amountOfLinesToTranslate += FinalDictionary.Count;
                }
            }

            return $"{outputMessage}Amount of lines to translate: {amountOfLinesToTranslate}";
        }

        /// <summary>
        /// Reads a localization file and fills either the original or new language dictionary with key-value pairs extracted from the file.
        /// Ignores comments, empty lines, and language code headers.
        /// </summary>
        /// <param name="file">The path to the localization file to read.</param>
        /// <param name="isOriginalLanguage">True if filling the original language dictionary; false for the new language dictionary.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task FillDictionaries(string file, bool isOriginalLanguage)
        {
            string[] fileContents = await File.ReadAllLinesAsync(file);

            foreach (string line in fileContents)
            {
                if (line.Trim().StartsWith('#') || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith(OriginalLanguageCode))
                {
                    continue;
                }
                else if (line.StartsWith(NewLanguageCode))
                {
                    continue;
                }

                int separatorIndex = line.IndexOf(':');
                if (separatorIndex != -1)
                {
                    string key = line[..separatorIndex].Trim();
                    string remainder = line[(separatorIndex + 1)..].Trim();

                    string value = remainder;

                    Match match = Regex.Match(remainder, @"^(?:(\d+)\s*)?""?(.*)""?$");
                    if (match.Success)
                    {
                        value = match.Groups[2].Value.Trim(' ', '"');
                    }

                    if (isOriginalLanguage)
                    {
                        OriginalLanguageDictionary[key] = value;
                    }
                    else
                    {
                        NewLanguageDictionary[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Identifies keys that are present in the original language dictionary but missing in the new language dictionary,
        /// returning a dictionary containing only those missing key-value pairs.
        /// </summary>
        /// <returns>A dictionary of missing keys and their corresponding values.</returns>
        private Dictionary<string, string> GetMissingKeysOnly()
        {
            HashSet<string> newKeys = [.. NewLanguageDictionary.Keys];

            Dictionary<string, string> result = OriginalLanguageDictionary
                .Where(kv => !newKeys.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return result;
        }

        /// <summary>
        /// Formats the final dictionary of key-value pairs into a YAML string representation,
        /// starting with the new language code as the root key.
        /// </summary>
        /// <returns>A string containing the formatted YAML content ready for output.</returns>
        private string FormatDictionaryToYaml()
        {
            StringBuilder sb = new();

            _ = sb.AppendLine(NewLanguageCode + ":");

            foreach (KeyValuePair<string, string> kv in FinalDictionary)
            {
                string key = kv.Key;
                string value = kv.Value;

                _ = sb.AppendLine($"  {key}: \"{value}\"");
            }

            return sb.ToString();
        }
    }
}
