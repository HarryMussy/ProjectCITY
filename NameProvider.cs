using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CitySkylines0._5alphabeta
{
    public class NameList
    {
        [JsonPropertyName("forenames")]
        public List<string> Forenames { get; set; }
        [JsonPropertyName("surnames")]
        public List<string> Surnames { get; set; }
    }

    public class NameProvider
    {
        private List<string> invalidnames; // Global list to store invalid names
        private readonly NameList nameList;

        public NameProvider(string filePath)
        {
            invalidnames = new List<string>();

            // Read the single JSON file and deserialize it using System.Text.Json
            try
            {
                string json = File.ReadAllText(filePath);

                // Deserialize JSON into NameList object
                nameList = JsonSerializer.Deserialize<NameList>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading JSON file: {ex.Message}");
            }
        }

        public string GetRandomName()
        {
            string name = "";
            bool validity = false;
            while (validity == false)
            {
                Random rng = new Random();
                string genname = nameList.Forenames[rng.Next(0, nameList.Forenames.Count)] + " " + nameList.Surnames[rng.Next(0, nameList.Surnames.Count)];

                // Check if the generated name is valid (not in invalidnames)
                if (invalidnames.Contains(genname))
                {
                    validity = false;
                }
                else
                {
                    invalidnames.Add(genname); // Add valid name to invalidnames list
                    name = genname;  // Assign the valid name
                    validity = true; // Exit the loop as a valid name was found
                }
            }
            return name;
        }
    }
}
