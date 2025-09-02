using CitySkylines0._5alphabeta;
using System.IO;

public class Background
{
    public List<Node> tiles;
    private Dictionary<Point, Node> nodeLookup;
    private Random random;
    private int width, height;
    private const float noiseScale = 0.11f;
    private const float landThreshold = 0.1f;
    private PerlinNoise perlinNoise;
    public Form1 Form1;
    private List<Image> grassImages;
    private Dictionary<Node, int> tileGrassImageIndex = new();
    private Dictionary<string, Image> sharedWaterImages = new();
    private Dictionary<Node, string> nodeWaterImageKey = new();
    private Dictionary<string, Image> grassEdgeImages = new();

    public Background(int width, int height, Form1 form1PassIn)
    {
        this.width = width;
        this.height = height;
        Form1 = form1PassIn;
        random = new Random();
        tiles = new List<Node>();
        perlinNoise = new PerlinNoise();
        LoadImages();
        GenerateMap();
        GenerateDetails();
    }

    private void LoadImages()
    {
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

                Point coords = new Point(x * 20, y * 20);
                Node node = new Node(coords.X, coords.Y, null, false, nodeNumber++, isLand);
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
            tileGrassImageIndex[node] = random.Next(grassImages.Count);

            if (!node.isGrass)
            {
                // Assign one of the water GIFs to this node
                int randomIndex = random.Next(1, 10);
                if (randomIndex == 1 && gifKeys.Count > 0) // 10% chance to assign a water1
                {
                    string chosenKey = gifKeys[0];
                    nodeWaterImageKey[node] = chosenKey;
                }
                else
                {
                    string chosenKey = gifKeys[1];
                    nodeWaterImageKey[node] = chosenKey;
                }

            }
        }
    }

    private void DrawEdge(string key, int x, int y, int tileSize, Graphics g)
    {
        if (grassEdgeImages.TryGetValue(key, out var img))
            g.DrawImage(img, x, y, tileSize, tileSize);
    }

    public void DrawMap(object? sender, Graphics g, float zoomLevel)
    {
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

        int tileSize = 20;

        for (int i = 0; i < tiles.Count; i++)
        {
            Node node = tiles[i];
            int gridX = i % width;
            int gridY = i / width;

            // Helper to check if a neighbor index is in bounds
            bool InBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;
            int idx(int x, int y) => y * width + x;

            // 8-way water checks (safe, based on list order)
            bool hasWaterN = InBounds(gridX, gridY - 1) && !tiles[idx(gridX, gridY - 1)].isGrass;
            bool hasWaterS = InBounds(gridX, gridY + 1) && !tiles[idx(gridX, gridY + 1)].isGrass;
            bool hasWaterE = InBounds(gridX + 1, gridY) && !tiles[idx(gridX + 1, gridY)].isGrass;
            bool hasWaterW = InBounds(gridX - 1, gridY) && !tiles[idx(gridX - 1, gridY)].isGrass;

            bool hasWaterNW = InBounds(gridX - 1, gridY - 1) && !tiles[idx(gridX - 1, gridY - 1)].isGrass;
            bool hasWaterNE = InBounds(gridX + 1, gridY - 1) && !tiles[idx(gridX + 1, gridY - 1)].isGrass;
            bool hasWaterSW = InBounds(gridX - 1, gridY + 1) && !tiles[idx(gridX - 1, gridY + 1)].isGrass;
            bool hasWaterSE = InBounds(gridX + 1, gridY + 1) && !tiles[idx(gridX + 1, gridY + 1)].isGrass;

            if (node.isGrass)
            {
                g.DrawImage(grassImages[tileGrassImageIndex[node]], node.coords.X, node.coords.Y, tileSize + 1, tileSize + 1);

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
            else if (nodeWaterImageKey.TryGetValue(node, out string gifKey))
            {
                // Draw water tile
                if (sharedWaterImages.TryGetValue(gifKey, out Image gifImage))
                {
                    g.DrawImage(gifImage, node.coords.X, node.coords.Y, tileSize, tileSize);
                }
            }
        }
        /*
         * debug info - uncomment to see node details on tiles
        foreach (Node node in tiles)
        {
            Font font2 = new Font("Comic Sans", 1);
            SolidBrush houseBrush = new SolidBrush(Color.Black);
            Point mousePos = node.coords;
            if (node.isNearRoad) { g.DrawString("NEAR ROAD", font2, houseBrush, mousePos.X, mousePos.Y + 3); }
            if (node.isGrass) { g.DrawString("GRASS", font2, houseBrush, mousePos.X, mousePos.Y + 8); }
            if (node.tiledata == null) { g.DrawString("NOT OCCUPIED", font2, houseBrush, mousePos.X, mousePos.Y + 13); }
            if (node.isBuildable) { g.DrawString("BUILDABLE", font2, houseBrush, mousePos.X, mousePos.Y + 18); }
        }*/
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
