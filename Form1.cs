using System.IO;

namespace ProjectCity
{
    public partial class Form1 : Form
    {
        public DateTime lastTickTime;
        public string buildingType = "";
        public Grid grid;
        public float zoomLevel = 1.0f;
        private Point mousePos;
        public int rectSize;
        public bool selectingEdgePainting = false;
        public bool selectingBuildingPainting = false;
        public bool selectingBulldozing = false;
        public bool notselecting = true;
        public bool creatingedge = true;
        public Point screencentre;
        bool viewGrid = false;
        public bool movingtiles = false;
        public bool mousedown = false;
        public string[] allOperations = { "None", "Building Roads", "Building", "Bulldozing" };
        public int currentOperation = 0;
        public int mouseXold = 0;
        public int mouseYold = 0;
        public float closestX = float.MaxValue;
        public float closestY = float.MaxValue;
        public Brush redBrush = new SolidBrush(Color.Red);
        public Point bottomLeft;
        public NameProvider nameProvider;
        InteractingObjectManager buttonManager;
        public NecessitiesManager necessitiesManager;
        private readonly EdgePainter edgePainter;
        private readonly UIManager uiManager;
        private readonly BuildingPainter buildingPainter;
        public PopulationManager populationManager;
        public Background background;
        public Bulldozer bulldozer;
        public int dx;
        public int dy;
        public long _frameCount = 0;
        public DateTime _lastCheckTime;
        public int fps;
        private int lastFps = 0;
        private DateTime lastFpsUpdate = DateTime.Now;
        public AudioManager audioManager;
        public Graphics g;
        public CarManager carManager;
        public Calendar calendar;
        public bool ReturnToMenu = false;

        public int gridDimensions = 100;
        public Point camera = new Point(0, 0);
        Random spawnCarRandom = new Random();

        public Form1(int difficulty, AudioManager audioManagerIn)
        {
            g = CreateGraphics();
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml("#1E7CB8");
            audioManager = audioManagerIn;
            rectSize = 16;
            screencentre = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
            this.MouseWheel += Form1_MouseWheel;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            System.Windows.Forms.Timer tickSpeed = new System.Windows.Forms.Timer();
            tickSpeed.Interval = 16; //~60fps
            tickSpeed.Tick += TimerTick;
            tickSpeed.Start();
            this.ClientSizeChanged += Form1_Resize;

            //classes... ASSEMBLE!
            DateTime now = DateTime.Now;
            calendar = new Calendar(now.Day, now.Month, now.Year, now.Hour, now.Minute, this);
            background = new Background(gridDimensions, gridDimensions, this, rectSize, difficulty);
            calendar.UpdateCurrentSeason();
            grid = new Grid(gridDimensions, gridDimensions, background, rectSize);
            necessitiesManager = new NecessitiesManager(grid);
            nameProvider = new NameProvider("roadnames.json");
            edgePainter = new EdgePainter(grid, this, nameProvider, background, g, rectSize);
            buttonManager = new InteractingObjectManager();
            carManager = new CarManager(grid, calendar);
            buildingPainter = new BuildingPainter(grid, this, g, rectSize, calendar, carManager);
            populationManager = new PopulationManager(grid);
            bulldozer = new Bulldozer(grid, this);
            List<EventHandler> allEventHandlers = new List<EventHandler>();

            allEventHandlers.Add(Form1_RoadButton);
            allEventHandlers.Add(Form1_ToggleNames);
            allEventHandlers.Add((sender, e) => Form1_BuildingBuilder(sender, e, buildingType));
            allEventHandlers.Add(Form1_ViewBuildingSpaces);
            allEventHandlers.Add(Form1_toggleGrid);
            allEventHandlers.Add(Form1_ChangeVolume);
            allEventHandlers.Add(Form1_BulldozingButton);
            uiManager = new UIManager(zoomLevel, () => (this.ClientSize.Width, this.ClientSize.Height), grid, buttonManager, this, allEventHandlers, calendar);
            Form1_PlayRandomTrack();

            lastTickTime = DateTime.Now;
        }

