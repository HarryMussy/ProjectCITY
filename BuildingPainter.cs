using Microsoft.VisualBasic.Devices;
using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.IO;
using System.Drawing.Imaging;

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
        private Dictionary<House, int> tileHouseImageIndex = new();
        private Random random = new Random();
        public AudioManager audioManager;
        public SmokeParticleManager smokeParticleManager;
        public Graphics g;


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
            foreach (House house in grid.buildings)
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
            Brush houseBrush = new SolidBrush(Color.Black);
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
                if (grid.cash >= 10000)
                {
                    foreach (Node node in grid.nodes)
                    {
                        int isTrue = FindNearbyBuildableNodes(sender, mousePos, node);
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
                        int isTrue = FindNearbyBuildableNodes(sender, mousePos, node);
                        if (isTrue == 0 || isTrue == 1)
                        {
                            g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, 20, 20);
                        }
                    }
                    g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29);
                }
            }

            // Use the form's zoomLevel if available
            float zoom = Form1 != null ? Form1.rectsize / 200f : 1.0f;

            foreach (House house in grid.buildings)
            {
                int imgIdx;
                if (!tileHouseImageIndex.TryGetValue(house, out imgIdx))
                {
                    imgIdx = random.Next(houseImages.Count);
                    tileHouseImageIndex[house] = imgIdx;
                }
                // Draw house image (assumes PNGs have transparency)
                g.DrawImage(houseImages[imgIdx], house.coords.X, house.coords.Y, 60 * zoom, 60 * zoom);
                Console.WriteLine($"Drawing house at {house.coords} with image index {imgIdx}");
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
            if (grid.cash >= 10000)
            {
                foreach (Node node in grid.nodes)
                {
                    int isTrue = FindNearbyBuildableNodes(sender, clickedPoint, node);
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
                    House newhouse = new House(1, new Size(30, 30), placement, "house");
                    audioManager.PlayPlaceSound();
                    grid.buildings.Add(newhouse);
                    grid.cash -= newhouse.cost;

                    // Assign a random image index for the new house
                    int imgIdx = random.Next(houseImages.Count);
                    tileHouseImageIndex[newhouse] = imgIdx;

                    foreach (Node node in grid.nodes)
                    {
                        if (node.coords.X + 10 <= clickedPoint.X + 30 && node.coords.X + 10 >= clickedPoint.X - 30 && node.coords.Y + 10 <= clickedPoint.Y + 30 && node.coords.Y + 10 >= clickedPoint.Y - 30)
                        {
                            newhouse.occupyingNodes.Add(node);
                            node.tiledata = newhouse;
                        }
                    }
                    smokeParticleManager.SpawnSmokeOnNewEdgesAndBuildings(new List<Edge>(), new List<House> { newhouse });
                }
            }
        }

        public int FindNearbyBuildableNodes(object? sender, Point mousePos, Node node)
        {
            Point worldMousePos = mousePos;
            var currentPoint = new Point((int)((worldMousePos.X - screencentre.X) / zoomLevel + screencentre.X), (int)((worldMousePos.Y - screencentre.Y) / zoomLevel + screencentre.Y));

            if (node.coords.X + 10 <= currentPoint.X + 30 && node.coords.X + 10 >= currentPoint.X - 30 &&
                node.coords.Y + 10 <= currentPoint.Y + 30 && node.coords.Y + 10 >= currentPoint.Y - 30)
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