using ModTranslator.BLL.Services.GenerateFilesForTranslation;
using ModTranslator.BLL.Services.TranslateFiles;
using ModTranslator.BLL.Services.ValidateFiles;
using ModTranslator.BO.Constants;
using ModTranslator.BO.Objects.Requests;

namespace ModTranslator
{
    public partial class Form1 : Form
    {
        private readonly IGenerateFileToTranslateService _generateFileToTranslateService;
        private readonly ITranslateFilesService _translateFileService;
        private readonly IValidateFilesService _validateFilesService;
        private readonly List<string> selectedFilesToGenerate = [];
        private readonly List<string> selectedFilesToTranslate = [];


        public Form1(
            IGenerateFileToTranslateService generateFileToTranslateService,
            ITranslateFilesService translateFileService,
            IValidateFilesService validateFilesService
            )
        {
            _generateFileToTranslateService = generateFileToTranslateService;
            _translateFileService = translateFileService;
            _validateFilesService = validateFilesService;
            InitializeComponent();
            LoadLanguages();
        }

        #region Main Functions

        private async void GenerateButton_Click(object sender, EventArgs e)
        {
            try
            {
                DisableUI();

                string? translateFrom = fromLanguageSelect.SelectedValue?.ToString();
                string? translateTo = toLanguageSelect.SelectedValue?.ToString();
                string currentPath = Path.Combine(Directory.GetCurrentDirectory(), "Files");

                GenerateFileToTranslateRequest request = new()
                {
                    FromLanguage = translateFrom ?? "",
                    ToLanguage = translateTo ?? "",
                    Files = selectedFilesToGenerate,
                    FolderPath = currentPath
                };

                string result = await _generateFileToTranslateService.GenerateFile(request);
                _ = MessageBox.Show(result, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EnableUI();
            }
        }

        private async void ValidateButton_Click(object sender, EventArgs e)
        {
            string? translateFrom = fromLanguageSelect.SelectedValue?.ToString();
            string? translateTo = toLanguageSelect.SelectedValue?.ToString();
            string currentPath = Path.Combine(Directory.GetCurrentDirectory(), "Files");

            RunValidationRequest request = new()
            {
                FromLanguage = translateFrom ?? "",
                ToLanguage = translateTo ?? "",
                Files = selectedFilesToGenerate,
                FolderPath = currentPath
            };

            (bool isSuccess, string message) = await _validateFilesService.RunValidation(request);
            _ = MessageBox.Show(message, isSuccess ? "Success" : "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void TranslateButton_Click(object sender, EventArgs e)
        {
            try
            {
                DisableUI();

                string currentPath = Path.Combine(Directory.GetCurrentDirectory(), "Files");

                TranslationRequest request = new()
                {
                    CurrentPath = currentPath,
                    Files = selectedFilesToTranslate
                };

                (bool isSuccess, string message) = await _translateFileService.TranslateFiles(request);
                _ = MessageBox.Show(message, isSuccess ? "Success" : "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EnableUI();
            }

        }

        #endregion


        #region Manage Files

        private static void SelectFiles(List<string> targetList, Panel panel, EventHandler removeHandler)
        {
            using OpenFileDialog ofd = new()
            {
                Multiselect = true,
                Title = "Select YAML files",
                Filter = "YAML files (*.yml)|*.yml"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            foreach (string file in ofd.FileNames)
            {
                if (targetList.Contains(file))
                {
                    continue;
                }

                targetList.Add(file);
                AddFilePanel(file, panel, removeHandler);
            }
        }

        private void AddFilesToGenerateButton_Click(object sender, EventArgs e)
        {
            SelectFiles(selectedFilesToGenerate, selectedFilesToGeneratePanel, BtnRemoveGenerate_Click);
        }

        private void AddFilesToTranslateButton_Click(object sender, EventArgs e)
        {
            SelectFiles(selectedFilesToTranslate, selectedFilesToTranslatePanel, BtnRemoveTranslate_Click);
        }

        private static void AddFilePanel(string filePath, Panel panel, EventHandler removeHandler)
        {
            Panel filePanel = new()
            {
                Height = 30,
                Width = panel.Width - 25,
                Margin = new Padding(5)
            };

            Label lblFileName = new()
            {
                Text = Path.GetFileName(filePath),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Width = filePanel.Width - 50,
                Height = 30,
                Left = 0
            };

            Button btnRemove = new()
            {
                Text = "X",
                Width = 30,
                Height = 30,
                Left = lblFileName.Width + 5,
                Tag = filePath
            };
            btnRemove.Click += removeHandler;

            filePanel.Controls.Add(lblFileName);
            filePanel.Controls.Add(btnRemove);

            panel.Controls.Add(filePanel);
        }

        private void BtnRemoveGenerate_Click(object sender, EventArgs e)
        {
            Button? btn = sender as Button;
            string? filePath = btn?.Tag?.ToString();

            // Remove from list
            if (filePath != null)
            {
                _ = selectedFilesToGenerate.Remove(filePath);
            }

            // Remove panel
            Control? panel = btn?.Parent;
            selectedFilesToGeneratePanel.Controls.Remove(panel);
            panel?.Dispose();
        }

        private void BtnRemoveTranslate_Click(object sender, EventArgs e)
        {
            Button? btn = sender as Button;
            string? filePath = btn?.Tag?.ToString();

            // Remove from list
            if (filePath != null)
            {
                _ = selectedFilesToTranslate.Remove(filePath);
            }

            // Remove panel
            Control? panel = btn?.Parent;
            selectedFilesToTranslatePanel.Controls.Remove(panel);
            panel?.Dispose();
        }

        private void ClearGenerateButton_Click(object sender, EventArgs e)
        {
            selectedFilesToGenerate.Clear();
            selectedFilesToGeneratePanel.Controls.Clear();
        }

        private void ClearTranslateButton_Click(object sender, EventArgs e)
        {
            selectedFilesToTranslate.Clear();
            selectedFilesToTranslatePanel.Controls.Clear();
        }

        #endregion


        #region Manage Languages

        private void LoadLanguages()
        {
            BindLanguageComboBox(fromLanguageSelect);
            BindLanguageComboBox(toLanguageSelect);
        }

        private static void BindLanguageComboBox(ComboBox comboBox)
        {
            comboBox.DataSource = LanguagesManager.LanguageRecords.ToList();
            comboBox.DisplayMember = "Key";
            comboBox.ValueMember = "Value";
        }

        #endregion


        private void DisableUI()
        {
            addFilesToGenerateButton.Enabled = false;
            addFilesToTranslateButton.Enabled = false;
            clearGenerateButton.Enabled = false;
            clearTranslateButton.Enabled = false;
            generateButton.Enabled = false;
            translateButton.Enabled = false;
            validateButton.Enabled = false;
        }

        private void EnableUI()
        {
            addFilesToGenerateButton.Enabled = true;
            addFilesToTranslateButton.Enabled = true;
            clearGenerateButton.Enabled = true;
            clearTranslateButton.Enabled = true;
            generateButton.Enabled = true;
            translateButton.Enabled = true;
            validateButton.Enabled = true;
        }
    }
}