        //difficulty falls to 1 if there is no difficulty input
        public Form1(AudioManager audioManagerIn)
        {
            g = CreateGraphics();
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml("#1E7CB8");
            audioManager = audioManagerIn;
            rectSize = 16;
            screencentre = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
            this.MouseWheel += Form1_MouseWheel;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            System.Windows.Forms.Timer tickSpeed = new System.Windows.Forms.Timer();
            tickSpeed.Interval = 8;
            tickSpeed.Tick += TimerTick;
            tickSpeed.Start();
            this.ClientSizeChanged += Form1_Resize;

            //classes... ASSEMBLE!
            DateTime now = DateTime.Now;
            calendar = new Calendar(now.Day, now.Month, now.Year, now.Hour, now.Minute, this);
            background = new Background(gridDimensions, gridDimensions, this, rectSize, 1);
            grid = new Grid(gridDimensions, gridDimensions, background, rectSize);
            necessitiesManager = new NecessitiesManager(grid);
            nameProvider = new NameProvider("roadnames.json");
            edgePainter = new EdgePainter(grid, this, nameProvider, background, g, rectSize);
            buttonManager = new InteractingObjectManager();
            carManager = new CarManager(grid, calendar);
            buildingPainter = new BuildingPainter(grid, this, g, rectSize, calendar, carManager);
            populationManager = new PopulationManager(grid);
            bulldozer = new Bulldozer(grid, this);

            List<EventHandler> allEventHandlers = new List<EventHandler>();

            allEventHandlers.Add(Form1_RoadButton);
            allEventHandlers.Add(Form1_ToggleNames);
            allEventHandlers.Add((sender, e) => Form1_BuildingBuilder(sender, e, buildingType));
            allEventHandlers.Add(Form1_ViewBuildingSpaces);
            allEventHandlers.Add(Form1_toggleGrid);
            allEventHandlers.Add(Form1_ChangeVolume);
            allEventHandlers.Add(Form1_BulldozingButton);
            uiManager = new UIManager(zoomLevel, () => (this.ClientSize.Width, this.ClientSize.Height), grid, buttonManager, this, allEventHandlers, calendar);
            Form1_PlayRandomTrack();
            lastTickTime = DateTime.Now;
        }

        public Form1(SaveManager.SaveData save, AudioManager audioManagerIn)
        {
            //initialize component FIRST, before any game logic
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml("#1E7CB8");
            audioManager = audioManagerIn;
            rectSize = 16;
            screencentre = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);

            //create graphics AFTER form is initialized
            g = CreateGraphics();

            this.MouseWheel += Form1_MouseWheel;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            this.ClientSizeChanged += Form1_Resize;

            System.Windows.Forms.Timer tickSpeed = new System.Windows.Forms.Timer();
            tickSpeed.Interval = 16;
            tickSpeed.Tick += TimerTick;
            tickSpeed.Start();

            if (save != null && save.background != null)
            {
                background = save.background;
            }
            else
            {
                background = new Background(gridDimensions, gridDimensions, this, rectSize, 1);
            }

            if (save != null && save.grid != null)
            {
                grid = save.grid;
            }
            else
            {
                grid = new Grid(gridDimensions, gridDimensions, background, rectSize);
            }

            if (save != null && save.calendar != null)
            {
                calendar = save.calendar;
                calendar.form1PassIn = this; //re-link form reference lost during serialisation
            }
            else
            {
                DateTime now = DateTime.Now;
                calendar = new Calendar(now.Day, now.Month, now.Year, now.Hour, now.Minute, this);
            }

            List<Person> population;
            if (save != null && save.calendar != null)
            {
                population = save.population;
            }
            else
            {
                population = new List<Person>();
            }

            if (save != null && save.necessitiesManager != null)
            {
                necessitiesManager = save.necessitiesManager;
            }
            else
            {
                necessitiesManager = new NecessitiesManager(grid);
            }

            //create managers with initialized graphics and form
            nameProvider = new NameProvider("roadnames.json");
            edgePainter = new EdgePainter(grid, this, nameProvider, background, g, rectSize);
            buttonManager = new InteractingObjectManager();
            carManager = new CarManager(grid, calendar);
            buildingPainter = new BuildingPainter(grid, this, g, rectSize, calendar, carManager);
            populationManager = new PopulationManager(grid);
            populationManager.Population = population;

            if (save != null && save.averageWellBeing > 0)
            {
                populationManager.AverageWellBeing = save.averageWellBeing;
            }
            if (save != null && save.globalDesires != null)
            {
                populationManager.GlobalDesires = save.globalDesires;
            }

            bulldozer = new Bulldozer(grid, this);

            //[JsonIgnore] fields are stripped during serialisation so must be reconnected manually
            foreach (Building b in grid.buildings)
            {
                b.InitializeAfterLoad();
                if (b is Hospital hospital)
                {
                    hospital.Reconnect(grid, carManager);
                }
                if (b is PoliceBuilding police)
                {
                    police.Reconnect(grid, carManager);
                }
                if (b is FireService fire)
                {
                    fire.Reconnect(grid, carManager);
                }
            }

