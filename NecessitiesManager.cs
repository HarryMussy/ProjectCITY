using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;

namespace CitySkylines0._5alphabeta
{
    public class NecessitiesManager
    {
        public float globalPowerSupply;
        public float globalWaterSupply;

        public float globalPowerDemand;
        public float globalWaterDemand;

        public string globalPowerStatus;
        public string globalWaterStatus;

        public Grid grid;

        public Dictionary<string, Image> NecessityImages = new Dictionary<string, Image>();
        public NecessitiesManager(Grid grid)
        {
            globalPowerDemand = 0;
            globalWaterDemand = 0;
            globalPowerSupply = 0;
            globalWaterSupply = 0;

            globalPowerStatus = $"{globalPowerDemand} / {globalPowerSupply}MW";
            globalWaterStatus = $"{globalWaterDemand} / {globalWaterSupply}L";

            this.grid = grid;
            LoadNecessityImages();
        }

        public void LoadNecessityImages()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string necessityFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Icons");

            foreach (string path in Directory.GetFiles(necessityFolder, "*.png"))
            {
                using var original = Image.FromFile(path);
                Bitmap bmp = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(original, 0, 0, original.Width, original.Height);
                }

                string key = Path.GetFileName(path);
                NecessityImages[key] = bmp;
            }
        }

        public void UpdateGlobalNecessities()
        {
            foreach (Building b in grid.buildings)
            {
                int workers = b.Occupants.Count(p => p != null);
                b.efficiency = (float)workers / b.MaxOccupants * 2f; //at 100% workers, the plant doubles its efficiency

                foreach (Necessity necessity in b.necessities)
                {
                    necessity.image = NecessityImages[necessity.GetNecessityType() + ".png"];

                    if (necessity.type is "Power")
                    {
                        if (b.type is "powerplant") //if it's a powerplant
                        {
                            globalPowerSupply += necessity.demand * b.efficiency;
                            necessity.fulFilled = true;
                        }
                        else
                        {
                            globalPowerDemand += necessity.demand;
                        }
                    }

                    else if (necessity.type is "Water")
                    {
                        if (b.type is "waterpump") //if it's a water pump
                        {
                            globalWaterSupply += necessity.demand * b.efficiency;
                            necessity.fulFilled = true;
                        }
                        else
                        {
                            globalWaterDemand += necessity.demand;
                        }
                    }
                }
            }

            foreach (Building b in grid.buildings)
            {
                foreach (Necessity necessity in b.necessities)
                {
                    if (necessity.type is "Power")
                    {
                        if (globalPowerSupply < globalPowerDemand && b.type != "powerplant")
                        {
                            necessity.fulFilled = false;
                        }
                        else
                        {
                            necessity.fulFilled = true;
                        }
                    }
                    else if (necessity.type is "Water")
                    {

                        if (globalWaterSupply < globalWaterDemand && b.type != "waterpump")
                        {
                            necessity.fulFilled = false;
                        }
                        else
                        {
                            necessity.fulFilled = true;
                        }
                    }
                    else if (necessity.type is "Workers")
                    {
                        necessity.fulFilled = b.Occupants.Count(p => p != null) > 0;
                    }
                }
            }

            globalPowerStatus = $"{globalPowerDemand} / {globalPowerSupply}MW";
            globalWaterStatus = $"{globalWaterDemand} / {globalWaterSupply}L";
        }
            
    }

    public class Necessity
    {
        public string type { get; set; }
        public float demand { get; private set; }
        public float decayRate { get; private set; } //the time it takes (in seconds) for the building to be abandoned

        [JsonIgnore] public Image image { get; set; }

        public bool fulFilled { get; set; }
        public Necessity() { }

        public Necessity(string typeIn, float initialValueIN, float decayRateIN, float demandIN)
        {
            type = typeIn;
            decayRate = decayRateIN;
            demand = demandIN;
        }

        public string GetNecessityType()
        {
            return type;
        }

        public void DrawNecessity(object? sender, Graphics g, Point mousePos, Point pos)
        {
            if (!this.fulFilled)
            {
                if (this.type == "Health")
                {
                    if (mousePos.X >= pos.X - 4 && mousePos.X <= pos.X + 4
                        && mousePos.Y >= pos.Y - 4 && mousePos.Y <= pos.Y + 4 && image != null)
                    {
                        g.DrawString("House is ill", new Font("Segoe UI", 8, FontStyle.Bold), new SolidBrush(Color.White), pos.X, pos.Y - 19);
                        g.DrawImage(image, pos.X, pos.Y, 16, 16);
                    }
                    else if (image != null)
                    {
                        g.DrawImage(image, pos.X, pos.Y, 8, 8);
                    }
                }
                else
                {
                    if (mousePos.X >= pos.X - 4 && mousePos.X <= pos.X + 4
                        && mousePos.Y >= pos.Y - 4 && mousePos.Y <= pos.Y + 4 && image != null)
                    {

                        g.DrawString("Needs: " + this.type, new Font("Segoe UI", 8, FontStyle.Bold), new SolidBrush(Color.White), pos.X, pos.Y - 11);
                        g.DrawImage(image, pos.X, pos.Y, 16, 16);
                    }
                    else if (image != null)
                    {
                        g.DrawImage(image, pos.X, pos.Y, 8, 8);
                    }
                }
            }
        }
    }

    public class Power : Necessity
    {
        public Power() { }
        public Power(float demandIN) : base("Power", 0, 0, demandIN) { }
    }

    public class Water : Necessity
    {
        public Water() { }
        public Water(float demandIN) : base("Water", 0, 0, demandIN) { }
    }

    public class Workers : Necessity
    {
        public Workers() { }
        public Workers(float demandIN) : base("Workers", 0, 0, demandIN) { }
    }

    public class Unhealthy : Necessity
    {
        public Unhealthy() { }
        public Unhealthy(float demandIN) : base("Ill", 0, 0, demandIN) { }
    }
}
