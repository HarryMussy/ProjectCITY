using CitySkylines0._5alphabeta;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.Xml.Linq;

public class Background
{
    [JsonIgnore] private Dictionary<Point, Node> nodeLookup;
    [JsonIgnore] private Random random;
    [JsonIgnore] private PerlinNoise perlinNoise;
    [JsonIgnore] public Form1 Form1;
    [JsonIgnore] private Dictionary<string, List<string>> seasonalGrassImages;
    [JsonIgnore] private List<string> waterImages;
    [JsonIgnore] private Dictionary<string, Dictionary<string, string>> seasonalGrassEdgeImages;
    [JsonIgnore] private float noiseScale = 0.05F;
    [JsonIgnore] private float landThreshold = 0.25F;

    private Bitmap? currentSeasonMap;
    private Bitmap? nextSeasonMap;

    private string? cachedSeason;
    private string? cachedNextSeason;
    
    // Image cache: path -> Image to avoid reloading every frame
    [JsonIgnore] private static Dictionary<string, Image> imageCache = new();
    [JsonIgnore] private bool isInitialized = false;

    public int difficulty { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int rectSize { get; set; }
    public List<Node> tiles { get; set; }

    public Background() { LoadImages(); }

    public Background(int width, int height, Form1 form1PassIn, int rectSizeIn, int difficulty)
    {
        this.width = width;
        this.height = height;
        this.rectSize = rectSizeIn;
        Form1 = form1PassIn;
        this.difficulty = difficulty;

        random = new Random();
        tiles = new List<Node>();
        perlinNoise = new PerlinNoise();

        if (difficulty == 1) 
        {
            noiseScale = 0.05F;
            landThreshold = 0.25F;
        }
        if (difficulty == 2)
        {
            noiseScale = 0.05F;
            landThreshold = 0.15F;
        }
        if (difficulty == 3)
        {
            noiseScale = 0.05F;
            landThreshold = 0.05F;
        }

        LoadImages();
        GenerateMap();
        GenerateDetails();
    }

    private void DrawEdgesForTile(Graphics g, Rectangle destRect, string season, bool hasWaterN, bool hasWaterS, bool hasWaterE, bool hasWaterW, bool hasWaterNW, bool hasWaterNE, bool hasWaterSW, bool hasWaterSE)
    {
        if (hasWaterN && !hasWaterS && !hasWaterE && !hasWaterW) DrawEdge("GrassEdge_E", g, destRect, season);
        if (hasWaterS && !hasWaterN && !hasWaterE && !hasWaterW) DrawEdge("GrassEdge_W", g, destRect, season);
        if (hasWaterE && !hasWaterW && !hasWaterN && !hasWaterS) DrawEdge("GrassEdge_N", g, destRect, season);
        if (hasWaterW && !hasWaterE && !hasWaterN && !hasWaterS) DrawEdge("GrassEdge_S", g, destRect, season);

        if (hasWaterN && hasWaterW && hasWaterNW) DrawEdge("GrassEdge_Outer_SE", g, destRect, season);
        if (hasWaterN && hasWaterE && hasWaterNE) DrawEdge("GrassEdge_Outer_NE", g, destRect, season);
        if (hasWaterS && hasWaterW && hasWaterSW) DrawEdge("GrassEdge_Outer_SW", g, destRect, season);
        if (hasWaterS && hasWaterE && hasWaterSE) DrawEdge("GrassEdge_Outer_NW", g, destRect, season);

        if (hasWaterNW && !hasWaterN && !hasWaterW) DrawEdge("GrassEdge_Inner_SE", g, destRect, season);
        if (hasWaterNE && !hasWaterN && !hasWaterE) DrawEdge("GrassEdge_Inner_NE", g, destRect, season);
        if (hasWaterSW && !hasWaterS && !hasWaterW) DrawEdge("GrassEdge_Inner_SW", g, destRect, season);
        if (hasWaterSE && !hasWaterS && !hasWaterE) DrawEdge("GrassEdge_Inner_NW", g, destRect, season);
    }

    public void GenerateMap()
    {
        int nodeNumber = 0;
        nodeLookup = new Dictionary<Point, Node>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = perlinNoise.Generate(x * noiseScale, y * noiseScale);
                bool isLand = noiseValue < landThreshold;
                Point coords = new Point(x * rectSize, y * rectSize);
                Node node = new Node(new Point(coords.X, coords.Y), false, false, nodeNumber++, false, isLand);
                tiles.Add(node);
                nodeLookup[coords] = node;
            }
        }
    }
    private void LoadImages()
    {
        waterImages = new List<string>();
        seasonalGrassImages = new Dictionary<string, List<string>>();
        seasonalGrassEdgeImages = new Dictionary<string, Dictionary<string, string>>();

        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        string[] seasons = { "Spring", "Summer", "Autumn", "Winter" };

        foreach(string season in seasons)
        {
            if (!seasonalGrassImages.ContainsKey(season))
            {
                seasonalGrassImages[season] = new List<string>();
            }
            if (!seasonalGrassEdgeImages.ContainsKey(season))
            {
                seasonalGrassEdgeImages[season] = new Dictionary<string, string>();
            }   
        }

        foreach (var season in seasons)
        {
            //grass variants
            string grassFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Grass", season, "GrassVar");

            var grassList = new List<Image>();

            if (Directory.Exists(grassFolder))
            {
                foreach (string path in Directory.GetFiles(grassFolder, "*.png"))
                {
                    seasonalGrassImages[season].Add(path);
                }
            }

            //edge variants
            string edgeFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Grass", season, "GrassEdges");
            var edgeDict = new Dictionary<string, Image>();
            if (Directory.Exists(edgeFolder))
            {
                foreach (string path in Directory.GetFiles(edgeFolder, "*.png"))
                {
                    string key = Path.GetFileNameWithoutExtension(path);
                    seasonalGrassEdgeImages[season][key] = path;
                }
            }
        }

        //water variants
        string waterFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Water");
        if (Directory.Exists(waterFolder))
        {
            foreach (string path in Directory.GetFiles(waterFolder, "*.gif"))
            {
                waterImages.Add(path);
            }
        }
    }

    public void GenerateDetails()
    {
        string[] seasons = { "Spring", "Summer", "Autumn", "Winter" };

        foreach (Node node in tiles)
        {
            if (!node.isGrass)
            {
                // assign a single random water image to all seasons
                string waterPath = waterImages[random.Next(waterImages.Count)];
                foreach (var season in seasons)
                {
                    node.seasonalImagePaths[season] = waterPath;
                }
            }
            else
            {
                // assign a random grass image for each season
                foreach (var season in seasons)
                {
                    var grassList = seasonalGrassImages[season];
                    if (grassList.Count > 0)
                    {
                        string grassPath = grassList[random.Next(grassList.Count)];
                        node.seasonalImagePaths[season] = grassPath;
                    }
                }
            }
        }
    }

    private Image GetCachedImage(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;

        if (!imageCache.TryGetValue(path, out var cachedImg))
        {
            cachedImg = Image.FromFile(path);
            imageCache[path] = cachedImg;
        }
        return cachedImg;
    }

    private void DrawEdge(string key, Graphics g, Rectangle destRect, string season)
    {
        g.DrawImage(Image.FromFile(waterImages[0]), destRect);
        if (seasonalGrassEdgeImages.TryGetValue(season, out var dict) && dict.TryGetValue(key, out var path))
        {
            var img = GetCachedImage(path);
            if (img != null) { g.DrawImage(img, destRect); }
        }
    }

    private Bitmap BuildSeasonBitmap(string season)
    {
        Bitmap map = new Bitmap(width * rectSize, height * rectSize);

        using Graphics g = Graphics.FromImage(map);

        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

        int tileSize = rectSize;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                Node node = tiles[i];

                int drawX = node.coords.X;
                int drawY = node.coords.Y;

                Rectangle destRect = new Rectangle(drawX, drawY, tileSize + 1, tileSize + 1);

                bool InBounds(int ix, int iy) => ix >= 0 && ix < width && iy >= 0 && iy < height;
                int idx(int ix, int iy) => iy * width + ix;

                bool hasWaterN = !InBounds(x, y - 1) || !tiles[idx(x, y - 1)].isGrass;
                bool hasWaterS = !InBounds(x, y + 1) || !tiles[idx(x, y + 1)].isGrass;
                bool hasWaterE = !InBounds(x + 1, y) || !tiles[idx(x + 1, y)].isGrass;
                bool hasWaterW = !InBounds(x - 1, y) || !tiles[idx(x - 1, y)].isGrass;

                bool hasWaterNW = !InBounds(x - 1, y - 1) || !tiles[idx(x - 1, y - 1)].isGrass;
                bool hasWaterNE = !InBounds(x + 1, y - 1) || !tiles[idx(x + 1, y - 1)].isGrass;
                bool hasWaterSW = !InBounds(x - 1, y + 1) || !tiles[idx(x - 1, y + 1)].isGrass;
                bool hasWaterSE = !InBounds(x + 1, y + 1) || !tiles[idx(x + 1, y + 1)].isGrass;

                if (node.isGrass)
                {
                    if (node.seasonalImagePaths.TryGetValue(season, out string filePath))
                    {
                        var img = GetCachedImage(filePath);
                        if (img != null)
                            g.DrawImage(img, destRect);
                    }
                    DrawEdgesForTile(g, destRect, season, hasWaterN, hasWaterS, hasWaterE, hasWaterW, hasWaterNW, hasWaterNE, hasWaterSW, hasWaterSE);
                }
                else
                {
                    if (node.seasonalImagePaths.TryGetValue(season, out string filePath))
                    {
                        var img = GetCachedImage(filePath);
                        if (img != null)
                            g.DrawImage(img, destRect);
                    }
                }
            }
        }

        return map;
    }

    public void DrawMap(object? sender, Graphics g, float zoomLevel)
    {
        if (Form1 == null) { return; }
        string currentSeason = Form1.calendar.GetCurrentSeason(Form1.calendar.month);
        string nextSeason = Form1.calendar.GetCurrentSeason(Form1.calendar.month + 1);
        float t = Form1.calendar.GetSeasonTransitionFactor();

        //rebuild if season changed
        if (currentSeasonMap == null || cachedSeason != currentSeason)
        {
            currentSeasonMap?.Dispose();
            currentSeasonMap = BuildSeasonBitmap(currentSeason);
            cachedSeason = currentSeason;
        }

        if (nextSeasonMap == null || cachedNextSeason != nextSeason)
        {
            nextSeasonMap?.Dispose();
            nextSeasonMap = BuildSeasonBitmap(nextSeason);
            cachedNextSeason = nextSeason;
        }

        if (currentSeasonMap == null) { return; }

        //force sharp rendering
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

        Rectangle destRect = new Rectangle(0, 0, currentSeasonMap.Width, currentSeasonMap.Height);

        //draw base season
        g.DrawImage(currentSeasonMap, destRect);

        //crossfade next season
        if (t > 0f && nextSeasonMap != null)
        {
            using ImageAttributes fadeAttr = new ImageAttributes();
            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix33 = t;
            fadeAttr.SetColorMatrix(matrix);
            g.DrawImage(nextSeasonMap, destRect, 0, 0, nextSeasonMap.Width, nextSeasonMap.Height, GraphicsUnit.Pixel, fadeAttr);
        }
    }
}

public class PerlinNoise
{
    private Random random;
    private static int[] randomTable = new int[512];

    public PerlinNoise()
    {
        random = new Random();
        for (int i = 0; i < 512; i++)
        {
            randomTable[i] = random.Next(0, 255);
        }
    }

    public float Generate(float x, float y)
    {
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;

        float fx = x - (int)x;
        float fy = y - (int)y;

        float u = Fade(fx);
        float v = Fade(fy);

        int aa = randomTable[X + randomTable[Y]];
        int ab = randomTable[X + randomTable[Y + 1]];
        int ba = randomTable[X + 1 + randomTable[Y]];
        int bb = randomTable[X + 1 + randomTable[Y + 1]];

        float x1 = Lerp(Grad(aa, fx, fy), Grad(ba, fx - 1, fy), u);
        float x2 = Lerp(Grad(ab, fx, fy - 1), Grad(bb, fx - 1, fy - 1), u);

        return Lerp(x1, x2, v);
    }

    private float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    private float Lerp(float a, float b, float t) => a + t * (b - a);
    private float Grad(int hash, float x, float y)
    {
        int h = hash & 15;
        float u = h < 8 ? x : y;
        return ((h & 4) == 0 ? -u : u);
    }
}
