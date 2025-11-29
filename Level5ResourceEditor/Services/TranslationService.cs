using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Level5ResourceEditor.Services
{
    public class TranslationService
    {
        private static TranslationService _instance;
        public static TranslationService Instance => _instance ?? (_instance = new TranslationService());

        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> _translations;
        public string CurrentLanguage { get; private set; }

        public event EventHandler LanguageChanged;

        private TranslationService()
        {
            _translations = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        }

        public void Initialize()
        {
            // Detect the system language
            string systemLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            // Map language codes
            CurrentLanguage = MapLanguageCode(systemLanguage);

            // Load translations
            LoadTranslations();
        }

        private string MapLanguageCode(string systemLanguage)
        {
            var supportedLanguages = new[] { "de", "en", "es", "fr", "it", "ja", "pt", "zh_hans", "zh_hant" };

            if (supportedLanguages.Contains(systemLanguage))
                return systemLanguage;

            // Chinese
            if (systemLanguage == "zh")
            {
                var culture = CultureInfo.CurrentUICulture.Name;
                return culture.Contains("Hans") || culture.Contains("CN") ? "zh_hans" : "zh_hant";
            }

            // We use English as the default language
            return "en";
        }

        private void LoadTranslations()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                // Get the root namespace automatically
                string rootNamespace = assembly.GetName().Name;
                string translationsPrefix = $"{rootNamespace}.Resources.Translations.";

                // Retrieve all embedded resources that match the pattern
                var resourceNames = assembly.GetManifestResourceNames()
                    .Where(name => name.StartsWith(translationsPrefix) && name.EndsWith(".json"));

                foreach (var resourceName in resourceNames)
                {
                    // Extract the category and file name
                    var parts = resourceName.Replace(translationsPrefix, "")
                                           .Replace(".json", "")
                                           .Split('.');

                    if (parts.Length >= 2)
                    {
                        string category = parts[0];
                        string fileName = string.Join(".", parts.Skip(1));

                        LoadTranslationFile(category, fileName, resourceName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading translations: {ex.Message}");
            }
        }

        private void LoadTranslationFile(string category, string fileName, string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string json = reader.ReadToEnd();
                            var translations = JsonConvert.DeserializeObject<List<TranslationEntry>>(json);

                            string key = $"{category}.{fileName}";
                            if (!_translations.ContainsKey(key))
                                _translations[key] = new Dictionary<string, Dictionary<string, string>>();

                            foreach (var entry in translations)
                            {
                                _translations[key][entry.Id] = entry.Names;
                            }

                            Debug.WriteLine($"Loaded translation file: {category}.{fileName}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Resource not found: {resourceName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading translation file {category}.{fileName}: {ex.Message}");
            }
        }

        public void ChangeLanguage(string languageCode)
        {
            if (CurrentLanguage != languageCode)
            {
                CurrentLanguage = languageCode;
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string GetTranslation(string category, string id)
        {
            try
            {
                if (_translations.ContainsKey(category) &&
                    _translations[category].ContainsKey(id) &&
                    _translations[category][id].ContainsKey(CurrentLanguage))
                {
                    return _translations[category][id][CurrentLanguage];
                }

                // Fallback to English if no translation exists
                if (_translations.ContainsKey(category) &&
                    _translations[category].ContainsKey(id) &&
                    _translations[category][id].ContainsKey("en"))
                {
                    return _translations[category][id]["en"];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Translation error: {ex.Message}");
            }

            return $"[{id}]";
        }

        public string GetTranslation(string category, string id, object parameters)
        {
            string text = GetTranslation(category, id);
            if (parameters == null)
                return text;

            foreach (PropertyInfo prop in parameters.GetType().GetProperties())
            {
                string placeholder = "{" + prop.Name + "}";
                string value = prop.GetValue(parameters)?.ToString() ?? "";
                text = text.Replace(placeholder, value);
            }

            return text;
        }

        private class TranslationEntry
        {
            public string Id { get; set; }
            public Dictionary<string, string> Names { get; set; }
        }
    }
}
