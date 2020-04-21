using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotFramework
{
    /// <summary>
    /// Functions for multi-language support with ini files mode
    /// </summary>
    public class Languages
    {
        /// <summary>
        /// The path to language ini files
        /// </summary>
        public string LanguagePath {private get; set;}
        /// <summary>
        /// The ini files detected after LoadLanguages
        /// </summary>
        public string[] LanguageInis { get; private set; }

        private Dictionary<string, string> LanguageKeyValue = new Dictionary<string, string>();
        /// <summary>
        /// The singleton instance
        /// </summary>
        public static Languages Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new Languages();
                }
                return instance;
            }
        }

        private static Languages instance;
        /// <summary>
        /// Load language files in LanguagePath
        /// </summary>
        /// <param name="language">Default Language file to load</param>
        public static string[] GetLanguages(string language)
        {
            Instance.LanguageKeyValue.Clear();
            if (Instance.LanguagePath == null)
            {
                throw new ArgumentException("LanguagePath is not set yet!");
            }
            Instance.LanguageInis = Directory.GetFiles(Instance.LanguagePath,"*.ini");
            string[] selectedfile = Instance.LanguageInis.Where(ini => ini.Contains(language)).ToArray();
            if (selectedfile.Count() < 1)//No defult language exist
            {
                throw new FileNotFoundException("Default Language file not found!");
            }
            else
            {
                var temp = File.ReadAllLines(selectedfile[0]);
                foreach(var line in temp)
                {
                    var splitted = line.Split('=');
                    if(splitted.Count() == 2)
                    {
                        Instance.LanguageKeyValue.Add(splitted[0],splitted[1]);
                    }
                }
            }
            return Instance.LanguageInis;
        }
        /// <summary>
        /// Load new language and overwrite old one
        /// </summary>
        /// <param name="languagepath">The path to new language, can be loaded from GetLanguage</param>
        public static void LoadLanguage(string languagepath)
        {
            Instance.LanguageKeyValue.Clear();
            var temp = File.ReadAllLines(languagepath);
            foreach (var line in temp)
            {
                var splitted = line.Split('=');
                if (splitted.Count() == 2)
                {
                    Instance.LanguageKeyValue.Add(splitted[0], splitted[1]);
                }
            }
        }
        /// <summary>
        /// Load new language and overwrite old one
        /// </summary>
        /// <param name="languageindex">The index number of language which loaded from GetLanguage</param>
        public static void LoadLanguage(int languageindex)
        {
            LoadLanguage(Instance.LanguageInis[languageindex]);
        }
        /// <summary>
        /// Get value after LoadLanguage
        /// </summary>
        /// <param name="key">key for getting value string</param>
        /// <param name="throw_exception">throw exception if not found or just return key</param>
        /// <returns>Value string</returns>
        public static string GetText(string key, bool throw_exception = false)
        {
            if (!Instance.LanguageKeyValue.ContainsKey(key))
            {
                if (!throw_exception)
                {
                    return key;
                }
                else
                {
                    throw new ArgumentException(key + " key not found!");
                }
            }
            else
            {
                return Instance.LanguageKeyValue[key];
            }
        }
    }
}
