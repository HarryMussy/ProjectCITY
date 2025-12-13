using System.IO;

namespace CitySkylines0._5alphabeta
{
    public class BuildingPainter
    {
        private readonly Grid grid;
        public Form1 Form1;
        public Point screencentre;
        private float zoomLevel = 1.0f;
        public bool viewBuildingSpaces = true;
        private List<Image> houseImages;
        private Dictionary<Building, int> tileHouseImageIndex = new();
        private Random random = new Random();
        public AudioManager audioManager;
        public SmokeParticleManager smokeParticleManager;
        public Graphics g;
        public string buildingType = "";
        private int rectSize;
        public Calendar calendar;


        public BuildingPainter(Grid gridPassIn, Form1 Form1PassIn, Graphics g, int rectSizeIn, Calendar calendar)
        {
            rectSize = rectSizeIn;
            this.g = g;
            grid = gridPassIn;
            Form1 = Form1PassIn;
            LoadHouseImages();
            audioManager = Form1.audioManager;
            smokeParticleManager = Form1.smokeParticleManager;
            this.calendar = calendar;
        }

        public void GenerateDetails()
        {
            foreach (Building house in grid.buildings)
            {
                //gives each house a random house image
                tileHouseImageIndex[house] = random.Next(houseImages.Count);
            }
        }

