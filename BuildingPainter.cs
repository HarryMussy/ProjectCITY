using Microsoft.VisualBasic.Devices;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;

using System.Xml.Linq;

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


        public BuildingPainter(Grid gridPassIn, Form1 Form1PassIn, Graphics g)
        {
            this.g = g;
            grid = gridPassIn;
            Form1 = Form1PassIn;
            LoadHouseImages();
            audioManager = Form1.audioManager;
            smokeParticleManager = Form1.smokeParticleManager;
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

            grid.FindRoadNodeIntersections();
            foreach (Node node in grid.buildableNodes)
            {
                if (viewBuildingSpaces == true)
                {
                    g.FillRectangle(redBrushBuilding, node.coords.X, node.coords.Y, 20, 20);
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
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, 3, 3);
                            if (isTrue == 0)
                            {
                                g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, 20, 20);
                                g.DrawString("-£10000", font, moneyCostBrush, mousePos.X - 30, mousePos.Y - 10);
                            }
                            else if (isTrue == 1)
                            {
                                g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, 20, 20);
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
                                g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, 20, 20);
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
                                g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, 20, 20);
                                g.DrawString("-£50000", font, moneyCostBrush, mousePos.X - 30, mousePos.Y - 10);
                            }
                            else if (isTrue == 1)
                            {
                                g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, 20, 20);
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
                                g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, 20, 20);
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
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, 2, 2);
                            if (isTrue == 0)
                            {
                                g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, 20, 20);
                                g.DrawString("-£30000", font, moneyCostBrush, mousePos.X - 30, mousePos.Y - 10);
                            }
                            else if (isTrue == 1)
                            {
                                g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, 20, 20);
                            }
                        }
                    }
                    else
                    {
                        foreach (Node node in grid.nodes)
                        {
                            int isTrue = FindNearbyBuildableNodes(sender, mousePos, node, 2, 2);
                            if (isTrue == 0 || isTrue == 1)
                            {
                                g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, 20, 20);
                            }
                        }
                        g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29);
                    }
                }
            }

            // Use the form's zoomLevel if available
            float zoom = Form1 != null ? Form1.rectsize / 200f : 1.0f;

            foreach (Building building in grid.buildings)
            {
                if (building.type == "house")
                {
                    int imgIdx;
                    if (!tileHouseImageIndex.TryGetValue(building, out imgIdx))
                    {
                        imgIdx = random.Next(houseImages.Count);
                        tileHouseImageIndex[building] = imgIdx;
                    }
                    // Draw house image (assumes PNGs have transparency)
                    g.DrawImage(houseImages[imgIdx], building.coords.X, building.coords.Y, 60 * zoom, 60 * zoom);
                }
                else if (building.type == "windfarm")
                {
                    // Draw a simple rectangle for power plants
                    g.FillRectangle(houseBrush, building.coords.X, building.coords.Y, 80 * zoom, 60 * zoom);
                    g.DrawString("Wind Farm", font, moneyCostBrush, building.coords.X, building.coords.Y + 20);
                }
                else if (building.type == "waterpump")
                {
                    // Draw a simple rectangle for power plants
                    g.FillRectangle(houseBrush, building.coords.X, building.coords.Y, 40 * zoom, 40 * zoom);
                    g.DrawString("Water\nPump", font, moneyCostBrush, building.coords.X, building.coords.Y + 5);
                }


                if (building.necessities[0].fulFilled == false)
                {
                    g.FillRectangle(yellowBrush, building.coords.X, building.coords.Y, 20 * zoom, 20 * zoom);
                }

                if (building.necessities[1].fulFilled == false)
                {
                    g.FillRectangle(blueBrush, building.coords.X + 20 * zoom, building.coords.Y, 20 * zoom, 20 * zoom);
                }
            }
        }

        private void LoadHouseImages()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string houseFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Houses");
            houseImages = new List<Image>();

            foreach (string path in Directory.GetFiles(houseFolder, "*.png"))
            {
                // Just load the image, PNG transparency will be preserved
                houseImages.Add(Image.FromFile(path));
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
                    int isTrue = FindNearbyBuildableNodes(sender, clickedPoint, node, 3, 3);
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
                    House newHouse = new House(new Size(30, 30), placement, "house");
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newHouse);
                    grid.cash -= newHouse.cost;

                    // Assign a random image index for the new house
                    int imgIdx = random.Next(houseImages.Count);
                    tileHouseImageIndex[newHouse] = imgIdx;

                    foreach (Node node in grid.nodes)
                    {
                        if (node.coords.X + 10 <= clickedPoint.X + 30 && node.coords.X + 10 >= clickedPoint.X - 30 && node.coords.Y + 10 <= clickedPoint.Y + 30 && node.coords.Y + 10 >= clickedPoint.Y - 30)
                        {
                            newHouse.occupyingNodes.Add(node);
                            node.tiledata = newHouse;
                        }
                    }
                    smokeParticleManager.SpawnSmokeOnNewEdgesAndBuildings(new List<Edge>(), new List<Building> { newHouse });
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
                    Windfarm newWindFarm = new Windfarm(new Size(40, 30), placement, "windfarm");
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newWindFarm);
                    grid.cash -= newWindFarm.cost;


                    foreach (Node node in grid.nodes)
                    {
                        if (node.coords.X + 10 <= clickedPoint.X + 40 && node.coords.X + 10 >= clickedPoint.X - 40 && node.coords.Y + 10 <= clickedPoint.Y + 30 && node.coords.Y + 10 >= clickedPoint.Y - 30)
                        {
                            newWindFarm.occupyingNodes.Add(node);
                            node.tiledata = newWindFarm;
                        }
                    }
                    smokeParticleManager.SpawnSmokeOnNewEdgesAndBuildings(new List<Edge>(), new List<Building> { newWindFarm });
                }
            }

            if (grid.cash >= 30000 && buildingType == "waterpump")
            {
                foreach (Node node in grid.nodes)
                {
                    int isTrue = FindNearbyBuildableNodes(sender, clickedPoint, node, 2, 2);
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
                    WaterPump newWaterPump = new WaterPump(new Size(20, 20), placement, "waterpump");
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newWaterPump);
                    grid.cash -= newWaterPump.cost;


                    foreach (Node node in grid.nodes)
                    {
                        if (node.coords.X + 10 <= clickedPoint.X + 20 && node.coords.X + 10 >= clickedPoint.X - 20 && node.coords.Y + 10 <= clickedPoint.Y + 20 && node.coords.Y + 10 >= clickedPoint.Y - 20)
                        {
                            newWaterPump.occupyingNodes.Add(node);
                            node.tiledata = newWaterPump;
                        }
                    }
                    smokeParticleManager.SpawnSmokeOnNewEdgesAndBuildings(new List<Edge>(), new List<Building> { newWaterPump });
                }
            }
        }

        public int FindNearbyBuildableNodes(object? sender, Point mousePos, Node node, int checkWidth, int checkHeight)
        {
            Point worldMousePos = mousePos;
            var currentPoint = new Point((int)((worldMousePos.X - screencentre.X) / zoomLevel + screencentre.X), (int)((worldMousePos.Y - screencentre.Y) / zoomLevel + screencentre.Y));

            if (node.coords.X + 10 <= currentPoint.X + (10 * checkWidth) && node.coords.X + 10 >= currentPoint.X - (10 * checkWidth) &&
                node.coords.Y + 10 <= currentPoint.Y + (10 * checkHeight) && node.coords.Y + 10 >= currentPoint.Y - (10 * checkHeight))
            {
                if (node.isGrass && node.tiledata == null && node.isNearRoad)
                {
                    return 0;
                }
                else { return 1; }
            }
            else { return 2; }
        }
    }
}