namespace ModTranslator
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            generateFilesContainer = new GroupBox();
            toLanguageSelect = new ComboBox();
            fromLanguageLabel = new Label();
            fromLanguageSelect = new ComboBox();
            toLanguageLabel = new Label();
            generateButton = new Button();
            selectedFilesToGeneratePanel = new FlowLayoutPanel();
            addFilesToGenerateButton = new Button();
            validateButton = new Button();
            clearGenerateButton = new Button();
            translateFilesContainer = new GroupBox();
            addFilesToTranslateButton = new Button();
            selectedFilesToTranslatePanel = new FlowLayoutPanel();
            translateButton = new Button();
            clearTranslateButton = new Button();
            appContainer = new Panel();
            generateFilesContainer.SuspendLayout();
            translateFilesContainer.SuspendLayout();
            appContainer.SuspendLayout();
            SuspendLayout();
            // 
            // generateFilesContainer
            // 
            generateFilesContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            generateFilesContainer.AutoSize = true;
            generateFilesContainer.Controls.Add(clearGenerateButton);
            generateFilesContainer.Controls.Add(validateButton);
            generateFilesContainer.Controls.Add(addFilesToGenerateButton);
            generateFilesContainer.Controls.Add(selectedFilesToGeneratePanel);
            generateFilesContainer.Controls.Add(generateButton);
            generateFilesContainer.Controls.Add(toLanguageLabel);
            generateFilesContainer.Controls.Add(fromLanguageSelect);
            generateFilesContainer.Controls.Add(fromLanguageLabel);
            generateFilesContainer.Controls.Add(toLanguageSelect);
            generateFilesContainer.Location = new Point(0, 3);
            generateFilesContainer.Name = "generateFilesContainer";
            generateFilesContainer.Size = new Size(640, 240);
            generateFilesContainer.TabIndex = 0;
            generateFilesContainer.TabStop = false;
            generateFilesContainer.Text = "Generate files to translate";
            // 
            // toLanguageSelect
            // 
            toLanguageSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            toLanguageSelect.FormattingEnabled = true;
            toLanguageSelect.Location = new Point(473, 130);
            toLanguageSelect.Name = "toLanguageSelect";
            toLanguageSelect.Size = new Size(140, 23);
            toLanguageSelect.TabIndex = 2;
            // 
            // fromLanguageLabel
            // 
            fromLanguageLabel.AutoSize = true;
            fromLanguageLabel.Location = new Point(377, 92);
            fromLanguageLabel.Name = "fromLanguageLabel";
            fromLanguageLabel.Size = new Size(90, 15);
            fromLanguageLabel.TabIndex = 4;
            fromLanguageLabel.Text = "From Language";
            // 
            // fromLanguageSelect
            // 
            fromLanguageSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            fromLanguageSelect.FormattingEnabled = true;
            fromLanguageSelect.Location = new Point(473, 89);
            fromLanguageSelect.Name = "fromLanguageSelect";
            fromLanguageSelect.Size = new Size(140, 23);
            fromLanguageSelect.TabIndex = 1;
            // 
            // toLanguageLabel
            // 
            toLanguageLabel.AutoSize = true;
            toLanguageLabel.Location = new Point(392, 133);
            toLanguageLabel.Name = "toLanguageLabel";
            toLanguageLabel.Size = new Size(75, 15);
            toLanguageLabel.TabIndex = 4;
            toLanguageLabel.Text = "To Language";
            // 
            // generateButton
            // 
            generateButton.Location = new Point(403, 178);
            generateButton.Name = "generateButton";
            generateButton.Size = new Size(75, 23);
            generateButton.TabIndex = 0;
            generateButton.Text = "Generate";
            generateButton.UseVisualStyleBackColor = true;
            generateButton.Click += GenerateButton_Click;
            // 
            // selectedFilesToGeneratePanel
            // 
            selectedFilesToGeneratePanel.AutoScroll = true;
            selectedFilesToGeneratePanel.BackColor = SystemColors.ControlLightLight;
            selectedFilesToGeneratePanel.BorderStyle = BorderStyle.FixedSingle;
            selectedFilesToGeneratePanel.Location = new Point(12, 22);
            selectedFilesToGeneratePanel.Name = "selectedFilesToGeneratePanel";
            selectedFilesToGeneratePanel.Size = new Size(311, 179);
            selectedFilesToGeneratePanel.TabIndex = 6;
            // 
            // addFilesToGenerateButton
            // 
            addFilesToGenerateButton.Location = new Point(329, 22);
            addFilesToGenerateButton.Name = "addFilesToGenerateButton";
            addFilesToGenerateButton.Size = new Size(75, 23);
            addFilesToGenerateButton.TabIndex = 4;
            addFilesToGenerateButton.Text = "Add Files";
            addFilesToGenerateButton.UseVisualStyleBackColor = true;
            addFilesToGenerateButton.Click += AddFilesToGenerateButton_Click;
            // 
            // validateButton
            // 
            validateButton.Location = new Point(499, 178);
            validateButton.Name = "validateButton";
            validateButton.Size = new Size(75, 23);
            validateButton.TabIndex = 7;
            validateButton.Text = "Validate";
            validateButton.UseVisualStyleBackColor = true;
            validateButton.Click += ValidateButton_Click;
            // 
            // clearGenerateButton
            // 
            clearGenerateButton.Location = new Point(329, 51);
            clearGenerateButton.Name = "clearGenerateButton";
            clearGenerateButton.Size = new Size(75, 23);
            clearGenerateButton.TabIndex = 8;
            clearGenerateButton.Text = "Clear";
            clearGenerateButton.UseVisualStyleBackColor = true;
            clearGenerateButton.Click += ClearGenerateButton_Click;
            // 
            // translateFilesContainer
            // 
            translateFilesContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            translateFilesContainer.AutoSize = true;
            translateFilesContainer.Controls.Add(clearTranslateButton);
            translateFilesContainer.Controls.Add(translateButton);
            translateFilesContainer.Controls.Add(selectedFilesToTranslatePanel);
            translateFilesContainer.Controls.Add(addFilesToTranslateButton);
            translateFilesContainer.Location = new Point(0, 249);
            translateFilesContainer.Name = "translateFilesContainer";
            translateFilesContainer.Size = new Size(640, 226);
            translateFilesContainer.TabIndex = 1;
            translateFilesContainer.TabStop = false;
            translateFilesContainer.Text = "Translate files";
            // 
            // addFilesToTranslateButton
            // 
            addFilesToTranslateButton.Location = new Point(329, 22);
            addFilesToTranslateButton.Name = "addFilesToTranslateButton";
            addFilesToTranslateButton.Size = new Size(75, 23);
            addFilesToTranslateButton.TabIndex = 0;
            addFilesToTranslateButton.Text = "Add Files";
            addFilesToTranslateButton.UseVisualStyleBackColor = true;
            addFilesToTranslateButton.Click += AddFilesToTranslateButton_Click;
            // 
            // selectedFilesToTranslatePanel
            // 
            selectedFilesToTranslatePanel.AutoScroll = true;
            selectedFilesToTranslatePanel.BackColor = SystemColors.ControlLightLight;
            selectedFilesToTranslatePanel.BorderStyle = BorderStyle.FixedSingle;
            selectedFilesToTranslatePanel.Location = new Point(12, 22);
            selectedFilesToTranslatePanel.Name = "selectedFilesToTranslatePanel";
            selectedFilesToTranslatePanel.Size = new Size(311, 179);
            selectedFilesToTranslatePanel.TabIndex = 2;
            // 
            // translateButton
            // 
            translateButton.Location = new Point(329, 178);
            translateButton.Name = "translateButton";
            translateButton.Size = new Size(75, 23);
            translateButton.TabIndex = 3;
            translateButton.Text = "Translate";
            translateButton.UseVisualStyleBackColor = true;
            translateButton.Click += TranslateButton_Click;
            // 
            // clearTranslateButton
            // 
            clearTranslateButton.Location = new Point(329, 51);
            clearTranslateButton.Name = "clearTranslateButton";
            clearTranslateButton.Size = new Size(75, 23);
            clearTranslateButton.TabIndex = 9;
            clearTranslateButton.Text = "Clear";
            clearTranslateButton.UseVisualStyleBackColor = true;
            clearTranslateButton.Click += ClearTranslateButton_Click;
            // 
            // appContainer
            // 
            appContainer.AutoScroll = true;
            appContainer.Controls.Add(translateFilesContainer);
            appContainer.Controls.Add(generateFilesContainer);
            appContainer.Dock = DockStyle.Fill;
            appContainer.Location = new Point(0, 0);
            appContainer.Name = "appContainer";
            appContainer.Size = new Size(640, 475);
            appContainer.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(640, 475);
            Controls.Add(appContainer);
            Name = "Form1";
            Text = "Form1";
            generateFilesContainer.ResumeLayout(false);
            generateFilesContainer.PerformLayout();
            translateFilesContainer.ResumeLayout(false);
            appContainer.ResumeLayout(false);
            appContainer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox generateFilesContainer;
        private Button clearGenerateButton;
        private Button validateButton;
        private Button addFilesToGenerateButton;
        private FlowLayoutPanel selectedFilesToGeneratePanel;
        private Button generateButton;
        private Label toLanguageLabel;
        private ComboBox fromLanguageSelect;
        private Label fromLanguageLabel;
        private ComboBox toLanguageSelect;
        private GroupBox translateFilesContainer;
        private Button clearTranslateButton;
        private Button translateButton;
        private FlowLayoutPanel selectedFilesToTranslatePanel;
        private Button addFilesToTranslateButton;
        private Panel appContainer;
    }
}
