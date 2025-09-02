using System.Drawing.Imaging;
using System.IO;

namespace CitySkylines0._5alphabeta
{
    public class SmokeParticle : IDisposable
    {
        public PointF Position;
        public float Opacity;
        public float Size;
        public float Speed; // movement speed
        public float LifeTime; // in seconds
        public float Age; // how long it has lived
        private int currentFrame = 0;
        private int frameCount;
        private int[] frameDurations;  // duration per frame in milliseconds
        private int elapsedFrameTime = 0;  // time passed since last frame change in ms

        private static readonly Random random = new Random();

        private Image smokeGif;
        private bool isAnimating;

        public SmokeParticle(PointF position, Image smokeGif)
        {
            Position = position;
            Opacity = 1.0f;
            Size = 10f + (float)(random.NextDouble() * 200);
            Speed = 2f + (float)(random.Next(-5, 5) * 5);
            LifeTime = 2f + (float)(random.Next(0, 2) * 2);
            Age = 0f;

            this.smokeGif = smokeGif;
            if (smokeGif != null)
            {
                frameCount = smokeGif.GetFrameCount(FrameDimension.Time);
                frameDurations = GetFrameDurations(smokeGif);
                currentFrame = 0;
                smokeGif.SelectActiveFrame(FrameDimension.Time, currentFrame);
            }
        }
        public void Dispose()
        {

        }

        private int[] GetFrameDurations(Image gif)
        {
            var durations = new int[frameCount];
            var timesProperty = gif.GetPropertyItem(0x5100); // Frame delay property

            for (int i = 0; i < frameCount; i++)
            {
                // Each delay is stored in 4 bytes as 1/100th seconds
                durations[i] = (timesProperty.Value[i * 4] + (timesProperty.Value[i * 4 + 1] << 8)) * 20; // convert to ms
                if (durations[i] == 0) durations[i] = 100; // fallback to 100ms if zero
            }
            return durations;
        }

        public bool Update(float deltaTime)
        {
            Age += deltaTime;
            if (Age >= LifeTime)
            {
                Opacity = 0;
                return false;
            }

            // Move up
            Position = new PointF(Position.X + (Speed * deltaTime * 3), Position.Y + (Speed * deltaTime));

            // Fade out gradually
            Opacity = 1f - (Age / LifeTime);

            // Grow or shrink as you wish (this is per-particle, not global)
            Size -= deltaTime * 2;

            // Frame update logic (per-particle, not global)
            if (smokeGif != null && frameCount > 1)
            {
                elapsedFrameTime += (int)(deltaTime * 1000); // ms

                while (elapsedFrameTime >= frameDurations[currentFrame])
                {
                    elapsedFrameTime -= frameDurations[currentFrame];
                    currentFrame++;
                    if (currentFrame >= frameCount)
                        currentFrame = frameCount - 1; // Stay on last frame
                    smokeGif.SelectActiveFrame(FrameDimension.Time, currentFrame);
                }
            }

            return true;
        }

        public void Draw(Graphics g)
        {
            if (Opacity <= 0 || smokeGif == null)
                return;

            var colorMatrix = new ColorMatrix
            {
                Matrix33 = Opacity // Alpha channel only
            };

            using var imgAttr = new ImageAttributes();
            imgAttr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            float halfSize = Size / 2f;
            RectangleF destRect = new RectangleF(Position.X - halfSize, Position.Y - halfSize, Size, Size);

            // Draw the entire image scaled to destRect
            g.DrawImage(
                smokeGif,
                Rectangle.Round(destRect),                // Destination rectangle
                0, 0,                                     // Source rectangle X, Y (top-left of the image)
                smokeGif.Width, smokeGif.Height,         // Source width and height
                GraphicsUnit.Pixel,
                imgAttr
            );
        }

    }

    public class SmokeParticleManager
    {
        private readonly Grid grid;
        private readonly Random random = new();
        public List<SmokeParticle> particles = new();
        private DateTime lastChecked;

        private readonly HashSet<Edge> edgesWithSmoke = new();
        private readonly HashSet<House> buildingsWithSmoke = new();

        // Store loaded smoke GIFs
        private List<Image> smokeGifs = new();

        public SmokeParticleManager(Grid grid)
        {
            this.grid = grid;
            LoadSmokeGifs();
            lastChecked = DateTime.Now;
        }

        private void LoadSmokeGifs()
        {
            string smokeFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gameArt", "Smoke");
            if (!Directory.Exists(smokeFolder))
                return;

            foreach (string file in Directory.GetFiles(smokeFolder, "*.gif"))
            {
                try
                {
                    Image gif = Image.FromFile(file);
                    ImageAnimator.Animate(gif, null); // Register for animation
                    smokeGifs.Add(gif);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load smoke gif {file}: {ex.Message}");
                }
            }
        }

        public void Update()
        {
            var now = DateTime.Now;
            var deltaTime = (float)(now - lastChecked).TotalSeconds;

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                if (!particles[i].Update(deltaTime))
                {
                    particles[i].Dispose();    // Dispose before removing
                    particles.RemoveAt(i);
                }
            }

            lastChecked = now;
        }

        public void Draw(Graphics g)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Draw(g);
            }
        }

        public void SpawnSmokeOnNewEdgesAndBuildings(List<Edge> newEdges, List<House> newBuildings)
        {
            foreach (var edge in newEdges)
            {
                if (!edgesWithSmoke.Contains(edge))
                {
                    edgesWithSmoke.Add(edge);
                    SpawnParticlesOnEdge(edge);
                }
            }

            foreach (var building in newBuildings)
            {
                if (!buildingsWithSmoke.Contains(building))
                {
                    buildingsWithSmoke.Add(building);
                    SpawnParticlesOnBuilding(building);
                }
            }
        }

        private void SpawnParticlesOnEdge(Edge edge)
        {
            int count = random.Next(5, 25);
            int pointsCount = edge.pointsOnTheEdge?.Count ?? 1;
            int spawnCount = (int)(count / (1f / pointsCount + 1f));

            for (int i = 0; i < spawnCount; i++)
            {
                float t = spawnCount > 1 ? i / (float)(spawnCount - 1) : 0.5f;
                float x = edge.a.X + t * (edge.b.X - edge.a.X);
                float y = edge.a.Y + t * (edge.b.Y - edge.a.Y);

                // Pick a random smoke GIF for this particle
                Image smokeGif = smokeGifs.Count > 0 ? smokeGifs[random.Next(smokeGifs.Count)] : null;

                if (smokeGif != null)
                    particles.Add(new SmokeParticle(new PointF(x, y), smokeGif));
            }
        }

        private void SpawnParticlesOnBuilding(House building)
        {
            var pos = new PointF(building.coords.X + building.size.Width / 2f, building.coords.Y + building.size.Height + 10);

            Image smokeGif = smokeGifs.Count > 0 ? smokeGifs[random.Next(smokeGifs.Count)] : null;

            if (smokeGif != null)
                particles.Add(new SmokeParticle(pos, smokeGif));
        }
    }

}
