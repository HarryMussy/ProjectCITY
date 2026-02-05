using System.IO;

namespace CitySkylines0._5alphabeta
{
    public class BuildingPainter
    {
        private readonly Grid grid;
        public Form1 Form1;
        public Point screencentre;

        private List<Image> houseImages;
        private Image powerPlantImage;
        private Image waterPumpImage;
        private Dictionary<Building, int> tileHouseImageIndex = new();
        private Random random = new Random();
        public AudioManager audioManager;
        public SmokeParticleManager smokeParticleManager;
        public Graphics g;
        public string buildingType = "";
        private int rectSize;
        public Calendar calendar;
        Size houseSize;
        Size powerPlantSize;
        Size waterPumpSize;


        private readonly Brush invalidBrushBuilding = new SolidBrush(Color.FromArgb(200, Color.DarkRed));
        private readonly Brush validBrushBuilding = new SolidBrush(Color.FromArgb(200, Color.Green));
        private readonly Brush moneyCostBrush = new SolidBrush(Color.Black);
        private readonly Brush moneyCostBrushSpace = new SolidBrush(Color.DarkRed);
        private readonly Font font = new Font("Comic Sans", 11);
        private readonly Font font2 = new Font("Comic Sans", 9);


        public BuildingPainter(Grid gridPassIn, Form1 Form1PassIn, Graphics g, int rectSizeIn, Calendar calendar)
        {
            rectSize = rectSizeIn;
            this.g = g;
            grid = gridPassIn;
            Form1 = Form1PassIn;
            LoadBuildingImages();
            audioManager = Form1.audioManager;
            smokeParticleManager = Form1.smokeParticleManager;
            this.calendar = calendar;

            houseSize = new Size(2, 2);
            powerPlantSize = new Size(4, 3);
            waterPumpSize = new Size(2, 2);
        }

