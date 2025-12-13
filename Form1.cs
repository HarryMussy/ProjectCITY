/* ----------GAME PLAN----------
 * Cash system, building roads of certain lengths uses cash DONE
 * House buildings DONE
 * Electricity DONE
 * Water DONE
 * Food
 * Necessities (e.g. hospitals, police, fire etc)
 * Background map to build only in certain places DONE
 * Cars on the roads
 * Other industries e.g. farming logging
 */

using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;



namespace CitySkylines0._5alphabeta
{
    public partial class Form1 : Form
    {
        public DateTime lastTickTime;
        public string buildingType = "";
        public Grid grid;
        private float zoomLevel = 1.0f;
        private Point currentMousePos;
        public int rectSize;
        public bool selectingEdgePainting = false;
        public bool selectingBuildingPainting = false;
        public bool notselecting = true;
        public bool creatingedge = true;
        public Point screencentre;
        public bool movingtiles = false;
        public bool mousedown = false;
        public string[] allOperations = { "None", "Building Roads", "Building" };
        public int currentOperation = 0;
        public int mouseXold = 0;
        public int mouseYold = 0;
        public Point tempa;
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
        private Background backgroundMap;
        public int dx;
        public int dy;
        public long _frameCount = 0;
        public DateTime _lastCheckTime;
        public int fps;
        public bool viewGrid = false;
        private DateTime lastUpdate = DateTime.Now;
        private int lastFps = 0;
        private DateTime lastFpsUpdate = DateTime.Now;
        public LoadingForm loadingForm;
        public AudioManager audioManager;
        public SmokeParticleManager smokeParticleManager;
        public Graphics g;
        public CarManager carManager;
        public Calendar calendar;
        private Random carRandom = new Random();

        public Form1()
        {
            //initiate loading form
            loadingForm = new LoadingForm();
            loadingForm.Show();
            g = CreateGraphics();
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml("#1E7CB8");
            audioManager = new AudioManager();
            rectSize = 16;
            screencentre = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
            this.MouseWheel += Form1_MouseWheel;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            System.Windows.Forms.Timer tickSpeed = new System.Windows.Forms.Timer();
            tickSpeed.Interval = 4;
            tickSpeed.Tick += TimerTick;
            tickSpeed.Start();
            //close loading form
            loadingForm.Close();
            this.ClientSizeChanged += Form1_Resize;

            //classes... ASSEMBLE!
            DateTime now = DateTime.Now;
            calendar = new Calendar(now.Day, now.Month, now.Year, now.Hour, now.Minute);
            backgroundMap = new Background(60, 60, this, rectSize);
            grid = new Grid(60, 60, backgroundMap, rectSize);
            necessitiesManager = new NecessitiesManager(grid);
            smokeParticleManager = new SmokeParticleManager(grid);
            nameProvider = new NameProvider("roadnames.json");
            edgePainter = new EdgePainter(grid, this, nameProvider, backgroundMap, g);
            buildingPainter = new BuildingPainter(grid, this, g, rectSize, calendar);
            buttonManager = new InteractingObjectManager();
            carManager = new CarManager(grid, calendar);
            List<EventHandler> allEventHandlers = new List<EventHandler>();

            allEventHandlers.Add(Form1_RoadButton);
            allEventHandlers.Add(Form1_ToggleNames);
            allEventHandlers.Add((sender, e) => Form1_BuildingBuilder(sender, e, buildingType));
            allEventHandlers.Add(Form1_ViewBuildingSpaces);
            allEventHandlers.Add(Form1_toggleGrid);
            allEventHandlers.Add(Form1_ChangeVolume);
            uiManager = new UIManager(zoomLevel, () => (this.ClientSize.Width, this.ClientSize.Height), grid, buttonManager, this, allEventHandlers, calendar);
            Form1_PlayRandomTrack();
        }

