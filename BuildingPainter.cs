using System.IO;

namespace CitySkylines0._5alphabeta
{
    public class BuildingPainter
    {
        private readonly Grid grid;
        public Form1 Form1;
        public Point screencentre;
        private Dictionary<string, Image> imageCache = new();
        private List<string> houseImagesPath;
        private List<string> shopsImagesPath;
        private List<string> factoriesImagesPath;
        private string powerPlantImagePath;
        private string waterPumpImagePath;
        private string policeBuildingImagePath;
        private string fireServiceBuildingImagePath;
        private string fireImagePath;
        private Image fireImg;
        private Image crimeImg;
        private Image abandonedTileImage;
        private string hospitalImagePath;
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
        Size fireServiceSize;
        Size policeBuildingSize;
        Size hospitalSize;
        Size shopSize;
        Size factorySize;
        CarManager carManager;
        private readonly Brush invalidBrushBuilding = new SolidBrush(Color.FromArgb(200, Color.DarkRed));
        private readonly Brush validBrushBuilding = new SolidBrush(Color.FromArgb(200, Color.Green));
        private readonly Brush moneyCostBrush = new SolidBrush(Color.Black);
        private readonly Brush moneyCostBrushSpace = new SolidBrush(Color.DarkRed);
        private readonly Font font = new Font("Comic Sans", 11);
        private readonly Font font2 = new Font("Comic Sans", 9);


        public BuildingPainter(Grid gridPassIn, Form1 Form1PassIn, Graphics g, int rectSizeIn, Calendar calendar, CarManager carManagerIn)
        {
            rectSize = rectSizeIn;
            this.g = g;
            grid = gridPassIn;
            Form1 = Form1PassIn;
            LoadBuildingImages();
            audioManager = Form1.audioManager;
            smokeParticleManager = Form1.smokeParticleManager;
            this.calendar = calendar;
            carManager = carManagerIn;
            houseSize = new Size(2, 2);
            hospitalSize = new Size(4, 3);
            powerPlantSize = new Size(4, 3);
            waterPumpSize = new Size(2, 2);
            shopSize = new Size(3, 2);
            factorySize = new Size(3, 3);
            fireServiceSize = new Size(3, 2);
            policeBuildingSize = new Size(3, 2);
        }

        public Image GetImage(string path)
        {
            if (!imageCache.ContainsKey(path))
            {
                imageCache[path] = Image.FromFile(path);
            }
            return imageCache[path];
        }

