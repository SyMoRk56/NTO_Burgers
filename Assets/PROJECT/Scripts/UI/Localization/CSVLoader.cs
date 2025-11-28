using System.Collections.Generic;
using UnityEngine;

public static class CSVLoader
{
    public static Dictionary<string, Dictionary<string, string>> LoadLocalizationCSV(string path)
    {
        TextAsset csvData = Resources.Load<TextAsset>(path);

        if (csvData == null)
        {
            Debug.LogError("CSV file not found at Resources/" + path);
            return new Dictionary<string, Dictionary<string, string>>();
        }

        var result = new Dictionary<string, Dictionary<string, string>>();

        string[] lines = csvData.text.Split('\n');

        string[] headers = lines[0].Trim('\ufeff').Trim().Split(';');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] fields = line.Split(';');

            string key = fields[0].Trim();
            var dict = new Dictionary<string, string>();

            for (int h = 1; h < headers.Length; h++)
            {
                string lang = headers[h];
                string text = fields.Length > h ? fields[h] : "";
                dict[lang] = text;
            }

            result[key] = dict;
        }

        return result;
    }
}
