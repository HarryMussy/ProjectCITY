using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;

namespace CitySkylines0._5alphabeta
{
    public class NecessitiesManager
    {
        public float globalPowerSupply { get; set; }
        public float globalWaterSupply { get; set; }

        public float globalPowerDemand { get; set; }
        public float globalWaterDemand { get; set; }

        [JsonIgnore] public string globalPowerStatus;
        [JsonIgnore] public string globalWaterStatus;

        [JsonIgnore] public Grid grid;

        [JsonIgnore] public Dictionary<string, Image> NecessityImages = new Dictionary<string, Image>();

        public NecessitiesManager() { }
        public NecessitiesManager(Grid g)
        {
            globalPowerDemand = 0;
            globalWaterDemand = 0;
            globalPowerSupply = 0;
            globalWaterSupply = 0;

            globalPowerStatus = $"{globalPowerDemand} / {globalPowerSupply}MW";
            globalWaterStatus = $"{globalWaterDemand} / {globalWaterSupply}L";

            grid = g;
            LoadNecessityImages();
        }

        public void InitialiseAfterBoot(Grid g)
        {
            grid = g;
            globalPowerStatus = $"{globalPowerDemand} / {globalPowerSupply}MW";
            globalWaterStatus = $"{globalWaterDemand} / {globalWaterSupply}L";
            LoadNecessityImages();
        }

        public void LoadNecessityImages()
        {
            string projectRoot = AppContext.BaseDirectory;
            string necessityFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Icons");

            foreach (string path in Directory.GetFiles(necessityFolder, "*.png"))
            {
                //load each icon into a 32bpp ARGB bitmap to preserve transparency
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

        //calculates total global supply and demand for power and water, then marks each building's necessities as fulfilled or not
        public void UpdateGlobalNecessities()
        {
            globalPowerSupply = 0;
            globalWaterSupply = 0;
            globalPowerDemand = 0;
            globalWaterDemand = 0;

            foreach (Building b in grid.buildings)
            {
                //efficiency scales with workforce: a fully staffed plant produces at double base output
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

            //distribute available supply to buildings in order. first come first served
            //once supply runs out, remaining buildings are marked as unfulfilled
            float availablePower = globalPowerSupply;
            float availableWater = globalWaterSupply;

            foreach (Building b in grid.buildings)
            {
                foreach (Necessity necessity in b.necessities)
                {
                    if (necessity.type == "Power")
                    {
                        if (b.type == "powerplant")
                        {
                            necessity.fulFilled = true;
                            continue;
                        }

                        if (availablePower >= necessity.demand)
                        {
                            necessity.fulFilled = true;
                            availablePower -= necessity.demand;
                        }
                        else
                        {
                            necessity.fulFilled = false;
                        }
                    }

                    else if (necessity.type == "Water")
                    {
                        if (b.type == "waterpump")
                        {
                            necessity.fulFilled = true;
                            continue;
                        }

                        if (availableWater >= necessity.demand)
                        {
                            necessity.fulFilled = true;
                            availableWater -= necessity.demand;
                        }
                        else
                        {
                            necessity.fulFilled = false;
                        }
                    }

                    else if (necessity.type == "Workers")
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
        public float demand { get; set; }
        public float decayRate { get; set; }

        [JsonIgnore] public Image image { get; set; }

        static Font font = new Font("Segoe UI", 8, FontStyle.Bold);
        static Brush brush = new SolidBrush(Color.White);
        static Brush brushOutline = new SolidBrush(Color.FromArgb(60, 60, 60));
        public bool fulFilled { get; set; }

        public Necessity() { } //required for JSON deserialization

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

        //draws the necessity icon above the building, shows a tooltip label when the mouse is nearby
        public void DrawNecessity(object? sender, Graphics g, Point mousePos, Point pos)
        {
            if (!this.fulFilled)
            {
                if (this.type == "Health")
                {
                    if (mousePos.X >= pos.X - 4 && mousePos.X <= pos.X + 4 && mousePos.Y >= pos.Y - 4 && mousePos.Y <= pos.Y + 4 && image != null)
                    {
                        AddStrokeToText(sender, g, "House is ill", 1, font, brushOutline, new Point(pos.X, pos.Y - 19));
                        g.DrawString("House is ill", font, brush, pos.X, pos.Y - 19);
                        g.DrawImage(image, pos.X, pos.Y, 16, 16);
                    }

                    else if (image != null)
                    {
                        g.DrawImage(image, pos.X, pos.Y, 8, 8);
                    }
                }
                else if (this.type == "Crime")
                {
                    if (mousePos.X >= pos.X - 4 && mousePos.X <= pos.X + 4 && mousePos.Y >= pos.Y - 4 && mousePos.Y <= pos.Y + 4 && image != null)
                    {
                        AddStrokeToText(sender, g, "Is experiencing " + this.type, 1, font, brushOutline, new Point(pos.X, pos.Y - 11));
                        g.DrawString("Is experiencing " + this.type, font, brush, pos.X, pos.Y - 11);
                        g.DrawImage(image, pos.X, pos.Y, 16, 16);
                    }

                    else if (image != null)
                    {
                        g.DrawImage(image, pos.X, pos.Y, 8, 8);
                    }
                }
                else
                {
                    if (mousePos.X >= pos.X - 4 && mousePos.X <= pos.X + 4 && mousePos.Y >= pos.Y - 4 && mousePos.Y <= pos.Y + 4 && image != null)
                    {
                        AddStrokeToText(sender, g, "Needs: " + this.type, 1, font, brushOutline, new Point(pos.X, pos.Y - 11));
                        g.DrawString("Needs: " + this.type, font, brush, pos.X, pos.Y - 11);
                        g.DrawImage(image, pos.X, pos.Y, 16, 16);
                    }

                    else if (image != null)
                    {
                        g.DrawImage(image, pos.X, pos.Y, 8, 8);
                    }
                }
            }
        }

        void AddStrokeToText(object? sender, Graphics g, string text, int strokeWidth, Font font, Brush brush, Point point)
        {
            for (float dx = -strokeWidth; dx <= strokeWidth; dx++)
            {
                for (float dy = -strokeWidth; dy <= strokeWidth; dy++)
                {
                    if (dx != 0 || dy != 0)
                    {
                        g.DrawString(text, font, brush, point.X + dx, point.Y + dy);
                    }
                }
            }
        }
    }

    public class Power : Necessity
    {
        public Power() { type = "Power"; } // Ensure type is set
        public Power(float demandIN) : base("Power", 0, 0, demandIN) { }
    }

    public class Water : Necessity
    {
        public Water() { type = "Water"; }
        public Water(float demandIN) : base("Water", 0, 0, demandIN) { }
    }

    public class Workers : Necessity
    {
        public Workers() { type = "Workers"; }
        public Workers(float demandIN) : base("Workers", 0, 0, demandIN) { }
    }

    public class Unhealthy : Necessity
    {
        public Unhealthy() { type = "Ill"; }
        public Unhealthy(float demandIN) : base("Ill", 0, 0, demandIN) { }
    }

    public class Crime : Necessity
    {
        public Crime() { type = "Crime"; }
        public Crime(float demandIN) : base("Crime", 0, 0, demandIN) { }
    }

    public class Fire : Necessity
    {
        public Fire() { type = "Fire"; }
        public Fire(float demandIN) : base("Fire", 0, 0, demandIN) { }
    }
}