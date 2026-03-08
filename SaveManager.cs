using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace CitySkylines0._5alphabeta
{
    public static class SaveManager
    {
        // Minimal save data. Extend this to include the full grid, buildings, cars, etc.
        public class SaveData
        {
            public Grid grid { get; set; }
            public Calendar calendar { get; set; }
            public Background background { get; set; }
            public List<Person> population { get; set; }
            public float averageWellBeing { get; set; }
            public Dictionary<string, int> globalDesires { get; set; }
            public float globalPowerDemand { get; set; }
            public float globalPowerSupply { get; set; }
            public float globalWaterDemand { get; set; }
            public float globalWaterSupply { get; set; }
        }

        public static void Save(string filePath, Grid grid, Calendar calendar, Background background, PopulationManager populationManager, NecessitiesManager necessitiesManager)
        {
            var data = new SaveData
            {
                grid = grid,
                calendar = calendar,
                population = populationManager.Population,
                background = background,
                averageWellBeing = populationManager.AverageWellBeing,
                globalDesires = populationManager.GlobalDesires,
                globalPowerDemand = necessitiesManager.globalPowerDemand,
                globalPowerSupply = necessitiesManager.globalPowerSupply,
                globalWaterDemand = necessitiesManager.globalWaterDemand,
                globalWaterSupply = necessitiesManager.globalWaterSupply
            };

            var json = JsonSerializer.Serialize(data, JsonSettings.Options);
            File.WriteAllText(filePath, json);
        }

        public static SaveData Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<SaveData>(json, JsonSettings.Options);
        }

        // Opens a .citysave open dialog and returns SaveData or null if cancelled
        public static SaveData LoadGameFromFile()
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "City Save (*.citysave)|*.citysave";
            dialog.DefaultExt = "citysave";
            dialog.CheckFileExists = true;
            dialog.Title = "Load Game";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = dialog.FileName;
                try
                {
                    var data = Load(filePath);
                    MessageBox.Show("Loaded: " + filePath);
                    return data;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load save: " + ex.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        // Prompts SaveFileDialog for a .citysave file and saves
        public static void SaveGameToFile(Grid grid, Calendar calendar, Background background, PopulationManager populationManager, NecessitiesManager necessitiesManager)
        {
            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "City Save (*.citysave)|*.citysave";
            dialog.DefaultExt = "citysave";
            dialog.AddExtension = true;
            dialog.Title = "Save Game";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = dialog.FileName;
                try
                {
                    Save(filePath, grid, calendar, background, populationManager, necessitiesManager);
                    MessageBox.Show("Saved: " + filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save game: " + ex.Message);
                }
            }
        }

        // Save to a provided path; ensures .citysave extension if missing
        public static void SaveGameToFile(string path, Grid grid, Calendar calendar, Background background, PopulationManager populationManager, NecessitiesManager necessitiesManager)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                // fallback to interactive save
                SaveGameToFile(grid, calendar, background, populationManager, necessitiesManager);
                return;
            }

            try
            {
                string filePath = path;
                if (Path.GetExtension(filePath).ToLowerInvariant() != ".citysave")
                {
                    filePath = Path.ChangeExtension(filePath, ".citysave");
                }

                Save(filePath, grid, calendar, background, populationManager, necessitiesManager);
                MessageBox.Show("Saved: " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save game: " + ex.Message);
            }
        }
    }
}
