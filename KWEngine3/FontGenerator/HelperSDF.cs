using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KWEngine3.FontGenerator
{
    internal static class HelperSDF
    {
        public static AtlasRoot GenerateFontDictionaryFromDisk(string jsonfile)
        {
            using (FileStream fs = File.Open(jsonfile, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string json = sr.ReadToEnd();
                    return GenerateFontDictionary(json);
                }
            }
        }

        public static AtlasRoot GenerateFontDictionaryFromAssembly(string assemblyfile)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream(assemblyfile))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    string json = sr.ReadToEnd();
                    return GenerateFontDictionary(json);
                }
            }
        }

        private static AtlasRoot GenerateFontDictionary(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            options.Converters.Add(new JsonStringEnumConverter());

            AtlasRoot data = JsonSerializer.Deserialize<AtlasRoot>(json, options)!;
            return data;
        }
    }
}
