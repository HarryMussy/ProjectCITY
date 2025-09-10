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
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Taskbar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
namespace CitySkylines0._5alphabeta
{
    public partial class Form1 : Form
    {
        public string buildingType = "hello";
        public readonly Grid grid;
        private float zoomLevel = 1.0f;
        private Point currentMousePos;
        public int rectsize;
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
        public float closest_x = float.MaxValue;
        public float closest_y = float.MaxValue;
        public Brush redBrush = new SolidBrush(Color.Red);
        public Point bottomLeft;
        Point a;
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
        private DateTime _lastFpsUpdate = DateTime.Now;
        private int _lastFps = 0;
        public LoadingForm loadingForm;
        public AudioManager audioManager;
        public SmokeParticleManager smokeParticleManager;
        public Graphics g;

        public Form1()
        {
            g = CreateGraphics();
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml("#1E7CB8");
            audioManager = new AudioManager();
            rectsize = 200;
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
            backgroundMap = new Background(60, 60, this);
            grid = new Grid(60, 60, backgroundMap);
            necessitiesManager = new NecessitiesManager(grid);
            smokeParticleManager = new SmokeParticleManager(grid);
            nameProvider = new NameProvider("roadnames.json");
            edgePainter = new EdgePainter(grid, this, nameProvider, backgroundMap, g);
            buildingPainter = new BuildingPainter(grid, this, g);
            buttonManager = new InteractingObjectManager();
            List<EventHandler> allEventHandlers = new List<EventHandler>();
            
            allEventHandlers.Add(Form1_RoadButton);
            allEventHandlers.Add(Form1_ToggleNames);
            allEventHandlers.Add((sender, e) => Form1_BuildingBuilder(sender, e, buildingType));
            allEventHandlers.Add(Form1_ViewBuildingSpaces);
            allEventHandlers.Add(Form1_toggleGrid);
            allEventHandlers.Add(Form1_ChangeVolume);
            uiManager = new UIManager(zoomLevel, () => (this.ClientSize.Width, this.ClientSize.Height), grid, buttonManager, this, allEventHandlers);
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
            var elapsed = (now - _lastFpsUpdate).TotalSeconds;

            if (elapsed >= 1.0)
            {
                long count = Interlocked.Exchange(ref _frameCount, 0);
                _lastFps = (int)(count / elapsed);
                _lastFpsUpdate = now;
            }

            return _lastFps;
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

            foreach (Node n in grid.nodes)
            {
                n.IsNodeBuildable();
            }

            buildingPainter.buildingType = buildingType;
            backgroundMap.UpdateWaterAnimations();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.CreateGraphics();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            WindowState = FormWindowState.Maximized;
            this.Paint += Form1_Paint;
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

            if (viewGrid == true)
            {
                foreach (Node node in grid.nodes)
                {
                    g.DrawRectangle(bluePen, node.coords.X, node.coords.Y, rectsize, rectsize);
                }
            }

            buildingPainter.BuildingPaint(sender, g, currentMousePos);
            edgePainter.RoadPaint(sender, g, currentMousePos);
            smokeParticleManager.Draw(g);
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
            currentMousePos = Mouse_Pos(this, m);
            if (movingtiles && mousedown)
            {
                dx = Convert.ToInt32((currentMousePos.X - mouseXold) / zoomLevel);
                dy = Convert.ToInt32((currentMousePos.Y - mouseYold) / zoomLevel);

                foreach (Edge edge in grid.edges)
                {
                    edge.a.X += dx;
                    edge.a.Y += dy;
                    edge.b.X += dx;
                    edge.b.Y += dy;

                    foreach(IntersectingNode node in edge.intersections)
                    {
                        node.coords.X += dx;
                        node.coords.Y += dy;
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

                foreach (Building b in grid.buildings)
                {
                    b.coords.X += dx;
                    b.coords.Y += dy;
                }

                foreach (Node node in backgroundMap.tiles)
                {
                    node.coords.X += dx;
                    node.coords.Y += dy;
                }

                mouseXold = currentMousePos.X;
                mouseYold = currentMousePos.Y;
            }
        }

        private void Form1_MouseUp(object? sender, MouseEventArgs m)
        {
            if (m.Button == MouseButtons.Left)
            {
                if (movingtiles == true && mousedown == true)
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
            for (float dx = -strokeWidth; dx <= strokeWidth; dx += strokeWidth)
            {
                for (float dy = -strokeWidth; dy <= strokeWidth; dy += strokeWidth)
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