        public void BuildingPaint(object? sender, Graphics g, Point mousePos)
        {
            if (Form1.selectingBuildingPainting == true)
            {
                if (buildingType == "house")
                {
                    if (grid.cash >= 10000)
                    {
                        foreach (Node node in grid.nodes)
                        {
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, houseSize.Width, houseSize.Height);
                            if (isTrue == 0)
                            {
                                g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize);
                                g.DrawString("-£10000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10);
                            }
                            else if (isTrue == 1)
                            {
                                g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize);
                            }
                        }
                    }
                    else
                    {
                        foreach (Node node in grid.nodes)
                        {
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, houseSize.Width, houseSize.Height);
                            if (isTrue == 0 || isTrue == 1)
                            {
                                g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize);
                            }
                        }
                        g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29);
                    }
                }

                else if (buildingType == "powerplant")
                {
                    if (grid.cash >= 50000)
                    {
                        foreach (Node node in grid.nodes)
                        {
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, powerPlantSize.Width, powerPlantSize.Height);
                            if (isTrue == 0)
                            {
                                g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize);
                                g.DrawString("-£50000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10);
                            }
                            else if (isTrue == 1)
                            {
                                g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize);
                            }
                        }
                    }
                    else
                    {
                        foreach (Node node in grid.nodes)
                        {
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, powerPlantSize.Width, powerPlantSize.Height);
                            if (isTrue == 0 || isTrue == 1)
                            {
                                g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize);
                            }
                        }
                        g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29);
                    }
                }

                else if (buildingType == "waterpump")
                {
                    if (grid.cash >= 30000)
                    {
                        foreach (Node node in grid.nodes)
                        {
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, waterPumpSize.Width, waterPumpSize.Height, true);
                            if (isTrue == 0)
                            {
                                g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize);
                                g.DrawString("-£30000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10);
                            }
                            else if (isTrue == 1)
                            {
                                g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize);
                            }
                        }
                    }
                    else
                    {
                        foreach (Node node in grid.nodes)
                        {
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, waterPumpSize.Width, waterPumpSize.Height, true);
                            if (isTrue == 0 || isTrue == 1)
                            {
                                g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize);
                            }
                        }
                        g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29);
                    }
                }
            }

            // Use the form's zoomLevel if available
            float zoom = Form1 != null ? Form1.rectSize / 200f : 1.0f;

            foreach (Building building in grid.buildings)
            {
                if (calendar.GetHour() >= 21 || calendar.GetHour() <= 5)
                {
                    using Brush glow1 = new SolidBrush(Color.FromArgb(90, 255, 255, 200));  // bright center
                    using Brush glow2 = new SolidBrush(Color.FromArgb(40, 255, 255, 200));   // mid glow
                    using Brush glow3 = new SolidBrush(Color.FromArgb(10, 255, 255, 200));   // outer glow

                    // Draw glow layers
                    g.FillEllipse(glow3, building.coords.X - 10, building.coords.Y - 10, (building.size.Width * rectSize) + 20, (building.size.Height * rectSize) + 20);
                    g.FillEllipse(glow2, building.coords.X - 5, building.coords.Y - 5, (building.size.Width * rectSize) + 10, (building.size.Height * rectSize) + 10);
                    g.FillEllipse(glow1, building.coords.X, building.coords.Y, (building.size.Width * rectSize), (building.size.Height * rectSize));
                }

                if (building.type == "house")
                {
                    int imgIdx;
                    if (!tileHouseImageIndex.TryGetValue(building, out imgIdx))
                    {
                        imgIdx = random.Next(houseImages.Count);
                        tileHouseImageIndex[building] = imgIdx;
                    }
                    // Use building.size for drawing
                    g.DrawImage(houseImages[imgIdx], building.coords.X, building.coords.Y, building.size.Width * rectSize, building.size.Height * rectSize);
                    g.DrawString(building.Occupants.Where(p => p != null).Count().ToString(), new Font("Segoe UI", 8, FontStyle.Bold), new SolidBrush(Color.White), building.coords.X, building.coords.Y + 5);

                    foreach (Person p in building.Occupants.Where(p => p != null))
                    {
                        if (p.IsHealthy == false)
                        {
                            Unhealthy ne = new Unhealthy(10);
                            ne.type = "Health";
                            ne.fulFilled = false;
                            ne.image = Form1.necessitiesManager.NecessityImages["Health.png"];
                            ne.DrawNecessity(sender, g, mousePos, new Point(building.coords.X, building.coords.Y + 8));
                        }
                    }
                }
                else if (building.type == "powerplant")
                {
                    g.DrawImage(powerPlantImage, building.coords.X, building.coords.Y, building.size.Width * rectSize, building.size.Height * rectSize);
                    /*g.DrawString($"{building.Occupants.Count(p => p != null)} / {building.MaxOccupants}\n\n {building.efficiency}", font, blueBrush, building.coords);*/
                }
                else if (building.type == "waterpump")
                {
                    g.DrawImage(waterPumpImage, building.coords.X, building.coords.Y, building.size.Width * rectSize, building.size.Height * rectSize);
                    /*g.DrawString($"{building.Occupants.Count(p => p != null)} / {building.MaxOccupants} \n\n {building.efficiency}", font, blueBrush, building.coords);*/
                }


                for (int i = 0; i < building.necessities.Count; i++)
                {
                    Necessity ne = building.necessities[i];
                    ne.DrawNecessity(sender, g, mousePos, new Point(building.coords.X + (i * 8), building.coords.Y));
                }
            }
        }

        private void LoadBuildingImages()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string houseFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Houses");
            houseImages = new List<Image>();

            foreach (string path in Directory.GetFiles(houseFolder, "*.png"))
            {
                using var original = Image.FromFile(path);
                Bitmap bmp = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(original, 0, 0, original.Width, original.Height);
                }

                houseImages.Add(bmp);
            }

            string powerPlantPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "powerPlant.png");
            powerPlantImage = Image.FromFile(powerPlantPath);

            string waterPumpPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "waterPump.png");
            waterPumpImage = Image.FromFile(waterPumpPath);
        }


        public void LeftMouseDown(object? sender, MouseEventArgs m)
        {
            List<int> checkedSpaces = new List<int>();
            List<Node> checkedNodes = new List<Node>();
            Point worldMousePos = ((Form1)sender).Mouse_Pos(sender, m);
            //var clickedPoint = new Point((int)((worldMousePos.X - screencentre.X) / zoomLevel + screencentre.X), (int)((worldMousePos.Y - screencentre.Y) / zoomLevel + screencentre.Y));
            var clickedPoint = worldMousePos;

            if (clickedPoint.X < grid.nodes.First().coords.X || clickedPoint.Y < grid.nodes.First().coords.Y || clickedPoint.X > grid.nodes.Last().coords.X || clickedPoint.Y > grid.nodes.Last().coords.Y)
            {
                return;
            }

            if (grid.cash >= 10000 && buildingType == "house")
            {
                foreach (Node node in grid.nodes)
                {
                    int isTrue = FindNearbyBuildableNodes(sender, clickedPoint, node, houseSize.Width, houseSize.Height);
                    if (isTrue == 1 || isTrue == 0) { checkedSpaces.Add(isTrue); checkedNodes.Add(node); }
                }
                if (checkedSpaces.Contains(1)) { }
                else
                {
                    Point placement = new Point(int.MaxValue, int.MaxValue);
                    House newHouse = new House(houseSize, placement, "house", 5, 5);
                    foreach (Node n in checkedNodes)
                    {
                        if (n.coords.X < placement.X && n.coords.Y < placement.Y)
                        {
                            placement = n.coords;
                        }
                        n.hasTileData = true;
                    }
                    newHouse.coords = placement;
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newHouse);
                    grid.cash -= newHouse.cost;

                    // Assign a random image index for the new house
                    int imgIdx = random.Next(houseImages.Count);
                    tileHouseImageIndex[newHouse] = imgIdx;

                    foreach (Node node in checkedNodes)
                    {
                        newHouse.occupyingNodes.Add(node);
                        node.hasTileData = true;
                    }
                    smokeParticleManager.SpawnParticlesOnBuilding(newHouse);
                }
            }

            if (grid.cash >= 50000 && buildingType == "powerplant")
            {
                foreach (Node node in grid.nodes)
                {
                    int isTrue = FindNearbyBuildableNodes(sender, clickedPoint, node, powerPlantSize.Width, powerPlantSize.Height);
                    if (isTrue == 1 || isTrue == 0) { checkedSpaces.Add(isTrue); checkedNodes.Add(node); }
                }
                if (checkedSpaces.Contains(1)) { }
                else
                {
                    Point placement = new Point(int.MaxValue, int.MaxValue);
                    PowerPlant newPowerPlant = new PowerPlant(powerPlantSize, placement, "powerplant", 500, 50);
                    foreach (Node n in checkedNodes)
                    {
                        if (n.coords.X < placement.X && n.coords.Y < placement.Y)
                        {
                            placement = n.coords;
                        }
                        n.hasTileData = true;
                    }
                    newPowerPlant.coords = placement;
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newPowerPlant);
                    grid.cash -= newPowerPlant.cost;


                    foreach (Node node in checkedNodes)
                    {
                        newPowerPlant.occupyingNodes.Add(node);
                        node.hasTileData = true;
                    }
                    smokeParticleManager.SpawnParticlesOnBuilding(newPowerPlant);
                }
            }

            if (grid.cash >= 30000 && buildingType == "waterpump")
            {
                foreach (Node node in grid.nodes)
                {
                    int isTrue = FindNearbyBuildableNodes(sender, clickedPoint, node, waterPumpSize.Width, waterPumpSize.Height, true);
                    if (isTrue == 1 || isTrue == 0) { checkedSpaces.Add(isTrue); checkedNodes.Add(node); }
                }
                if (checkedSpaces.Contains(1)) { }
                else
                {
                    Point placement = new Point(int.MaxValue, int.MaxValue);
                    WaterPump newWaterPump = new WaterPump(waterPumpSize, placement, "waterpump", 50, 250);
                    foreach (Node n in checkedNodes)
                    {
                        if (n.coords.X < placement.X && n.coords.Y < placement.Y)
                        {
                            placement = n.coords;
                        }
                        n.hasTileData = true;
                    }
                    newWaterPump.coords = placement;
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newWaterPump);
                    grid.cash -= newWaterPump.cost;


                    foreach (Node node in checkedNodes)
                    {
                        newWaterPump.occupyingNodes.Add(node);
                        node.hasTileData = true;
                    }
                    smokeParticleManager.SpawnParticlesOnBuilding(newWaterPump);
                }
            }
        }

        public int FindNearbyBuildableNodes(object? sender, Point mousePos, Node node, int checkWidth, int checkHeight)
        {
            Point currentPoint = mousePos;

            if (node.coords.X + 8 <= currentPoint.X + (8 * checkWidth) && node.coords.X + 8 >= currentPoint.X - (8 * checkWidth) &&
                node.coords.Y + 8 <= currentPoint.Y + (8 * checkHeight) && node.coords.Y + 8 >= currentPoint.Y - (8 * checkHeight))
            {
                if (node.isGrass && node.hasTileData == false && node.isNearRoad && !node.isRoad)
                {
                    return 0; //can place building here
                }
                else { return 1; } //cannot place building here
            }
            else { return 2; } //outside of check zone
        }


        //for water pumps modified to require at least one neighbor to be water
        public int FindNearbyBuildableNodes(object? sender, Point mousePos, Node node, int checkWidth, int checkHeight, bool isWaterPump)
        {
            Point currentPoint = mousePos;

            if (node.coords.X + 8 <= currentPoint.X + (8 * checkWidth) && node.coords.X + 8 >= currentPoint.X - (8 * checkWidth) &&
                node.coords.Y + 8 <= currentPoint.Y + (8 * checkHeight) && node.coords.Y + 8 >= currentPoint.Y - (8 * checkHeight))
            {
                if (!(node.isGrass && node.hasTileData == false && node.isNearRoad && !node.isRoad)) { return 1; }

                if (!isWaterPump) { return 0; }

                int tileW = rectSize;

                for (int dx = 0; dx <= 1; dx++)
                {
                    for (int dy = 0; dy <= 1; dy++)
                    {
                        Point topLeft = new Point(node.coords.X - dx * tileW, node.coords.Y - dy * tileW);

                        Point[] blockPoints = new Point[]
                        {
                            new Point(topLeft.X, topLeft.Y),
                            new Point(topLeft.X + tileW, topLeft.Y),
                            new Point(topLeft.X, topLeft.Y + tileW),
                            new Point(topLeft.X + tileW, topLeft.Y + tileW)
                        };

                        bool allBuildable = true;
                        List<Node> blockNodes = new List<Node>();

                        foreach (Point p in blockPoints)
                        {
                            Node neighbourNode = grid.nodes.FirstOrDefault(n => n.coords == p);
                            if (neighbourNode == null || !neighbourNode.isGrass || neighbourNode.hasTileData == true || !neighbourNode.isNearRoad)
                            {
                                allBuildable = false;
                                break;
                            }
                            blockNodes.Add(neighbourNode);
                        }

                        if (!allBuildable) { continue; }

                        Point[] adjacentChecks = new Point[]
                        {
                            new Point(topLeft.X - tileW, topLeft.Y),
                            new Point(topLeft.X - tileW, topLeft.Y + tileW),
                            new Point(topLeft.X + 2 * tileW, topLeft.Y),
                            new Point(topLeft.X + 2 * tileW, topLeft.Y + tileW),
                            new Point(topLeft.X, topLeft.Y - tileW),
                            new Point(topLeft.X + tileW, topLeft.Y - tileW),
                            new Point(topLeft.X, topLeft.Y + 2 * tileW),
                            new Point(topLeft.X + tileW, topLeft.Y + 2 * tileW)
                        };

                        bool hasWaterAdjacent = false;
                        foreach (Point adj in adjacentChecks)
                        {
                            Node adjNode = grid.nodes.FirstOrDefault(n => n.coords == adj);
                            if (adjNode != null && adjNode.isGrass == false)
                            {
                                hasWaterAdjacent = true;
                                break;
                            }
                        }

                        if (hasWaterAdjacent)
                        {
                            return 0;
                        }
                    }
                }
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }
}