            background.InitializeAfterLoad(this);
            necessitiesManager.InitialiseAfterBoot(grid);

            //reload road placement and logic
            foreach (Road road in grid.roads)
            {
                road.RebuildAfterLoad();
            }

            grid.FindRoadTilesAndAdjacentRoadTiles();

            foreach (Road road in grid.roads)
            {
                road.lane1.occupyingNodesIndex = grid.FindRoadTilesForSpecificEdge(road.lane1, 0);
                road.lane2.occupyingNodesIndex = grid.FindRoadTilesForSpecificEdge(road.lane2, 1);
            }

            //rebuild A* pathfinding graph from the restored road network
            grid.RebuildEntireRoadGraph();

            List<EventHandler> allEventHandlers = new List<EventHandler>();
            allEventHandlers.Add(Form1_RoadButton);
            allEventHandlers.Add(Form1_ToggleNames);
            allEventHandlers.Add((sender, e) => Form1_BuildingBuilder(sender, e, buildingType));
            allEventHandlers.Add(Form1_ViewBuildingSpaces);
            allEventHandlers.Add(Form1_toggleGrid);
            allEventHandlers.Add(Form1_ChangeVolume);
            allEventHandlers.Add(Form1_BulldozingButton);
            uiManager = new UIManager(zoomLevel, () => (this.ClientSize.Width, this.ClientSize.Height), grid, buttonManager, this, allEventHandlers, calendar);
            Form1_PlayRandomTrack();

            lastTickTime = DateTime.Now;
        }


        public void ReturnToMainMenu()
        {
            ReturnToMenu = true;
            this.Close();
        }

        //changes the volume
        private void Form1_ChangeVolume(object? sender, EventArgs e)
        {
            if (sender is TrackBar slider)
            {
                float volume = slider.Value / 100f;
                foreach (var volProvider in audioManager.trackVolumeProviders)
                {
                    volProvider.Volume = volume;
                }
            }
        }

        //chooses a random track from the available tracks and plays it
        public void Form1_PlayRandomTrack()
        {
            Random rnd = new Random();
            int i = rnd.Next(1, 4);
            string root = AppContext.BaseDirectory;
            string filepath = Path.Combine(root, @$"gameAssets\audio\Tracks\track{i}.wav");
            audioManager.PlayTrack(filepath, false);
        }

        //FPS counter: counts frames over a 1 second window using an atomic exchange to avoid race conditions
        public int GetFps()
        {
            var now = DateTime.Now;
            var elapsed = (now - lastFpsUpdate).TotalSeconds;

            if (elapsed >= 1.0)
            {
                //atomically swap frame count to zero and read the value in one operation
                long count = Interlocked.Exchange(ref _frameCount, 0);
                lastFps = (int)(count / elapsed);
                lastFpsUpdate = now;
            }

            return lastFps;
        }