        public Form1(SaveManager.SaveData save)
        {
            loadingForm = new LoadingForm();
            loadingForm.Show();
            g = CreateGraphics();
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml("#1E7CB8");
            audioManager = new AudioManager();
            rectSize = 16;
            screencentre = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
            this.MouseWheel += Form1_MouseWheel;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            System.Windows.Forms.Timer tickSpeed = new System.Windows.Forms.Timer();
            tickSpeed.Interval = 16;
            tickSpeed.Tick += TimerTick;
            tickSpeed.Start();
            loadingForm.Close();
            this.ClientSizeChanged += Form1_Resize;

            // If the save contains a Grid/Calendar, use them; otherwise fall back to defaults.

            backgroundMap = new Background(60, 60, this, rectSize);
            if (save != null && save.grid != null)
            {
                grid = save.grid;
            }
            else
            {
                grid = new Grid(60, 60, backgroundMap, rectSize);
            }

            if (save != null && save.calendar != null)
            {
                calendar = save.calendar;
            }
            else
            {
                DateTime now = DateTime.Now;
                calendar = new Calendar(now.Day, now.Month, now.Year, now.Hour, now.Minute);
            }

            // re-create managers that depend on grid/background/calendar
            necessitiesManager = new NecessitiesManager(grid);
            smokeParticleManager = new SmokeParticleManager(grid);
            nameProvider = new NameProvider("roadnames.json");
            edgePainter = new EdgePainter(grid, this, nameProvider, backgroundMap, g);
            buildingPainter = new BuildingPainter(grid, this, g, rectSize, calendar);
            buttonManager = new InteractingObjectManager();
            carManager = new CarManager(grid, calendar);

            List<EventHandler> allEventHandlers = new List<EventHandler>();
            allEventHandlers.Add(Form1_RoadButton);
            allEventHandlers.Add(Form1_ToggleNames);
            allEventHandlers.Add((sender, e) => Form1_BuildingBuilder(sender, e, buildingType));
            allEventHandlers.Add(Form1_ViewBuildingSpaces);
            allEventHandlers.Add(Form1_toggleGrid);
            allEventHandlers.Add(Form1_ChangeVolume);
            uiManager = new UIManager(zoomLevel, () => (this.ClientSize.Width, this.ClientSize.Height), grid, buttonManager, this, allEventHandlers, calendar);
            Form1_PlayRandomTrack();
        }

        private void Form1_ChangeVolume(object? sender, EventArgs e)
        {
            try
            {
                if (sender is TrackBar slider)
                {
                    float volume = slider.Value / 100f;
                    foreach (var volProvider in audioManager.trackVolumeProviders)
                    {
                        volProvider.Volume = volume;
                    }
                    audioManager.volume = volume;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Volume change error: " + ex.Message);
            }
        }

        public void Form1_PlayRandomTrack()
        {
            Random rnd = new Random();
            int i = rnd.Next(1, 4);
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string filepath = Path.Combine(projectRoot, @$"gameAssets\audio\Tracks\track{i}.wav");
            audioManager.PlayTrack(filepath, false);
        }

        public int GetFps()
        {
            var now = DateTime.Now;
            var elapsed = (now - lastFpsUpdate).TotalSeconds;

            if (elapsed >= 1.0)
            {
                long count = Interlocked.Exchange(ref _frameCount, 0);
                lastFps = (int)(count / elapsed);
                lastFpsUpdate = now;
            }

            return lastFps;
        }


        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                uiManager.UpdateUI();
            }
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            screencentre = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
            bottomLeft = new Point(0, this.ClientSize.Height);
            smokeParticleManager.Update();
            fps = GetFps();
            this.Invalidate();

            var now = DateTime.Now;
            double elapsedMs = (now - lastTickTime).TotalMilliseconds;
            lastTickTime = now;
            calendar.AdvanceTime(elapsedMs);

            // Reset both demand and supply at the start of each tick
            necessitiesManager.globalElectricityDemand = 0;
            necessitiesManager.globalWaterDemand = 0;
            necessitiesManager.globalElectricitySupply = 0;
            necessitiesManager.globalWaterSupply = 0;

