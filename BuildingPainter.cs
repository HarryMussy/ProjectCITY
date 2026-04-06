using System.IO;
using System.Windows.Forms;

namespace ProjectCity
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

        //returns an image based off of the image path
        public Image GetImage(string path)
        {
            if (!imageCache.ContainsKey(path))
            {
                imageCache[path] = Image.FromFile(path);
            }
            return imageCache[path];
        }

        //draws the building information onto the map
        public void BuildingPaint(object? sender, Graphics g, Point mousePos)
        {
            //draws an outline of where the building will be based off the type of building
            if (Form1.selectingBuildingPainting == true)
            {
                if (buildingType == "house")
                {
                    var footprintNodes = GetFootprintNodes(mousePos, houseSize.Width, houseSize.Height);
                    bool canPlace = CanPlaceBuilding(grid, mousePos, houseSize.Width, houseSize.Height);

                    //3 states: have enough money and is in a valid placement
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

                    //3 states: have enough money and is in a valid placement
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

                    //3 states: have enough money and is in a valid placement
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

                    //3 states: have enough money and is in a valid placement- difference for water pumps as they need to be placed by water as well
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

                    //3 states: have enough money and is in a valid placement
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

                    //3 states: have enough money and is in a valid placement
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

                    //3 states: have enough money and is in a valid placement
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

                    //3 states: have enough money and is in a valid placement
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

            //use the form's zoomLevel if available
            float zoom = Form1 != null ? Form1.rectSize / 200f : 1.0f;

            //loop through every building
            foreach (Building building in grid.buildings)
            {
                //apply a glow to buildings at night
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

                //for every house that isn't abandoned
                if (building.type == "house" && !building.isAbandoned)
                {
                    int imgIdx;
                    if (!tileHouseImageIndex.TryGetValue(building, out imgIdx))
                    {
                        imgIdx = random.Next(houseImagesPath.Count);
                        tileHouseImageIndex[building] = imgIdx;
                    }

                    //use building.size for drawing
                    g.DrawImage(GetImage(building.imagePath), building.coords.X, building.coords.Y, building.size.Width * rectSize, building.size.Height * rectSize);

                    foreach (Person p in building.Occupants.Where(p => p != null))
                    {
                        //draw an unhealthy icon in the case of the people in the house being ill
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

                //for any other building
                else
                {
                    if (!building.isAbandoned)
                    {
                        g.DrawImage(GetImage(building.imagePath), building.coords.X, building.coords.Y, building.size.Width * rectSize, building.size.Height * rectSize);
                    }
                }

                //draw the necessities for the building (power water etc.) if not fulfilled
                for (int i = 0; i < building.necessities.Count; i++)
                {
                    Necessity ne = building.necessities[i];
                    ne.DrawNecessity(sender, g, mousePos, new Point(building.coords.X + (i * 8), building.coords.Y));
                }

                //if the building is experiencing crime, draw the crime icon as well
                if (building.isInCrime)
                {
                    Crime crime = new Crime();
                    crime.image = crimeImg;
                    crime.type = "Crime";
                    crime.DrawNecessity(sender, g, mousePos, new Point(building.coords.X + 16, building.coords.Y + 8));
                }

                //if the building is on fire draw a fire over the building
                if (building.isOnFire)
                {
                    foreach (int nodeNumber in building.occupyingNodesIndex)
                    {
                        Node fireNode = grid.nodes.FirstOrDefault(n => n.nodeNumber == nodeNumber);
                        if (fireNode != null)
                        {
                            g.DrawImage(fireImg, fireNode.coords.X, fireNode.coords.Y, rectSize, rectSize);
                        }
                    }
                }

                //if the building is abandoned kill every person in the building and draw the abandoned building image
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


            //update each emergency service
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

        //load the images for each building and each effect
        private void LoadBuildingImages()
        {
            //find the project folder
            string projectRoot = AppContext.BaseDirectory;

            //find the folder for all of the houses
            string houseFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Houses");
            houseImagesPath = new List<string>();

            foreach (string path in Directory.GetFiles(houseFolder, "*.png"))
            {
                //load all of the house images
                using var original = Image.FromFile(path);
                houseImagesPath.Add(path);
            }

            //find the folder for shops
            string shopFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Shops");
            shopsImagesPath = new List<string>();

            foreach (string path in Directory.GetFiles(shopFolder, "*.png"))
            {
                //load all of the shop images
                using var original = Image.FromFile(path);
                shopsImagesPath.Add(path);
            }

            //find the folder for factories
            string factoryFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Factories");
            factoriesImagesPath = new List<string>();

            foreach (string path in Directory.GetFiles(factoryFolder, "*.png"))
            {
                //load every factory image
                using var original = Image.FromFile(path);
                factoriesImagesPath.Add(path);
            }

            //load all of the other building images
            powerPlantImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "powerPlant.png");
            waterPumpImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "waterPump.png");
            hospitalImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "hospital.png");
            policeBuildingImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "police.png");
            fireServiceBuildingImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "fireservicebuilding.png");

            //find the images for the fires
            fireImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Icons", "fire.png");
            fireImg = GetImage(fireImagePath);

            //find the images for crimes
            string crimeImgPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Icons", "Crime.png");
            crimeImg = GetImage(crimeImgPath);

            //find the images for abandoned buildings
            string abandonedTileImagePath = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Abandoned.png");
            abandonedTileImage = GetImage(abandonedTileImagePath);
        }


        public void LeftMouseDown(object? sender, MouseEventArgs m)
        {
            //find the position of the mouse oon the world map
            List<int> checkedSpaces = new List<int>();
            List<Node> checkedNodes = new List<Node>();
            Point worldMousePos = ((Form1)sender).Mouse_Pos(sender, m);
            var clickedPoint = worldMousePos;

            //if the clicked point is not on the world map do nothing
            if (clickedPoint.X < grid.nodes.First().coords.X || clickedPoint.Y < grid.nodes.First().coords.Y || clickedPoint.X > grid.nodes.Last().coords.X || clickedPoint.Y > grid.nodes.Last().coords.Y)
            {
                return;
            }

            //if the clicked point is on the map and you have enough cash, building a specific building
            if (grid.cash >= 10000 && buildingType == "house")
            {
                //check if the building space is valid
                if (!CanPlaceBuilding(grid, clickedPoint, houseSize.Width, houseSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, houseSize.Width, houseSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                //create a new house
                House newHouse = new House(houseSize, placement, "house", 10, 10);
                newHouse.imagePath = houseImagesPath[random.Next(houseImagesPath.Count())];

                //play the placement sound
                audioManager.PlayPlaceSound();

                //add the house to the available buildings
                grid.buildings.Add(newHouse);
                grid.cash -= newHouse.cost;

                //update map tile data
                foreach (Node node in footprintNodes)
                {
                    newHouse.occupyingNodesIndex.Add(node.nodeNumber);
                    node.hasTileData = true;
                }
            }

            if (grid.cash >= 50000 && buildingType == "powerplant")
            {
                //check if the building placement is valid
                if (!CanPlaceBuilding(grid, clickedPoint, powerPlantSize.Width, powerPlantSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, powerPlantSize.Width, powerPlantSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                //construct a new power plant
                PowerPlant newPowerPlant = new PowerPlant(powerPlantSize, placement, "powerplant", 1000, 50);
                newPowerPlant.imagePath = powerPlantImagePath;

                //play the placement sound
                audioManager.PlayPlaceSound();

                //add the building to the list
                grid.buildings.Add(newPowerPlant);
                grid.cash -= newPowerPlant.cost;

                //update map tile data
                foreach (Node node in footprintNodes)
                {
                    newPowerPlant.occupyingNodesIndex.Add(node.nodeNumber);
                    node.hasTileData = true;
                }
            }

            if (grid.cash >= 50000 && buildingType == "factory")
            {
                //check if the building space is valid
                if (!CanPlaceBuilding(grid, clickedPoint, factorySize.Width, factorySize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, factorySize.Width, factorySize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                //construct a new factory
                Factory newFactory = new Factory(factorySize, placement, "factory", 100, 100);
                newFactory.imagePath = factoriesImagesPath[random.Next(factoriesImagesPath.Count())];

                //play a placement sound
                audioManager.PlayPlaceSound();

                //add a building to the list
                grid.buildings.Add(newFactory);
                grid.cash -= newFactory.cost;

                //update node tile data on the map
                foreach (Node node in footprintNodes)
                {
                    newFactory.occupyingNodesIndex.Add(node.nodeNumber);
                    node.hasTileData = true;
                }
            }

            if (grid.cash >= 30000 && buildingType == "waterpump")
            {
                //check if building placement is valid- different from other buildings as it has to be adjacent to water
                if (!CanPlaceWaterPump(grid, clickedPoint, waterPumpSize.Width, waterPumpSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, waterPumpSize.Width, waterPumpSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                //construct a new water pump
                WaterPump newWaterPump = new WaterPump(waterPumpSize, placement, "waterpump", 50, 500);
                newWaterPump.imagePath = waterPumpImagePath;

                //play the placement sound
                audioManager.PlayPlaceSound();

                //add the building to the list of buildings
                grid.buildings.Add(newWaterPump);
                grid.cash -= newWaterPump.cost;

                //update map tile data
                foreach (Node node in footprintNodes)
                {
                    newWaterPump.occupyingNodesIndex.Add(node.nodeNumber);
                    node.hasTileData = true;
                }
            }

            if (grid.cash >= 100000 && buildingType == "hospital")
            {
                //check to see if the placement is valid
                if (!CanPlaceBuilding(grid, clickedPoint, hospitalSize.Width, hospitalSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, hospitalSize.Width, hospitalSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                //construct a new hospital
                Hospital newHospital = new Hospital(hospitalSize, placement, "hospital", 250, 250, grid, carManager);
                newHospital.imagePath = hospitalImagePath;

                //play the placement sound
                audioManager.PlayPlaceSound();

                //add the hospital to the list of buildings
                grid.buildings.Add(newHospital);
                grid.cash -= newHospital.cost;

                //update node tile data
                foreach (Node node in footprintNodes)
                {
                    newHospital.occupyingNodesIndex.Add(node.nodeNumber);
                    node.hasTileData = true;
                }
            }

            if (grid.cash >= 30000 && buildingType == "shop")
            {
                //check if building placmenet is valid
                if (!CanPlaceBuilding(grid, clickedPoint, shopSize.Width, shopSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, shopSize.Width, shopSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                //construct a new shop
                Shop newShop = new Shop(shopSize, placement, "shop", 10, 10);
                newShop.imagePath = shopsImagesPath[random.Next(shopsImagesPath.Count())];

                //play the placement sound
                audioManager.PlayPlaceSound();

                //add the shop to the buildings list
                grid.buildings.Add(newShop);
                grid.cash -= newShop.cost;

                //update node tile data
                foreach (Node node in footprintNodes)
                {
                    newShop.occupyingNodesIndex.Add(node.nodeNumber);
                    node.hasTileData = true;
                }
            }

            if (grid.cash >= 100000 && buildingType == "policebuilding")
            {
                //check if the building placement is valid
                if (!CanPlaceBuilding(grid, clickedPoint, policeBuildingSize.Width, policeBuildingSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, policeBuildingSize.Width, policeBuildingSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                //construct a new police building
                PoliceBuilding newPoliceBuilding = new PoliceBuilding(policeBuildingSize, placement, "policebuilding", 50, 25, grid, carManager);
                newPoliceBuilding.imagePath = policeBuildingImagePath;

                //play a placement sound
                audioManager.PlayPlaceSound();

                //add the police building to the list of buildings
                grid.buildings.Add(newPoliceBuilding);
                grid.cash -= newPoliceBuilding.cost;

                //update node data on map
                foreach (Node node in footprintNodes)
                {
                    newPoliceBuilding.occupyingNodesIndex.Add(node.nodeNumber);
                    node.hasTileData = true;
                }
            }

            if (grid.cash >= 75000 && buildingType == "fireservice")
            {
                //check if the building placement is valid
                if (!CanPlaceBuilding(grid, clickedPoint, fireServiceSize.Width, fireServiceSize.Height)) { return; }

                var footprintNodes = GetFootprintNodes(clickedPoint, fireServiceSize.Width, fireServiceSize.Height);

                Point placement = footprintNodes.OrderBy(n => n.coords.X).ThenBy(n => n.coords.Y).First().coords;

                //construct a new fire service station
                FireService newFireService = new FireService(fireServiceSize, placement, "fireservice", 15, 350, grid, carManager);
                newFireService.imagePath = fireServiceBuildingImagePath;

                //play the placement sound
                audioManager.PlayPlaceSound();

                //add the building to the buildings list
                grid.buildings.Add(newFireService);
                grid.cash -= newFireService.cost;

                //update node tile data on the map
                foreach (Node node in footprintNodes)
                {
                    newFireService.occupyingNodesIndex.Add(node.nodeNumber);
                    node.hasTileData = true;
                }
            }
        }

        //check if the node is next to a road-node
        private bool IsDirectlyAdjacentToRoad(Node node)
        {
            int tile = rectSize;

            //check all possible directions
            Point[] directions = { new Point(tile, 0), new Point(-tile, 0), new Point(0, tile), new Point(0, -tile) };

            //for every direction
            foreach (Point dir in directions)
            {
                //check if the adjacent node is a road
                Point checkPoint = new Point(node.coords.X + dir.X, node.coords.Y + dir.Y);
                Node adjacent = grid.nodes.FirstOrDefault(n => n.coords == checkPoint);
                if (adjacent != null && adjacent.isRoad) { return true; } //if it is a road, return true
            }

            return false;
        }

        //check if the building can be placed by checking if the building is adjacent to a road and on grass
        public bool CanPlaceBuilding(Grid grid, Point mousePos, int width, int height)
        {
            //footprintNodes => the nodes that a building occupies
            List<Node> footprintNodes = new List<Node>();

            foreach (Node node in grid.nodes)
            {
                if (node.coords.X + 8 <= mousePos.X + (8 * width) && node.coords.X + 8 >= mousePos.X - (8 * width) && node.coords.Y + 8 <= mousePos.Y + (8 * height) &&node.coords.Y + 8 >= mousePos.Y - (8 * height))
                {
                    if (!node.isGrass || node.hasTileData || node.isRoad) { return false; } //checks to see if any of the nodes fulfill these invalid properties
                    footprintNodes.Add(node); //otherwise add to footprint
                }
            }

            if (footprintNodes.Count == 0) { return false; } //if no nodes in footprint, output false

            bool hasRoadAccess = footprintNodes.Any(n => IsDirectlyAdjacentToRoad(n)); //checks if the building nodes is adjacent to a road-node

            return hasRoadAccess; //if footprint nodes is valid and is adjacent to a road
        }


        //for water pumps modified to require at least one neighbor to be water
        public bool CanPlaceWaterPump(Grid grid, Point mousePos, int width, int height)
        {
            List<Node> footprintNodes = new List<Node>();

            //gather footprint nodes
            foreach (Node node in grid.nodes)
            {
                if (node.coords.X + 8 <= mousePos.X + (8 * width) &&
                    node.coords.X + 8 >= mousePos.X - (8 * width) &&
                    node.coords.Y + 8 <= mousePos.Y + (8 * height) &&
                    node.coords.Y + 8 >= mousePos.Y - (8 * height))
                {
                    //tile must be buildable grass
                    if (!node.isGrass || node.hasTileData || node.isRoad)
                        return false;

                    footprintNodes.Add(node);
                }
            }

            if (footprintNodes.Count == 0)
                return false;

            //check road adjacency
            bool hasRoadAccess = footprintNodes.Any(n => IsDirectlyAdjacentToRoad(n));
            if (!hasRoadAccess)
                return false;

            //check water adjacency
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

            if (!hasWaterAccess) { return false; }
            return true;
        }

        //finds the nodes that a building is placed over
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