        //resizes the UI when the window state changes
        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                uiManager.UpdateUI();
            }
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            //recalculate screen centre each tick in case the window has been resized
            screencentre = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
            bottomLeft = new Point(0, this.ClientSize.Height);
            fps = GetFps();

            //calculate real milliseconds elapsed since last tick for time-based updates
            var now = DateTime.Now;
            double elapsedms = (now - lastTickTime).TotalMilliseconds;
            lastTickTime = now;
            calendar.AdvanceTime(elapsedms);

            //reset both demand and supply at the start of each tick
            necessitiesManager.globalPowerDemand = 0;
            necessitiesManager.globalWaterDemand = 0;
            necessitiesManager.globalPowerSupply = 0;
            necessitiesManager.globalWaterSupply = 0;
            necessitiesManager.UpdateGlobalNecessities();

            foreach (Building b in grid.buildings)
            {
                b.UpdateBuilding(elapsedms / 1000); //pass in seconds not milliseconds
                bool necessitiesFilled = true;
                foreach (Necessity n in b.necessities)
                {
                    if (!n.fulFilled) { necessitiesFilled = false; break; }
                }

                if (necessitiesFilled)
                {
                    //scale tax income by wellbeing, unhappy citizens pay less tax
                    float modifier = populationManager.AverageWellBeing / 100f;
                    grid.cash += b.tax * modifier;
                }
                else
                {
                    grid.cash -= b.tax / 100;
                }
            }

            //sell excess electricity and water for passive income
            if (necessitiesManager.globalPowerSupply > necessitiesManager.globalPowerDemand) { grid.cash += (necessitiesManager.globalPowerSupply - necessitiesManager.globalPowerDemand) / 1000; }
            if (necessitiesManager.globalWaterSupply > necessitiesManager.globalWaterDemand) { grid.cash += (necessitiesManager.globalWaterSupply - necessitiesManager.globalWaterDemand) / 1000; }

            buildingPainter.buildingType = buildingType;

            //spawn a car with 15% chance each tick, capped at 1 car per 3 buildings to avoid overcrowding roads
            if (carManager.cars.Count() < grid.buildings.Count() / 3 && spawnCarRandom.Next(100) < 15)
            {
                carManager.SpawnCarNearBuilding();
            }

            //clear occupying car states (to prevent grid locks)
            foreach (Node n in grid.nodes.Where(n => n.isRoad))
            {
                n.OccupyingCar = null;
            }

            //adds and removes cars
            List<Car> carsToRemove = new List<Car>();
            foreach (Car car in carManager.cars.ToList()) // iterate a copy to be safe
            {
                bool needsRemoving = carManager.MoveCar(car);
                if (needsRemoving) { carsToRemove.Add(car); }
                if (car.currentNode != null) { car.currentNode.OccupyingCar = car; } //reassign nodes occupying car (to prevent grid locks)
            }

            foreach (Car car in carsToRemove)
            {
                carManager.DespawnCar(car);
            }

            //updates population
            populationManager.UpdatePopulation();
            populationManager.UpdateWellBeing();

            this.Invalidate();
        }

        //loads game items
        private void Form1_Load(object sender, EventArgs e)
        {
            string projectRoot = AppContext.BaseDirectory;
            string iconPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "projectCityMain.ico");

            this.CreateGraphics();
            //enable double buffering to prevent screen flicker during rendering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            WindowState = FormWindowState.Maximized;
            this.Paint += Form1_Paint;
            this.Icon = new Icon(iconPath);
            this.Text = "PROJECT CITY";
        }

        private void Form1_ToggleNames(object? sender, EventArgs e)
        {
            edgePainter.toggleRoadNames = !edgePainter.toggleRoadNames;
        }

        //rendering pipeline: world transforms are applied before drawing world elements, then reset for UI so it always draws in screen space
        private void Form1_Paint(object? sender, PaintEventArgs p)
        {
            Interlocked.Increment(ref _frameCount);
            g = p.Graphics;
            Pen bluePen = new Pen(Color.Blue, 1);
            Brush whiteBrush = new SolidBrush(Color.White);

            //apply camera pan and zoom, translate to screen centre, scale, translate back
            g.TranslateTransform(screencentre.X - camera.X, screencentre.Y - camera.Y);
            g.ScaleTransform(zoomLevel, zoomLevel);
            g.TranslateTransform(-screencentre.X, -screencentre.Y);

            background.DrawMap(sender, g, zoomLevel);

            if (viewGrid)
            {
                foreach (Node n in grid.nodes)
                {
                    g.DrawRectangle(bluePen, n.coords.X, n.coords.Y, 16, 16);
                }
            }

            edgePainter.RoadPaint(sender, g, mousePos);

            //reset transform before drawing the night overlay so it covers the full screen
            g.ResetTransform();
            calendar.TimePainter(sender, g);

            //reapply world transform so buildings and cars render on top of the overlay
            g.TranslateTransform(screencentre.X - camera.X, screencentre.Y - camera.Y);
            g.ScaleTransform(zoomLevel, zoomLevel);
            g.TranslateTransform(-screencentre.X, -screencentre.Y);

            buildingPainter.BuildingPaint(sender, g, mousePos);
            carManager.CarPaint(sender, g);
            bulldozer.BulldozerPainter(sender, g);

            //reset transform so the UI draws in screen space, unaffected by camera
            g.ResetTransform();
            uiManager.ConstructUI(sender, g);
        }

        public void Form1_toggleGrid(object? sender, EventArgs e)
        {
            viewGrid = !viewGrid;
        }

        //converts a screen-space mouse position into world-space coordinates
        //accounts for the current camera offset and zoom level
        public Point Mouse_Pos(object? sender, MouseEventArgs m)
        {
            float x = m.X - (screencentre.X - camera.X);
            float y = m.Y - (screencentre.Y - camera.Y);

            x /= zoomLevel;
            y /= zoomLevel;

            x += screencentre.X;
            y += screencentre.Y;

            return new Point((int)x, (int)y);
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs m)
        {
            if (selectingEdgePainting)
            {
                if (m.Button == MouseButtons.Left)
                {
                    edgePainter.LeftMouseDown(sender, m);
                }
                if (m.Button == MouseButtons.Right)
                {
                    //right click cancels current road placement
                    edgePainter.startPoint = null;
                }
            }
            else if (selectingBuildingPainting)
            {
                if (m.Button == MouseButtons.Left)
                {
                    buildingPainter.LeftMouseDown(sender, m);
                }
            }
            else if (selectingBulldozing && m.Button == MouseButtons.Left)
            {
                bulldozer.Bulldozing(sender, mousePos, true, m);
            }
            else
            {
                if (m.Button == MouseButtons.Left)
                {
                    //no tool selected, begin camera drag
                    movingtiles = true;
                    mousedown = true;
                    mouseXold = m.X;
                    mouseYold = m.Y;
                }
            }
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs m)
        {
            //zoom in or out depending on scroll direction
            float scaleFactor = (m.Delta > 0) ? 1.1f : 0.9f;
            float newZoomLevel = zoomLevel * scaleFactor;
            newZoomLevel = Math.Max(0.1f, Math.Min(newZoomLevel, 10f));

            zoomLevel = newZoomLevel;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs m)
        {
            mousePos = Mouse_Pos(sender, m);
            if (movingtiles && mousedown)
            {
                //move camera opposite to mouse movement to create a drag-the-world effect
                dx = m.X - mouseXold;
                dy = m.Y - mouseYold;

                camera.X -= dx;
                camera.Y -= dy;

                mouseXold = m.X;
                mouseYold = m.Y;
            }
            if (selectingBulldozing) { bulldozer.Bulldozing(sender, mousePos, false, m); }
        }

        private void Form1_MouseUp(object? sender, MouseEventArgs m)
        {
            if (movingtiles && mousedown)
            {
                if (m.Button == MouseButtons.Left)
                {
                    movingtiles = false;
                    mousedown = false;
                    mouseXold = 0;
                    mouseYold = 0;
                }
            }
        }

        private void Form1_BulldozingButton(object? sender, EventArgs e)
        {
            if (selectingBulldozing == false)
            {
                selectingEdgePainting = false;
                selectingBuildingPainting = false;
                notselecting = false;
                selectingBulldozing = true;
                currentOperation = 3;
                edgePainter.startPoint = null;
            }
            else
            {
                selectingBulldozing = false;
                notselecting = false;
                selectingEdgePainting = false;
                selectingBuildingPainting = false;
                currentOperation = 0;
                edgePainter.startPoint = null;
            }
        }

        private void Form1_RoadButton(object? sender, EventArgs e)
        {
            if (selectingEdgePainting == false)
            {
                selectingEdgePainting = true;
                selectingBuildingPainting = false;
                notselecting = false;
                edgePainter.startPoint = null;
                selectingBulldozing = false;
                currentOperation = 1;
            }
            else
            {
                selectingBulldozing = false;
                notselecting = true;
                selectingEdgePainting = false;
                selectingBuildingPainting = false;
                edgePainter.startPoint = null;
                currentOperation = 0;
            }
        }

        //clicking the same building type again deselects it; clicking a different type switches to it
        public void Form1_BuildingBuilder(object? sender, EventArgs e, string typeIn)
        {
            if (selectingBuildingPainting == false || allOperations[2] != "Building " + typeIn.ToUpper())
            {
                notselecting = false;
                selectingBuildingPainting = true;
                selectingEdgePainting = false;
                selectingBulldozing = false;
                allOperations[2] = "Building " + typeIn.ToUpper();
                currentOperation = 2;
                buildingType = typeIn;
                edgePainter.startPoint = null;
            }
            else
            {
                //clicking the same building type again deselects it
                selectingBulldozing = false;
                notselecting = true;
                selectingBuildingPainting = false;
                selectingEdgePainting = false;
                edgePainter.startPoint = null;
                buildingType = "hello";
                currentOperation = 0;
            }
        }

        private void Form1_ViewBuildingSpaces(object? sender, EventArgs e)
        {
            edgePainter.viewBuildingSpaces = !edgePainter.viewBuildingSpaces;
        }

        //draws text with a stroke outline by rendering it offset in every direction
        //improves readability of text drawn over the game world
        public void AddStrokeToText(object? sender, Graphics g, string text, int strokeWidth, Font font, Brush brush, Point point)
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
}