        public void BuildingPaint(object? sender, Graphics g, Point mousePos)
        {
            Brush redBrushBuilding = new SolidBrush(Color.FromArgb(100, Color.Red));
            Brush invalidBrushBuilding = new SolidBrush(Color.FromArgb(200, Color.DarkRed));
            Brush validBrushBuilding = new SolidBrush(Color.FromArgb(200, Color.Green));
            Brush houseBrush = new SolidBrush(Color.Gray);
            Brush blueBrush = new SolidBrush(Color.Blue); //for water supply/ need for water
            Brush yellowBrush = new SolidBrush(Color.Yellow); //for electricity demand/ need for electricity
            Brush moneyCostBrush = new SolidBrush(Color.Black);
            Brush moneyCostBrushSpace = new SolidBrush(Color.DarkRed);
            Font font = new Font("Comic Sans", 11);
            Font font2 = new Font("Comic Sans", 9);


            foreach (Node node in grid.buildableNodes)
            {
                if (viewBuildingSpaces == true)
                {
                    g.FillRectangle(redBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize);
                }
            }

            if (Form1.selectingBuildingPainting == true)
            {
                if (buildingType == "house")
                {
                    if (grid.cash >= 10000)
                    {
                        foreach (Node node in grid.nodes)
                        {
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, 1, 1);
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
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, 3, 3);
                            if (isTrue == 0 || isTrue == 1)
                            {
                                g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize);
                            }
                        }
                        g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29);
                    }
                }

                else if (buildingType == "windfarm")
                {
                    if (grid.cash >= 50000)
                    {
                        foreach (Node node in grid.nodes)
                        {
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, 4, 3);
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
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, 4, 3);
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
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, 2, 2, true);
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
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, 2, 2, true);
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
                    g.FillEllipse(glow3, building.coords.X - 10, building.coords.Y - 10, building.size.Width + 20, building.size.Height + 20);
                    g.FillEllipse(glow2, building.coords.X - 5, building.coords.Y - 5, building.size.Width + 10, building.size.Height + 10);
                    g.FillEllipse(glow1, building.coords.X, building.coords.Y, building.size.Width, building.size.Height);
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
                    g.DrawImage(houseImages[imgIdx], building.coords.X, building.coords.Y, building.size.Width, building.size.Height);
                }
                else if (building.type == "windfarm")
                {
                    string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
                    string windFarmFilePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "windTurbine.gif");
                    g.DrawImage(Image.FromFile(windFarmFilePath), building.coords.X, building.coords.Y, building.size.Width, building.size.Height);
                    ImageAnimator.Animate(Image.FromFile(windFarmFilePath), null);
                }
                else if (building.type == "waterpump")
                {
                    g.FillRectangle(houseBrush, building.coords.X, building.coords.Y, building.size.Width, building.size.Height);
                    g.DrawString("Water\nPump", font, moneyCostBrush, building.coords.X, building.coords.Y + 5);
                }


                if (building.necessities[0].fulFilled == false)
                {
                    g.FillRectangle(yellowBrush, building.coords.X, building.coords.Y, 3, 3);
                }

                if (building.necessities[1].fulFilled == false)
                {
                    g.FillRectangle(blueBrush, building.coords.X, building.coords.Y, 3, 3);
                }

                //debug code to find where the building is drawn from
                /*g.FillEllipse(yellowBrush, building.coords.X, building.coords.Y, 3, 3);*/
            }
        }

        private void LoadHouseImages()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string houseFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Houses", "B");
            houseImages = new List<Image>();

            foreach (string path in Directory.GetFiles(houseFolder, "*.png"))
            {
                // Just load the image, PNG transparency will be preserved
                Image i = Image.FromFile(path);
                houseImages.Add(i);
            }
        }

        public void LeftMouseDown(object? sender, MouseEventArgs m)
        {
            List<int> checkedSpaces = new List<int>();
            List<Node> checkedNodes = new List<Node>();
            Point worldMousePos = ((Form1)sender).Mouse_Pos(sender, m);
            var clickedPoint = new Point((int)((worldMousePos.X - screencentre.X) / zoomLevel + screencentre.X), (int)((worldMousePos.Y - screencentre.Y) / zoomLevel + screencentre.Y));
            if (grid.cash >= 10000 && buildingType == "house")
            {
                foreach (Node node in grid.nodes)
                {
                    int isTrue = FindNearbyBuildableNodes(sender, clickedPoint, node, 1, 1);
                    if (isTrue == 1 || isTrue == 0) { checkedSpaces.Add(isTrue); checkedNodes.Add(node); }
                }
                if (checkedSpaces.Contains(1)) { }
                else
                {
                    Point placement = new Point(int.MaxValue, int.MaxValue);
                    foreach (Node n in checkedNodes)
                    {
                        if (n.coords.X < placement.X && n.coords.Y < placement.Y)
                        {
                            placement = n.coords;
                        }
                    }
                    House newHouse = new House(new Size(rectSize * 1, rectSize * 1), placement, "house", 10, 5);
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newHouse);
                    grid.cash -= newHouse.cost;

                    // Assign a random image index for the new house
                    int imgIdx = random.Next(houseImages.Count);
                    tileHouseImageIndex[newHouse] = imgIdx;

                    foreach (Node node in grid.nodes)
                    {
                        if (FindNearbyBuildableNodes(sender, new Point(placement.X + 8, placement.Y + 8), node, 1, 1) == 0)
                        {
                            newHouse.occupyingNodes.Add(node);
                            node.tileData = newHouse;
                        }
                    }
                    /*smokeParticleManager.SpawnSmokeOnNewEdgesAndBuildings(new List<Edge>(), new List<Building> { newHouse });*/
                }
            }

            if (grid.cash >= 50000 && buildingType == "windfarm")
            {
                foreach (Node node in grid.nodes)
                {
                    int isTrue = FindNearbyBuildableNodes(sender, clickedPoint, node, 4, 3);
                    if (isTrue == 1 || isTrue == 0) { checkedSpaces.Add(isTrue); checkedNodes.Add(node); }
                }
                if (checkedSpaces.Contains(1)) { }
                else
                {
                    Point placement = new Point(int.MaxValue, int.MaxValue);
                    foreach (Node n in checkedNodes)
                    {
                        if (n.coords.X < placement.X && n.coords.Y < placement.Y)
                        {
                            placement = n.coords;
                        }
                    }
                    Windfarm newWindFarm = new Windfarm(new Size(rectSize * 4, rectSize * 3), placement, "windfarm", -50, 0);
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newWindFarm);
                    grid.cash -= newWindFarm.cost;


                    foreach (Node node in grid.nodes)
                    {
                        if (FindNearbyBuildableNodes(sender, new Point(placement.X + 32, placement.Y + 24), node, 4, 3) == 0)
                        {
                            newWindFarm.occupyingNodes.Add(node);
                            node.tileData = newWindFarm;
                        }
                    }
                    /*smokeParticleManager.SpawnSmokeOnNewEdgesAndBuildings(new List<Edge>(), new List<Building> { newWindFarm });*/
                }
            }

            if (grid.cash >= 30000 && buildingType == "waterpump")
            {
                foreach (Node node in grid.nodes)
                {
                    int isTrue = FindNearbyBuildableNodes(sender, clickedPoint, node, 2, 2, true);
                    if (isTrue == 1 || isTrue == 0) { checkedSpaces.Add(isTrue); checkedNodes.Add(node); }
                }
                if (checkedSpaces.Contains(1)) { }
                else
                {
                    Point placement = new Point(int.MaxValue, int.MaxValue);
                    foreach (Node n in checkedNodes)
                    {
                        if (n.coords.X < placement.X && n.coords.Y < placement.Y)
                        {
                            placement = n.coords;
                        }
                    }
                    WaterPump newWaterPump = new WaterPump(new Size(rectSize * 2, rectSize * 2), placement, "waterpump", 15, -50);
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newWaterPump);
                    grid.cash -= newWaterPump.cost;


                    foreach (Node node in grid.nodes)
                    {
                        if (FindNearbyBuildableNodes(sender, new Point(placement.X + 16, placement.Y + 16), node, 2, 2) == 0)
                        {
                            newWaterPump.occupyingNodes.Add(node);
                            node.tileData = newWaterPump;
                        }
                    }
                    /* smokeParticleManager.SpawnSmokeOnNewEdgesAndBuildings(new List<Edge>(), new List<Building> { newWaterPump });*/
                }
            }
        }

        public int FindNearbyBuildableNodes(object? sender, Point mousePos, Node node, int checkWidth, int checkHeight)
        {
            Point worldMousePos = mousePos;
            var currentPoint = new Point((int)((worldMousePos.X - screencentre.X) / zoomLevel + screencentre.X), (int)((worldMousePos.Y - screencentre.Y) / zoomLevel + screencentre.Y));

            //finds the nodes that the building will be placed on
            if (node.coords.X + 8 <= currentPoint.X + (8 * checkWidth) && node.coords.X + 8 >= currentPoint.X - (8 * checkWidth) &&
                node.coords.Y + 8 <= currentPoint.Y + (8 * checkHeight) && node.coords.Y + 8 >= currentPoint.Y - (8 * checkHeight))
            {
                if (node.isGrass && node.tileData == null && node.isNearRoad) //checks if you are able to build on those tiles
                {
                    return 0; //can place building here
                }
                else { return 1; } //cannot place building here
            }
            else { return 2; } //outside of check zone
        }


        //for water pumps - modified to require at least one cardinal neighbor to be water
        public int FindNearbyBuildableNodes(object? sender, Point mousePos, Node node, int checkWidth, int checkHeight, bool isWaterPump)
        {
            Point worldMousePos = mousePos;
            var currentPoint = new Point((int)((worldMousePos.X - screencentre.X) / zoomLevel + screencentre.X),
                                         (int)((worldMousePos.Y - screencentre.Y) / zoomLevel + screencentre.Y));

            //finds the nodes that are inside the build check zone
            if (node.coords.X + 8 <= currentPoint.X + (8 * checkWidth) && node.coords.X + 8 >= currentPoint.X - (8 * checkWidth) &&
                node.coords.Y + 8 <= currentPoint.Y + (8 * checkHeight) && node.coords.Y + 8 >= currentPoint.Y - (8 * checkHeight))
            {
                //basic buildability checks
                if (!(node.isGrass && node.tileData == null && node.isNearRoad)) { return 1; }

                if (!isWaterPump) { return 0; }

                int tileW = rectSize;   //tile size in pixels
                                        //try all 4 possible top-left origins of a 2x2 block 
                for (int dx = 0; dx <= 1; dx++)
                {
                    for (int dy = 0; dy <= 1; dy++)
                    {
                        Point topLeft = new Point(node.coords.X - dx * tileW, node.coords.Y - dy * tileW);

                        //gather the four nodes that form the 2x2 block
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
                            if (neighbourNode == null || !neighbourNode.isGrass || neighbourNode.tileData != null || !neighbourNode.isNearRoad)
                            {
                                allBuildable = false;
                                break;
                            }
                            blockNodes.Add(neighbourNode);
                        }

                        if (!allBuildable) { continue; } // try next possible origin

                        //check cardinal neighbours
                        Point[] adjacentChecks = new Point[]
                        {
                            // left side (west)
                            new Point(topLeft.X - tileW, topLeft.Y),
                            new Point(topLeft.X - tileW, topLeft.Y + tileW),
    
                            // right side (east)
                            new Point(topLeft.X + 2 * tileW, topLeft.Y),
                            new Point(topLeft.X + 2 * tileW, topLeft.Y + tileW),

                            // top side (north)
                            new Point(topLeft.X, topLeft.Y - tileW),
                            new Point(topLeft.X + tileW, topLeft.Y - tileW),

                           // bottom side (south)
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