            foreach (Building b in grid.buildings)
            {
                necessitiesManager.UpdateGlobalNecessities();
                bool necessitiesFilled = true;
                foreach (Necessity n in b.necessities)
                {
                    if (!n.fulFilled) { necessitiesFilled = false; break; }
                }
                if (necessitiesFilled) { grid.cash += b.tax; } //taxes the houses as long as they have running water and electricity
            }

            if (necessitiesManager.globalElectricitySupply > necessitiesManager.globalElectricityDemand) { grid.cash += (necessitiesManager.globalElectricitySupply - necessitiesManager.globalElectricityDemand) * 3; } //sells excess electricity for cash
            if (necessitiesManager.globalWaterSupply > necessitiesManager.globalWaterDemand) { grid.cash += (necessitiesManager.globalWaterSupply - necessitiesManager.globalWaterDemand) * 3 / 1000; } //sells excess water for cash

            foreach (Edge edge in grid.edges)
            {
                edge.IntersectionAlreadyExists();
            }

            foreach (Node n in grid.nodes)
            {
                n.IsNodeBuildable();
                if (n.tileData != null) { grid.cash -= (float)0.01; } //charge for maintaining buildable land
            }

            buildingPainter.buildingType = buildingType;
            backgroundMap.UpdateWaterAnimations();

            //spawns cars if there are less cars than houses and in a 1% chance
            if (carRandom.NextDouble() < 0.1 && carManager.cars.Count <= grid.buildings.Count)
            {
                SpawnCarNearBuilding();
            }

            List<Car> carsToRemove = new List<Car>();
            foreach (Car car in carManager.cars.ToList()) // iterate a copy to be safe
            {
                try
                {
                    bool finished = carManager.MoveCar(car);
                    if (finished)
                        carsToRemove.Add(car);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TimerTick MoveCar exception: " + ex.ToString());
                    // don't crash whole game — mark car for removal or stop it
                    car.isMoving = false;
                }
            }
            foreach (Car car in carsToRemove)
            {
                carManager.cars.Remove(car);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string iconPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "projectCityMain.ico");

            this.CreateGraphics();
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

        private void Form1_Paint(object? sender, PaintEventArgs p)
        {
            Interlocked.Increment(ref _frameCount);
            g = p.Graphics;
            Pen bluePen = new Pen(Color.Blue, 1);
            Brush whiteBrush = new SolidBrush(Color.White);

            g.TranslateTransform(screencentre.X, screencentre.Y);
            g.ScaleTransform(zoomLevel, zoomLevel);
            g.TranslateTransform(-screencentre.X, -screencentre.Y);



            backgroundMap.DrawMap(sender, g, zoomLevel);
            edgePainter.RoadPaint(sender, g, currentMousePos);

            // draw night BEFORE cars
            g.ResetTransform();
            calendar.TimePainter(sender, g);  // darkness overlay

            // restore world transform
            g.TranslateTransform(screencentre.X, screencentre.Y);
            g.ScaleTransform(zoomLevel, zoomLevel);
            g.TranslateTransform(-screencentre.X, -screencentre.Y);

            // now draw cars on top of darkness
            carManager.CarPaint(sender, g);
            buildingPainter.BuildingPaint(sender, g, currentMousePos);

            g.ResetTransform();
            uiManager.ConstructUI(sender, g);

        }

        public void Form1_toggleGrid(object? sender, EventArgs e)
        {
            viewGrid = !viewGrid;
        }


