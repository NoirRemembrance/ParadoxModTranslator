using ModTranslator.BO.Constants;
using ModTranslator.BO.Objects.API_DTO;
using ModTranslator.BO.Objects.Requests;
using ModTranslator.BO.Objects.Settings;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ModTranslator.BLL.Services.TranslateFiles
{
    public class TranslateFilesService : ITranslateFilesService
    {
        private readonly SemaphoreSlim _concurrentRequestsLimiter;
        private readonly AppSettings _appSettings;
        private readonly HttpClient _httpClient;
        private string LanguageCode = "";
        private string LanguageName = "";
        private bool HasErrors = false;

        public TranslateFilesService(
            HttpClient httpClient,
            AppSettings appSettings)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
            _concurrentRequestsLimiter = new(appSettings.RequestsSettings.MaxConcurrentRequests);
        }

        /// <summary>
        /// Translates multiple localization files using an external API and writes the translations to output files.
        /// It manages concurrency, avoids re-translating existing lines, and handles errors during translation.
        /// </summary>
        /// <param name="request">The translation request containing file paths and language details.</param>
        /// <returns>A tuple indicating success and a message describing the outcome.</returns>
        public async Task<(bool isSuccess, string message)> TranslateFiles(TranslationRequest request)
        {
            if(_appSettings.APISettings.ApiKey == "Put your API key here and edit Url and Model to match yours.")
            {
                return (false, "Go into the appsettings.json file and input your API settings.");
            }

            foreach (string filePath in request.Files)
            {
                List<string>? fileLines = await GetFileContent(filePath);
                if (fileLines == null)
                {
                    return (false, ConstantStrings.ErrorNullUploadedFile);
                }

                if (!SetLanguageCode(fileLines[0]))
                {
                    return (false, ConstantStrings.InvalidLanguageValueInFirstLine);
                }

                string fileName = Path.GetFileName(filePath);
                string outputFilePath = await CreateTranslatedFileIfDontExist(request.CurrentPath, fileName, fileLines);

                if (File.Exists(outputFilePath))
                {
                    string[] existingLines = await File.ReadAllLinesAsync(outputFilePath);
                    HashSet<string> existingKeys = [];

                    foreach (string? line in existingLines.Skip(1))
                    {
                        string trimmedLine = line.Trim();
                        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                        {
                            continue;
                        }

                        int colonIndex = trimmedLine.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            string key = trimmedLine[..colonIndex].Trim();
                            _ = existingKeys.Add(key);
                        }
                    }

                    fileLines = [.. fileLines
                        .Where(line =>
                        {
                            string trimmedLine = line.Trim();
                            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#')) { return true; } int colonIndex = trimmedLine.IndexOf(':');
                            if (colonIndex <= 0) { return true; } string key = trimmedLine[..colonIndex].Trim();
                            return !existingKeys.Contains(key);
                        })];
                }

                using SemaphoreSlim fileWriteLock = new(1, 1);
                HashSet<int> processedIndices = [];
                object lockObject = new();

                List<Task> translationTasks = [.. fileLines
                    .Select((line, index) => (line, index))
                    .Where(t => !t.line.Trim().StartsWith(LanguageCode))
                    .Select(t => ProcessTranslationGroup(fileLines, t.index, outputFilePath, fileWriteLock, processedIndices, lockObject))];

                try
                {
                    await Task.WhenAll(translationTasks);
                }
                catch (Exception ex)
                {
                    HasErrors = true;
                    await File.AppendAllTextAsync(outputFilePath, $"#Error in translation tasks: {ex.Message}{Environment.NewLine}");
                }

                await File.AppendAllTextAsync(outputFilePath, "#File translation finished" + Environment.NewLine);
            }

            string result = "Translation finished.";
            if (HasErrors)
            {
                result += "\nThere could be errors in the translation, please review the files.";
            }

            return (!HasErrors, result);
        }

        /// <summary>
        /// Processes a group of lines for translation by sending them to the translation API, 
        /// then appends the translated results to the output file in a thread-safe manner.
        /// </summary>
        /// <param name="fileLines">All lines from the original file.</param>
        /// <param name="startIndex">The starting index of lines to translate.</param>
        /// <param name="outputFilePath">The path of the file to append translated lines to.</param>
        /// <param name="fileWriteLock">A semaphore to synchronize file write access.</param>
        /// <param name="processedIndices">A set tracking which line indices have been processed.</param>
        /// <param name="lockObject">An object used for locking when accessing shared collections.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task ProcessTranslationGroup(
            List<string> fileLines,
            int startIndex,
            string outputFilePath,
            SemaphoreSlim fileWriteLock,
            HashSet<int> processedIndices,
            object lockObject)
        {
            await _concurrentRequestsLimiter.WaitAsync();
            try
            {
                List<string> linesBeingTranslated = [];
                List<int> indicesBeingTranslated = [];

                lock (lockObject)
                {
                    for (int i = startIndex; i < fileLines.Count; i++)
                    {
                        if (!processedIndices.Contains(i))
                        {
                            linesBeingTranslated.Add(fileLines[i].Trim());
                            indicesBeingTranslated.Add(i);
                            _ = processedIndices.Add(i);
                        }

                        if (linesBeingTranslated.Count >= _appSettings.RequestsSettings.MaxLengthOfRequests)
                        {
                            break;
                        }
                    }
                }

                if (linesBeingTranslated.Count == 0)
                {
                    return;
                }

                string? translation = await GetTranslationFromAPI(linesBeingTranslated);

                if (translation == null)
                {
                    return;
                }

                List<string> outputContent = ProcessTranslation(linesBeingTranslated, translation);

                await fileWriteLock.WaitAsync();
                try
                {
                    await File.AppendAllTextAsync(outputFilePath, string.Join('\n', outputContent) + Environment.NewLine);
                }
                finally
                {
                    _ = fileWriteLock.Release();
                }
            }
            finally
            {
                _ = _concurrentRequestsLimiter.Release();
            }
        }

        /// <summary>
        /// Sends a batch of lines to the translation API and returns the translated text.
        /// Handles API request setup, serialization, and error handling.
        /// </summary>
        /// <param name="lines">The lines to translate.</param>
        /// <returns>The translated text returned from the API or null if an error occurs.</returns>
        private async Task<string?> GetTranslationFromAPI(List<string> lines)
        {
            APIRequest.Data requestBody = new()
            {
                Model = _appSettings.APISettings.Model,
                Messages =
                [
                    new APIRequest.Message { Role = "system", Content = GenerateSystemPrompt() },
                    new APIRequest.Message { Role = "user", Content = string.Join('\n', lines) }
                ]
            };

            StringContent requestContent = new(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            using HttpRequestMessage apiRequest = new(HttpMethod.Post, _appSettings.APISettings.Url)
            {
                Content = requestContent,
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _appSettings.APISettings.ApiKey) }
            };

            try
            {
                using CancellationTokenSource cts = new(TimeSpan.FromSeconds(_appSettings.RequestsSettings.TimeoutSeconds));
                HttpResponseMessage response = await _httpClient.SendAsync(apiRequest, cts.Token);
                string responseString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<APIResponse>(responseString)?.Choices?.FirstOrDefault()?.Message?.Content;
            }
            catch (Exception ex)
            {
                HasErrors = true;
                Console.WriteLine($"Error in API request: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generates the system prompt message used to instruct the translation API about the task context.
        /// It includes language information and instructions to preserve placeholders.
        /// </summary>
        /// <returns>A string containing the system prompt for the translation API.</returns>
        private string GenerateSystemPrompt()
        {
            return $"""
            You are a helpful assistant translating to {LanguagesManager.GetLanguageKey(LanguageCode)}.
            You provide translations for sentences in Paradox games mods localization files.
            Maintain any placeholder values like "$value$", "£value£", or "[value.function]" without translating them.
            """;
        }

        /// <summary>
        /// Processes the raw translated text returned from the API, pairing it with the original lines,
        /// and formats the output to maintain keys and translated values.
        /// Detects mismatches in line counts and flags errors if necessary.
        /// </summary>
        /// <param name="originalLines">The original lines sent for translation.</param>
        /// <param name="translation">The translated text returned from the API.</param>
        /// <returns>A list of formatted translated lines ready for output.</returns>
        private List<string> ProcessTranslation(List<string> originalLines, string translation)
        {
            List<string> result = [];

            string[] translatedLines = translation.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (originalLines.Count != translatedLines.Length)
            {
                HasErrors = true;
                result = ["#Error: Mismatch in translated lines count. Review the following:",
                    .. translatedLines.Select(line => $"  {line}")];
            }
            else
            {
                result = [.. originalLines.Zip(translatedLines, (original, translated) =>
                    {
                        int colonIndex = original.IndexOf(':');
                        if (colonIndex == -1) { return original; } string key = original[..colonIndex].Trim();
                        string value = translated.Split(':', 2)[1].Trim();
                        return $"  {key}: {value}";
                    })
                ];
            }
            return result;
        }

        /// <summary>
        /// Creates the translated output file if it does not already exist.
        /// Initializes the file with the first line from the original content.
        /// </summary>
        /// <param name="currentPath">The base directory path for output files.</param>
        /// <param name="fileName">The name of the original file.</param>
        /// <param name="fileLines">The original file's lines.</param>
        /// <returns>The full path to the translated output file.</returns>
        private async Task<string> CreateTranslatedFileIfDontExist(string currentPath, string fileName, List<string> fileLines)
        {
            string outputFolder = Path.Combine(currentPath, "TranslatedFiles", "localisation", "replace", LanguageCode);
            _ = Directory.CreateDirectory(outputFolder);
            string outputFilePath = Path.Combine(outputFolder, fileName.Replace("_ToBeTranslated", "_Translated"));

            if (!File.Exists(outputFilePath))
            {
                await File.WriteAllBytesAsync(outputFilePath, [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(fileLines[0] + Environment.NewLine)]);
            }

            return outputFilePath;
        }

        /// <summary>
        /// Sets the language code and language name based on the first line of the localization file.
        /// </summary>
        /// <param name="firstFileLine">The first line from the localization file, expected to contain the language code.</param>
        /// <returns>True if the language code is valid and set successfully; otherwise, false.</returns>
        private bool SetLanguageCode(string firstFileLine)
        {
            LanguageCode = firstFileLine.Replace(":", "");
            LanguageName = LanguagesManager.GetLanguageKey(LanguageCode);
            return !string.IsNullOrEmpty(LanguageName);
        }

        /// <summary>
        /// Reads all non-empty lines from a specified file asynchronously.
        /// Returns null if the file path is invalid or if reading fails.
        /// </summary>
        /// <param name="filePath">The path of the file to read.</param>
        /// <returns>A list of non-empty lines from the file or null on failure.</returns>
        private static async Task<List<string>?> GetFileContent(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            try
            {
                return await File.ReadAllLinesAsync(filePath).ContinueWith(t => t.Result.Where(line => !string.IsNullOrWhiteSpace(line)).ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return null;
            }
        }
    }
}
