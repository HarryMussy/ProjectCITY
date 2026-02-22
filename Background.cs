using CitySkylines0._5alphabeta;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Xml.Linq;

public class Background
{
    [JsonIgnore] private Dictionary<Point, Node> nodeLookup;
    [JsonIgnore] private Random random;
    [JsonIgnore] private PerlinNoise perlinNoise;
    [JsonIgnore] public Form1 Form1;
    [JsonIgnore] private List<Image> grassImages;
    [JsonIgnore] private Dictionary<string, Image> sharedWaterImages;
    [JsonIgnore] private Dictionary<string, Image> grassEdgeImages;
    [JsonIgnore] private float noiseScale = 0.05F;
    [JsonIgnore] private float landThreshold = 0.25F;
    [JsonIgnore] Dictionary<string, Image> imageLookup;
    [JsonIgnore] private ImageAttributes? seasonalAttributes;

    private Dictionary<string, ColorMatrix> seasonalMatrices = new Dictionary<string, ColorMatrix>();
    private string? lastSeason;
    private ImageAttributes? cachedAttributes;

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
        PrecomputeSeasonalColors();
        GenerateMap();
        GenerateDetails();
    }

    public void PrecomputeSeasonalColors()
    {
        seasonalMatrices["Winter"] = WinterMatrix();
        seasonalMatrices["Spring"] = SpringMatrix();
        seasonalMatrices["Summer"] = SummerMatrix();
        seasonalMatrices["Autumn"] = AutumnMatrix();
    }

    private ColorMatrix LerpColorMatrix(ColorMatrix from, ColorMatrix to, float t)
    {
        float[][] result = new float[5][];
        for (int i = 0; i < 5; i++)
        {
            result[i] = new float[5];
            for (int j = 0; j < 5; j++)
            {
                result[i][j] = from[i, j] + (to[i, j] - from[i, j]) * t;
            }
        }
        return new ColorMatrix(result);
    }

    private ColorMatrix WinterMatrix()
    {
        return new ColorMatrix(new float[][]
        {
        new float[]{0.6f, 0.6f, 0.7f, 0, 0}, // R
        new float[]{0.6f, 0.6f, 0.7f, 0, 0}, // G
        new float[]{0.6f, 0.6f, 0.8f, 0, 0}, // B
        new float[]{0,    0,    0,   1, 0}, // Alpha
        new float[]{0.1f, 0.1f, 0.2f, 0, 1}  // Offset (slight cool shadow)
        });
    }

    private ColorMatrix SpringMatrix()
    {
        return new ColorMatrix(new float[][]
        {
            new float[]{0f,0f,0f,0,0},
            new float[]{0f,0f,0f,0,0},
            new float[]{0f,0f,0f,0,0},
            new float[]{0,0,0,0,0},
            new float[]{0,0,0,0,0}
        });
    }

    private ColorMatrix SummerMatrix()
    {
        return new ColorMatrix(new float[][]
        {
            new float[]{0f,0f,0f,0,0},
            new float[]{0f,0f,0f,0,0},
            new float[]{0f,0f,0f,0,0},
            new float[]{0,0,0,0,0},
            new float[]{0,0,0,0,0}
        });
    }

    private ColorMatrix AutumnMatrix()
    {
        return new ColorMatrix(new float[][]
        {
            new float[]{1.2f,0.1f,0,0,0},
            new float[]{0.3f,0.8f,0,0,0},
            new float[]{0.1f,0.2f,0.6f,0,0},
            new float[]{0,0,0,1,0},
            new float[]{0,0,0,0,1}
        });
    }

    private void LoadImages()
    {
        grassImages = new List<Image>();
        sharedWaterImages = new Dictionary<string, Image>();
        grassEdgeImages = new Dictionary<string, Image>();
        imageLookup = new Dictionary<string, Image>();

        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));

        string grassFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Grass", "GrassVar");
        grassImages = new List<Image>();

        foreach (string path in Directory.GetFiles(grassFolder, "*.png"))
        {
            using var original = Image.FromFile(path);
            Bitmap transparentBitmap = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(transparentBitmap))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(original, 0, 0, original.Width, original.Height);
            }
            grassImages.Add(transparentBitmap);
        }

        string waterFolder = Path.Combine(projectRoot,"gameAssets", "gameArt", "Water");
        foreach (string path in Directory.GetFiles(waterFolder, "*.gif"))
        {
            string fileName = Path.GetFileName(path);
            Image gifImage = Image.FromFile(path);
            ImageAnimator.Animate(gifImage, null); // Register for animation
            sharedWaterImages[fileName] = gifImage;
        }

        // Load grass edge images
        string edgeFolder = Path.Combine(projectRoot, "gameAssets", "gameArt", "Grass", "GrassEdges");
        foreach (string path in Directory.GetFiles(edgeFolder, "*.png"))
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            grassEdgeImages[fileName] = Image.FromFile(path);
        }

        for (int i = 0; i < grassImages.Count; i++)
        {
            imageLookup["grass" + i] = grassImages[i];
        }

        foreach (var kv in sharedWaterImages)
        {
            imageLookup[kv.Key] = kv.Value;
        }
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

    public void GenerateDetails()
    {
        var gifKeys = sharedWaterImages.Keys.ToList();

        foreach (Node node in tiles)
        {
            node.imageKey = "grass" + random.Next(grassImages.Count);

            if (!node.isGrass)
            {
                //assign one of the water GIFs to this node
                int randomIndex = random.Next(1, 10);
                if (randomIndex == 1 && gifKeys.Count > 0) //10% chance to assign a water1
                {
                    string chosenKey = gifKeys[0];
                    node.imageKey = chosenKey;
                }
                else
                {
                    string chosenKey = gifKeys[1];
                    node.imageKey = chosenKey;
                }

            }
        }
    }

    private void DrawEdge(string key, int x, int y, int tileSize, Graphics g)
    {
        // Draw a water tile underneath
        Image waterTile = imageLookup["water2.gif"]; // or choose random water GIF if you want animation
        g.DrawImage(waterTile, x, y, tileSize + 1, tileSize + 1);

        // Draw the grass edge on top
        if (grassEdgeImages.TryGetValue(key, out var edgeImg))
        {
            if (seasonalAttributes != null)
            {
                Rectangle destRect = new Rectangle(x, y, tileSize + 1, tileSize + 1);
                g.DrawImage(edgeImg, destRect, 0, 0, edgeImg.Width, edgeImg.Height, GraphicsUnit.Pixel, seasonalAttributes);
            }
            else
            {
                g.DrawImage(edgeImg, x, y, tileSize + 1, tileSize + 1);
            }
        }
    }

    public void DrawMap(object? sender, Graphics g, float zoomLevel)
    {
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

        int tileSize = rectSize;

        for (int i = 0; i < tiles.Count; i++)
        {
            Node node = tiles[i];
            int gridX = i % width;
            int gridY = i / width;

            //helper to check if a neighbor index is in bounds
            bool InBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;
            int idx(int x, int y) => y * width + x;

            //8-way water checks (safe, based on list order): treat out-of-bounds neighbors as water
            bool hasWaterN = !InBounds(gridX, gridY - 1) || !tiles[idx(gridX, gridY - 1)].isGrass;
            bool hasWaterS = !InBounds(gridX, gridY + 1) || !tiles[idx(gridX, gridY + 1)].isGrass;
            bool hasWaterE = !InBounds(gridX + 1, gridY) || !tiles[idx(gridX + 1, gridY)].isGrass;
            bool hasWaterW = !InBounds(gridX - 1, gridY) || !tiles[idx(gridX - 1, gridY)].isGrass;

            bool hasWaterNW = !InBounds(gridX - 1, gridY - 1) || !tiles[idx(gridX - 1, gridY - 1)].isGrass;
            bool hasWaterNE = !InBounds(gridX + 1, gridY - 1) || !tiles[idx(gridX + 1, gridY - 1)].isGrass;
            bool hasWaterSW = !InBounds(gridX - 1, gridY + 1) || !tiles[idx(gridX - 1, gridY + 1)].isGrass;
            bool hasWaterSE = !InBounds(gridX + 1, gridY + 1) || !tiles[idx(gridX + 1, gridY + 1)].isGrass;

            if (node.isGrass)
            {
                Image img = imageLookup[node.imageKey];

                if (seasonalAttributes != null)
                {
                    Rectangle destRect = new Rectangle(node.coords.X, node.coords.Y, tileSize + 1, tileSize + 1);
                    g.DrawImage(img, destRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, seasonalAttributes);
                }
                else
                {
                    g.DrawImage(img, node.coords.X, node.coords.Y, tileSize + 1, tileSize + 1);
                }

                //determine edges based on surrounding tiles
                int x = node.coords.X;
                int y = node.coords.Y;
                //edge tiles
                if (hasWaterN && !hasWaterS && !hasWaterE && !hasWaterW) DrawEdge("GrassEdge_E", x, y, tileSize, g);
                if (hasWaterS && !hasWaterN && !hasWaterE && !hasWaterW) DrawEdge("GrassEdge_W", x, y, tileSize, g);
                if (hasWaterE && !hasWaterW && !hasWaterN && !hasWaterS) DrawEdge("GrassEdge_N", x, y, tileSize, g);
                if (hasWaterW && !hasWaterE && !hasWaterN && !hasWaterS) DrawEdge("GrassEdge_S", x, y, tileSize, g);

                //outer corners
                if (hasWaterN && hasWaterW && hasWaterNW) DrawEdge("GrassEdge_Outer_SE", x, y, tileSize, g);
                if (hasWaterN && hasWaterE && hasWaterNE) DrawEdge("GrassEdge_Outer_NE", x, y, tileSize, g);
                if (hasWaterS && hasWaterW && hasWaterSW) DrawEdge("GrassEdge_Outer_SW", x, y, tileSize, g);
                if (hasWaterS && hasWaterE && hasWaterSE) DrawEdge("GrassEdge_Outer_NW", x, y, tileSize, g);

                //inner corners
                if (hasWaterNW && !hasWaterN && !hasWaterW) DrawEdge("GrassEdge_Inner_SE", x, y, tileSize, g);
                if (hasWaterNE && !hasWaterN && !hasWaterE) DrawEdge("GrassEdge_Inner_NE", x, y, tileSize, g);
                if (hasWaterSW && !hasWaterS && !hasWaterW) DrawEdge("GrassEdge_Inner_SW", x, y, tileSize, g);
                if (hasWaterSE && !hasWaterS && !hasWaterE) DrawEdge("GrassEdge_Inner_NW", x, y, tileSize, g);
            }
            else
            {
                g.DrawImage(imageLookup[node.imageKey], node.coords.X, node.coords.Y, tileSize + 1, tileSize + 1);
            }
        }

    }
    public void UpdateSeasonalAttributes()
    {
        string currentSeason = Form1.calendar.CurrentSeason;
        string nextSeason = Form1.calendar.GetNextSeason(currentSeason);

        // Determine fade factor (0-1) based on week of month, e.g., last 7 days fade
        float t = Form1.calendar.GetSeasonTransitionFactor(); // implement in Calendar

        ColorMatrix currentMatrix = seasonalMatrices[currentSeason];
        ColorMatrix nextMatrix = seasonalMatrices[nextSeason];

        cachedAttributes = new ImageAttributes();
        cachedAttributes.SetColorMatrix(LerpColorMatrix(currentMatrix, nextMatrix, t));
        seasonalAttributes = cachedAttributes;
    }

    public List<Node> GetBuildableNodes()
    {
        return tiles.Where(n => n.isNearRoad).ToList();
    }

    public string GetTileType(Point point)
    {
        if (nodeLookup.TryGetValue(point, out var node))
            return node.isGrass ? "Land" : "Water";
        return "Water";
    }

    // Optional helper to update animated GIFs from Form1
    public void UpdateWaterAnimations()
    {
        foreach (var img in sharedWaterImages.Values)
        {
            ImageAnimator.UpdateFrames(img);
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