        public void BuildingPaint(object? sender, Graphics g, Point mousePos)
        {
            if (Form1.selectingBuildingPainting == true)
            {
                if (buildingType == "house")
                {
                    var footprintNodes = GetFootprintNodes(mousePos, houseSize.Width, houseSize.Height);
                    bool canPlace = CanPlaceBuilding(grid, mousePos, houseSize.Width, houseSize.Height);

                    foreach (Node node in footprintNodes)
                    {
                        if (grid.cash < 10000) { g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else if (canPlace) { g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else { g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                    }

                    if (grid.cash >= 10000 && canPlace) { g.DrawString("-£10000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10); }
                    else if (grid.cash < 10000) { g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29); }
                }

                else if (buildingType == "powerplant")
                {
                    var footprintNodes = GetFootprintNodes(mousePos, powerPlantSize.Width, powerPlantSize.Height);
                    bool canPlace = CanPlaceBuilding(grid, mousePos, powerPlantSize.Width, powerPlantSize.Height);

                    foreach (Node node in footprintNodes)
                    {
                        if (grid.cash < 50000) { g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else if (canPlace) { g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else { g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                    }

                    if (grid.cash >= 50000 && canPlace) { g.DrawString("-£50000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10); }
                    else if (grid.cash < 50000) { g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29); }
                }

                else if (buildingType == "factory")
                {
                    var footprintNodes = GetFootprintNodes(mousePos, factorySize.Width, factorySize.Height);
                    bool canPlace = CanPlaceBuilding(grid, mousePos, factorySize.Width, factorySize.Height);

                    foreach (Node node in footprintNodes)
                    {
                        if (grid.cash < 50000) { g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else if (canPlace) { g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else { g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                    }

                    if (grid.cash >= 50000 && canPlace) { g.DrawString("-£50000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10); }
                    else if (grid.cash < 50000) { g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29); }
                }

                else if (buildingType == "waterpump")
                {
                    var footprintNodes = GetFootprintNodes(mousePos, waterPumpSize.Width, waterPumpSize.Height);
                    bool canPlace = CanPlaceWaterPump(grid, mousePos, waterPumpSize.Width, waterPumpSize.Height);

                    foreach (Node node in footprintNodes)
                    {
                        if (grid.cash < 10000) { g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else if (canPlace) { g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else { g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                    }

                    if (grid.cash >= 30000 && canPlace) { g.DrawString("-£30000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10); }
                    else if (grid.cash < 30000) { g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29); }
                }

                else if (buildingType == "shop")
                {
                    var footprintNodes = GetFootprintNodes(mousePos, shopSize.Width, shopSize.Height);
                    bool canPlace = CanPlaceBuilding(grid, mousePos, shopSize.Width, shopSize.Height);

                    foreach (Node node in footprintNodes)
                    {
                        if (grid.cash < 30000) { g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else if (canPlace) { g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else { g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                    }

                    if (grid.cash >= 30000 && canPlace) { g.DrawString("-£30000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10); }
                    else if (grid.cash < 30000) { g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29); }
                }

                else if (buildingType == "hospital")
                {
                    var footprintNodes = GetFootprintNodes(mousePos, hospitalSize.Width, hospitalSize.Height);
                    bool canPlace = CanPlaceBuilding(grid, mousePos, hospitalSize.Width, hospitalSize.Height);

                    foreach (Node node in footprintNodes)
                    {
                        if (grid.cash < 100000) { g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else if (canPlace) { g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else { g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                    }

                    if (grid.cash >= 100000 && canPlace) { g.DrawString("-£100000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10); }
                    else if (grid.cash < 100000) { g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29); }
                }

                else if (buildingType == "policebuilding")
                {
                    var footprintNodes = GetFootprintNodes(mousePos, policeBuildingSize.Width, policeBuildingSize.Height);
                    bool canPlace = CanPlaceBuilding(grid, mousePos, policeBuildingSize.Width, policeBuildingSize.Height);

                    foreach (Node node in footprintNodes)
                    {
                        if (grid.cash < 100000) { g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else if (canPlace) { g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else { g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                    }

                    if (grid.cash >= 100000 && canPlace) { g.DrawString("-£100000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10); }
                    else if (grid.cash < 100000) { g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29); }
                }

                else if (buildingType == "fireservice")
                {
                    var footprintNodes = GetFootprintNodes(mousePos, fireServiceSize.Width, fireServiceSize.Height);
                    bool canPlace = CanPlaceBuilding(grid, mousePos, fireServiceSize.Width, fireServiceSize.Height);

                    foreach (Node node in footprintNodes)
                    {
                        if (grid.cash < 75000) { g.FillRectangle(moneyCostBrushSpace, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else if (canPlace) { g.FillRectangle(validBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                        else { g.FillRectangle(invalidBrushBuilding, node.coords.X, node.coords.Y, rectSize, rectSize); }
                    }

                    if (grid.cash >= 75000 && canPlace) { g.DrawString("-£75000", font, moneyCostBrush, mousePos.X - 10, mousePos.Y - 10); }
                    else if (grid.cash < 75000) { g.DrawString("NOT\nENOUGH\nMONEY", font2, moneyCostBrush, mousePos.X - 29, mousePos.Y - 29); }
                }
            }

            // Use the form's zoomLevel if available
            float zoom = Form1 != null ? Form1.rectSize / 200f : 1.0f;

            foreach (Building building in grid.buildings)
            {
                if (building.isAbandoned == false && (calendar.GetHour() >= 21 || calendar.GetHour() <= 5))
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
                        imgIdx = random.Next(houseImagesPath.Count);
                        tileHouseImageIndex[building] = imgIdx;
                    }
                    // Use building.size for drawing
                    g.DrawImage(GetImage(building.imagePath), building.coords.X, building.coords.Y, building.size.Width * rectSize, building.size.Height * rectSize);
                    /*                    g.DrawString(building.Occupants.Where(p => p != null).Count().ToString(), new Font("Segoe UI", 8, FontStyle.Bold), new SolidBrush(Color.White), building.coords.X, building.coords.Y + 5);*/

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
                else
                {
                    g.DrawImage(GetImage(building.imagePath), building.coords.X, building.coords.Y, building.size.Width * rectSize, building.size.Height * rectSize);
                }

                for (int i = 0; i < building.necessities.Count; i++)
                {
                    Necessity ne = building.necessities[i];
                    ne.DrawNecessity(sender, g, mousePos, new Point(building.coords.X + (i * 8), building.coords.Y));
                }

                if (building.isInCrime)
                {
                    Crime crime = new Crime();
                    crime.image = crimeImg;
                    crime.type = "Crime";
                    crime.DrawNecessity(sender, g, mousePos, new Point(building.coords.X + 16, building.coords.Y + 8));
                }

                if (building.isOnFire)
                {

                    for (int x = 0; x < building.size.Width; x++)
                    {
                        for (int y = 0; y < building.size.Height; y++)
                        {
                            if (random.Next(6) == 0) // flame density
                            {
                                int drawX = building.coords.X + (x * rectSize);
                                int drawY = building.coords.Y + (y * rectSize);

                                g.DrawImage(fireImg, drawX, drawY, rectSize, rectSize);
                            }
                        }
                    }
                }

                if (building.isAbandoned)
                {
                    if (building.Occupants != null)
                    {
                        for (int i = 0; i < building.Occupants.Length; i++)
                        {
                            if (building.Occupants[i] != null)
                            {
                                building.Occupants[i].KillPerson();
                            }
                        }
                    }

                    g.DrawImage(abandonedTileImage, building.coords.X, building.coords.Y, building.size.Width * 16, building.size.Height * 16);
                }
            }

            foreach (Hospital h in grid.buildings.Where(b => b.type == "hospital"))
            {
                h.UpdateHospital();
            }

            foreach (PoliceBuilding pb in grid.buildings.Where(b => b.type == "policebuilding"))
            {
                pb.UpdatePoliceBuilding();
            }

            foreach (FireService fs in grid.buildings.Where(b => b.type == "fireservice"))
            {
                fs.UpdateFireService();
            }
        }

        private void LoadBuildingImages()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string houseFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Houses");
            houseImagesPath = new List<string>();

            foreach (string path in Directory.GetFiles(houseFolder, "*.png"))
            {
                using var original = Image.FromFile(path);
                houseImagesPath.Add(path);
            }

            string shopFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Shops");
            shopsImagesPath = new List<string>();

            foreach (string path in Directory.GetFiles(shopFolder, "*.png"))
            {
                using var original = Image.FromFile(path);
                shopsImagesPath.Add(path);
            }

            string factoryFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Factories");
            factoriesImagesPath = new List<string>();

            foreach (string path in Directory.GetFiles(factoryFolder, "*.png"))
            {
                using var original = Image.FromFile(path);
                factoriesImagesPath.Add(path);
            }

            powerPlantImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "powerPlant.png");

            waterPumpImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "waterPump.png");

            hospitalImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "hospital.png");

            policeBuildingImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "police.png");

            fireServiceBuildingImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "fireservicebuilding.png");

            fireImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Icons", "fire.png");
            fireImg = GetImage(fireImagePath);

            string crimeImgPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Icons", "Crime.png");
            crimeImg = GetImage(crimeImgPath);

            string abandonedTileImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Abandoned.png");
            abandonedTileImage = GetImage(abandonedTileImagePath);
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
                if (!CanPlaceBuilding(grid, clickedPoint, houseSize.Width, houseSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, houseSize.Width, houseSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                House newHouse = new House(houseSize, placement, "house", 10, 10);
                newHouse.imagePath = houseImagesPath[random.Next(houseImagesPath.Count())];

                audioManager.PlayPlaceSound();
                grid.buildings.Add(newHouse);
                grid.cash -= newHouse.cost;

                foreach (Node node in footprintNodes)
                {
                    newHouse.occupyingNodes.Add(node);
                    node.hasTileData = true;
                }

                smokeParticleManager.SpawnParticlesOnBuilding(newHouse);
            }

            if (grid.cash >= 50000 && buildingType == "powerplant")
            {
                if (!CanPlaceBuilding(grid, clickedPoint, powerPlantSize.Width, powerPlantSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, powerPlantSize.Width, powerPlantSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                PowerPlant newPowerPlant = new PowerPlant(powerPlantSize, placement, "powerplant", 1000, 50);
                newPowerPlant.imagePath = powerPlantImagePath;

                audioManager.PlayPlaceSound();
                grid.buildings.Add(newPowerPlant);
                grid.cash -= newPowerPlant.cost;

                foreach (Node node in footprintNodes)
                {
                    newPowerPlant.occupyingNodes.Add(node);
                    node.hasTileData = true;
                }

                smokeParticleManager.SpawnParticlesOnBuilding(newPowerPlant);
            }

            if (grid.cash >= 50000 && buildingType == "factory")
            {
                if (!CanPlaceBuilding(grid, clickedPoint, factorySize.Width, factorySize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, factorySize.Width, factorySize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                Factory newFactory = new Factory(factorySize, placement, "factory", 100, 100);
                newFactory.imagePath = factoriesImagesPath[random.Next(factoriesImagesPath.Count())];

                audioManager.PlayPlaceSound();
                grid.buildings.Add(newFactory);
                grid.cash -= newFactory.cost;

                foreach (Node node in footprintNodes)
                {
                    newFactory.occupyingNodes.Add(node);
                    node.hasTileData = true;
                }

                smokeParticleManager.SpawnParticlesOnBuilding(newFactory);
            }

            if (grid.cash >= 30000 && buildingType == "waterpump")
            {
                if (!CanPlaceWaterPump(grid, clickedPoint, waterPumpSize.Width, waterPumpSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, waterPumpSize.Width, waterPumpSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                WaterPump newWaterPump = new WaterPump(waterPumpSize, placement, "waterpump", 50, 500);
                newWaterPump.imagePath = waterPumpImagePath;

                audioManager.PlayPlaceSound();
                grid.buildings.Add(newWaterPump);
                grid.cash -= newWaterPump.cost;

                foreach (Node node in footprintNodes)
                {
                    newWaterPump.occupyingNodes.Add(node);
                    node.hasTileData = true;
                }

                smokeParticleManager.SpawnParticlesOnBuilding(newWaterPump);
            }

            if (grid.cash >= 100000 && buildingType == "hospital")
            {
                if (!CanPlaceBuilding(grid, clickedPoint, hospitalSize.Width, hospitalSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, hospitalSize.Width, hospitalSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                Hospital newHospital = new Hospital(hospitalSize, placement, "hospital", 250, 250, grid, carManager);
                newHospital.imagePath = hospitalImagePath;

                audioManager.PlayPlaceSound();
                grid.buildings.Add(newHospital);
                grid.cash -= newHospital.cost;

                foreach (Node node in footprintNodes)
                {
                    newHospital.occupyingNodes.Add(node);
                    node.hasTileData = true;
                }

                smokeParticleManager.SpawnParticlesOnBuilding(newHospital);
            }

            if (grid.cash >= 30000 && buildingType == "shop")
            {
                if (!CanPlaceBuilding(grid, clickedPoint, shopSize.Width, shopSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, shopSize.Width, shopSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                Shop newShop = new Shop(shopSize, placement, "shop", 10, 10);
                newShop.imagePath = shopsImagesPath[random.Next(shopsImagesPath.Count())];

                audioManager.PlayPlaceSound();
                grid.buildings.Add(newShop);
                grid.cash -= newShop.cost;

                foreach (Node node in footprintNodes)
                {
                    newShop.occupyingNodes.Add(node);
                    node.hasTileData = true;
                }

                smokeParticleManager.SpawnParticlesOnBuilding(newShop);
            }

            if (grid.cash >= 100000 && buildingType == "policebuilding")
            {
                if (!CanPlaceBuilding(grid, clickedPoint, policeBuildingSize.Width, policeBuildingSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, policeBuildingSize.Width, policeBuildingSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                PoliceBuilding newPoliceBuilding = new PoliceBuilding(policeBuildingSize, placement, "policebuilding", 50, 25, grid, carManager);
                newPoliceBuilding.imagePath = policeBuildingImagePath;

                audioManager.PlayPlaceSound();
                grid.buildings.Add(newPoliceBuilding);
                grid.cash -= newPoliceBuilding.cost;

                foreach (Node node in footprintNodes)
                {
                    newPoliceBuilding.occupyingNodes.Add(node);
                    node.hasTileData = true;
                }

                smokeParticleManager.SpawnParticlesOnBuilding(newPoliceBuilding);
            }

            if (grid.cash >= 75000 && buildingType == "fireservice")
            {
                if (!CanPlaceBuilding(grid, clickedPoint, fireServiceSize.Width, fireServiceSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, fireServiceSize.Width, fireServiceSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                FireService newFireService = new FireService(fireServiceSize, placement, "fireservice", 15, 350, grid, carManager);
                newFireService.imagePath = fireServiceBuildingImagePath;

                audioManager.PlayPlaceSound();
                grid.buildings.Add(newFireService);
                grid.cash -= newFireService.cost;

                foreach (Node node in footprintNodes)
                {
                    newFireService.occupyingNodes.Add(node);
                    node.hasTileData = true;
                }

                smokeParticleManager.SpawnParticlesOnBuilding(newFireService);
            }
        }

        private bool IsDirectlyAdjacentToRoad(Node node)
        {
            int tile = rectSize;

            Point[] directions = { new Point(tile, 0), new Point(-tile, 0), new Point(0, tile), new Point(0, -tile) };

            foreach (Point dir in directions)
            {
                Point checkPoint = new Point(node.coords.X + dir.X, node.coords.Y + dir.Y);

                Node adjacent = grid.nodes.FirstOrDefault(n => n.coords == checkPoint);

                if (adjacent != null && adjacent.isRoad) { return true; }
            }

            return false;
        }

        public bool CanPlaceBuilding(Grid grid, Point mousePos, int width, int height)
        {
            List<Node> footprintNodes = new List<Node>();

            foreach (Node node in grid.nodes)
            {
                if (node.coords.X + 8 <= mousePos.X + (8 * width) &&
                    node.coords.X + 8 >= mousePos.X - (8 * width) &&
                    node.coords.Y + 8 <= mousePos.Y + (8 * height) &&
                    node.coords.Y + 8 >= mousePos.Y - (8 * height))
                {
                    if (!node.isGrass || node.hasTileData || node.isRoad)
                        return false;

                    footprintNodes.Add(node);
                }
            }

            if (footprintNodes.Count == 0)
                return false;

            bool hasRoadAccess = footprintNodes.Any(n => IsDirectlyAdjacentToRoad(n));

            return hasRoadAccess;
        }


        //for water pumps modified to require at least one neighbor to be water
        public bool CanPlaceWaterPump(Grid grid, Point mousePos, int width, int height)
        {
            List<Node> footprintNodes = new List<Node>();

            // 1️⃣ Gather footprint nodes
            foreach (Node node in grid.nodes)
            {
                if (node.coords.X + 8 <= mousePos.X + (8 * width) &&
                    node.coords.X + 8 >= mousePos.X - (8 * width) &&
                    node.coords.Y + 8 <= mousePos.Y + (8 * height) &&
                    node.coords.Y + 8 >= mousePos.Y - (8 * height))
                {
                    // Tile must be buildable grass
                    if (!node.isGrass || node.hasTileData || node.isRoad)
                        return false;

                    footprintNodes.Add(node);
                }
            }

            if (footprintNodes.Count == 0)
                return false;

            // 2️⃣ Check road adjacency
            bool hasRoadAccess = footprintNodes.Any(n => IsDirectlyAdjacentToRoad(n));
            if (!hasRoadAccess)
                return false;

            // 3️⃣ Check water adjacency
            bool hasWaterAccess = footprintNodes.Any(n =>
            {
                Point[] checks =
                {
            new Point(n.coords.X - rectSize, n.coords.Y),
            new Point(n.coords.X + rectSize, n.coords.Y),
            new Point(n.coords.X, n.coords.Y - rectSize),
            new Point(n.coords.X, n.coords.Y + rectSize)
        };

                foreach (Point p in checks)
                {
                    Node adj = grid.nodes.FirstOrDefault(x => x.coords == p);
                    if (adj != null && !adj.isGrass)
                        return true;
                }

                return false;
            });

            if (!hasWaterAccess)
                return false;

            return true;
        }

        private List<Node> GetFootprintNodes(Point mousePos, int width, int height)
        {
            List<Node> footprintNodes = new List<Node>();

            foreach (Node node in grid.nodes)
            {
                if (node.coords.X + 8 <= mousePos.X + (8 * width) &&
                    node.coords.X + 8 >= mousePos.X - (8 * width) &&
                    node.coords.Y + 8 <= mousePos.Y + (8 * height) &&
                    node.coords.Y + 8 >= mousePos.Y - (8 * height))
                {
                    footprintNodes.Add(node);
                }
            }

            return footprintNodes;
        }
    }
}