        public Point Mouse_Pos(object? sender, MouseEventArgs m)
        {
            return new Point(
                (int)((m.Location.X - screencentre.X) / zoomLevel + screencentre.X),
                (int)((m.Location.Y - screencentre.Y) / zoomLevel + screencentre.Y)
            );
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs m)
        {
            if (selectingEdgePainting == true)
            {
                if (m.Button == MouseButtons.Left)
                {
                    //num decides whether to find point a or point b based off of whether num is even
                    edgePainter.LeftMouseDown(sender, m);
                }
                if (m.Button == MouseButtons.Right)
                {
                    edgePainter.startPoint = null;
                }
            }
            else if (selectingBuildingPainting == true)
            {
                if (m.Button == MouseButtons.Left)
                {
                    buildingPainter.LeftMouseDown(sender, m);
                }
            }
            else
            {
                if (m.Button == MouseButtons.Left)
                {
                    movingtiles = true;
                    mousedown = true;
                    mouseXold = m.X;
                    mouseYold = m.Y;
                }
            }
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs m)
        {
            //zoom based on wheel direction
            float scaleFactor = (m.Delta > 0) ? 1.1f : 0.9f; //zoom in or out
            float newZoomLevel = zoomLevel * scaleFactor;
            newZoomLevel = Math.Max(0.1f, Math.Min(newZoomLevel, 10f));

            //update the zoom level
            zoomLevel = newZoomLevel;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs m)
        {
            /*if (!Control.MouseButtons.HasFlag(MouseButtons.Left))
            {
                movingtiles = false;
                mousedown = false;
                return;
            }*/

            currentMousePos = Mouse_Pos(this, m);
            if (movingtiles && mousedown)
            {
                dx = Convert.ToInt32((currentMousePos.X - mouseXold) / zoomLevel);
                dy = Convert.ToInt32((currentMousePos.Y - mouseYold) / zoomLevel);

                foreach (Edge edge in grid.edges)
                {
                    edge.a = new Point(edge.a.X + dx, edge.a.Y + dy);
                    edge.b = new Point(edge.b.X + dx, edge.b.Y + dy);

                    foreach (IntersectingNode node in edge.intersections)
                    {
                        node.coords = new Point(node.coords.X + dx, node.coords.Y + dy);
                    }

                    List<Point> pointsTemp = edge.pointsOnTheEdge.ToList();
                    for (int i = 0; i < pointsTemp.Count; i++)
                    {
                        Point point = pointsTemp[i];
                        point.X += dx;
                        point.Y += dy;
                        edge.pointsOnTheEdge[i] = point;
                    }
                }

                foreach (Car c in carManager.cars)
                {
                    c.currentPosition.X += dx;
                    c.currentPosition.Y += dy;
                }

                foreach (Building b in grid.buildings)
                {
                    b.coords = new Point(b.coords.X + dx, b.coords.Y + dy);
                }

                foreach (Node node in backgroundMap.tiles)
                {
                    node.coords = new Point(node.coords.X + dx, node.coords.Y + dy);
                }

                foreach (Node node in grid.nodes)
                {
                    node.coords = new Point(node.coords.X + dx, node.coords.Y + dy);
                }
                mouseXold = currentMousePos.X;
                mouseYold = currentMousePos.Y;
            }
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

        private void Form1_RoadButton(object? sender, EventArgs e)
        {
            if (selectingEdgePainting == false)
            {
                selectingEdgePainting = true;
                selectingBuildingPainting = false;
                notselecting = false;
                currentOperation = 1;
            }
            else
            {
                notselecting = true;
                selectingEdgePainting = false;
                selectingBuildingPainting = false;
                currentOperation = 0;
            }
        }

        public void Form1_BuildingBuilder(object? sender, EventArgs e, string typeIn)
        {
            if (selectingBuildingPainting == false)
            {
                notselecting = false;
                selectingBuildingPainting = true;
                selectingEdgePainting = false;
                allOperations[2] = "Building " + typeIn.ToUpper();
                currentOperation = 2;
                buildingType = typeIn;
            }
            else
            {
                notselecting = true;
                selectingBuildingPainting = false;
                selectingEdgePainting = false;
                buildingType = "hello";
                currentOperation = 0;
            }
        }

        private void Form1_ViewBuildingSpaces(object? sender, EventArgs e)
        {
            buildingPainter.viewBuildingSpaces = !buildingPainter.viewBuildingSpaces;
        }

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

        private void SpawnCarNearBuilding()
        {
            if (grid.buildings == null || grid.buildings.Count == 0) return;
            if (grid.roadNodes == null || grid.roadNodes.Count == 0) return;

            // use the shared RNG
            var rng = carRandom;

            var building = grid.buildings[rng.Next(grid.buildings.Count)];
            if (building == null) return;

            Node closestNode = grid.roadNodes.OrderBy(n => Distance(building.coords, n.coords)).FirstOrDefault();
            if (closestNode == null) return;

            building = grid.buildings[rng.Next(grid.buildings.Count)];
            if (building == null) return;

            Node nDest = grid.roadNodes.OrderBy(n => Distance(building.coords, n.coords)).FirstOrDefault();
            if (nDest == null) return;

            Car car = new Car(closestNode, 3, nDest);
            carManager.AssignImage(car);
            car.route = carManager.CreateCarRoute(car);

            if (car.route == null || car.route.Count == 0)
            {
                Debug.WriteLine("Car has no route — cannot move.");
                // decide: either don't add the car or add it but mark not moving
                return;
            }

            carManager.cars.Add(car);
        }


        private float Distance(Point a, Point b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /*public void SaveGame(string filepath)
        {
            SaveData data = new SaveData();

            data.Cash = grid.cash;

            // Save Roads
            data.Edges = grid.edges.Select(e => new SaveEdge
            {
                Weight = e.edgeweight,
                Name = e.name,
                A = e.a,
                B = e.b
            }).ToList();

            // Save Buildings
            data.Buildings = grid.buildings.Select(b => new SaveBuilding
            {
                Type = b.type,
                Coords = b.coords,
                Width = b.size.Width,
                Height = b.size.Height
            }).ToList();

            // Save Time
            data.Calendar = new SaveCalendar
            {
                Day = calendar.day,
                Month = calendar.month,
                Year = calendar.year,
                Hour = calendar.hour,
                Minute = calendar.minute
            };

            string json = System.Text.Json.JsonSerializer.Serialize(
                data,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(filepath, json);
        }*/

        /*public void LoadGame(string filepath)
        {
            string json = File.ReadAllText(filepath);
            SaveData data = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json);

            // Clear current game world
            grid.edges.Clear();
            grid.buildings.Clear();

            // Load cash
            grid.cash = data.Cash;

            // Restore time
            calendar = new Calendar(
                data.Calendar.Day,
                data.Calendar.Month,
                data.Calendar.Year,
                data.Calendar.Hour,
                data.Calendar.Minute
            );

            // Restore roads
            foreach (var e in data.Edges)
            {
                Road road = new Road(e.Weight, e.A, e.B, e.Name);
                grid.edges.Add(road);
            }

            // Restore buildings
            foreach (var b in data.Buildings)
            {
                Building building = b.Type switch
                {
                    "house" => new House(new Size(b.Width, b.Height), b.Coords, b.Type),
                    "windfarm" => new Windfarm(new Size(b.Width, b.Height), b.Coords, b.Type),
                    "waterpump" => new WaterPump(new Size(b.Width, b.Height), b.Coords, b.Type),

                    _ => null
                };

                if (building != null)
                    grid.buildings.Add(building);
            }

            // Regenerate buildable nodes and road intersections
            grid.FindRoadNodeIntersections();

            Invalidate(); // redraw
        }*/

        public void SaveGameToPath(string path) { SaveManager.SaveGameToFile(path, this.grid, this.calendar); }
        public void LoadGameFromPath()
        {
            SaveManager.SaveData data = SaveManager.LoadGameFromFile();
            if (data != null)
            {
                this.grid = data.grid;
                this.calendar = data.calendar;
            }
        }


        public void ReturnToMainMenu()
        {
            this.Hide();
            new MainMenuForm().ShowDialog();
            this.Close();
        }
    }
}