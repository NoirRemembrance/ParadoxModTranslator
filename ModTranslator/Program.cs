using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModTranslator.BLL.Services.GenerateFilesForTranslation;
using ModTranslator.BLL.Services.TranslateFiles;
using ModTranslator.BLL.Services.ValidateFiles;
using ModTranslator.BO.Objects.Settings;

namespace ModTranslator
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            AppSettings appSettings = new();
            configuration.Bind(appSettings);

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton(appSettings)
                .AddSingleton<IGenerateFileToTranslateService, GenerateFileToTranslateService>()
                .AddSingleton<ITranslateFilesService, TranslateFilesService>()
                .AddSingleton<IValidateFilesService, ValidateFilesService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<Form1>()
                .BuildServiceProvider();

            Form1 form = serviceProvider.GetRequiredService<Form1>();
            Application.Run(form);
        }
    }
}