using CitySkylines0._5alphabeta;
using System.IO;

public class UIManager
{
    private float zoomLevel;
    private readonly Func<(float, float)> getDimensions;
    private float zoomedWidth;
    private float zoomedHeight;
    private float zoomedBottomLeftX;
    private float zoomedBottomLeftY;
    private Grid map;
    private InteractingObjectManager interactingObjectManager;
    private Calendar calendar;
    private Form1 form;
    private EventHandler roadButtonClickHandler;
    private EventHandler toggleNamesClickHandler;
    private EventHandler buildingButtonClickHandler;
    private EventHandler viewBuildingSpaceClickHandler;
    private EventHandler toggleGridViewClickHandler;
    private EventHandler volSlider;
    private EventHandler bulldozingButtonClickHandler;
    private bool buttonsCreated = false; // Flag to track if buttons are already created
    private float displayWellbeing = 100f;


    public UIManager(float zoomLevel, Func<(float, float)> getDimensions, Grid map, InteractingObjectManager buttonManager, Form1 form, List<EventHandler> allEventHandlers, Calendar calendarIn)
    {
        this.zoomLevel = zoomLevel;
        this.getDimensions = getDimensions;
        this.map = map;
        this.interactingObjectManager = buttonManager;
        this.form = form;
        roadButtonClickHandler = allEventHandlers[0];
        toggleNamesClickHandler = allEventHandlers[1];
        buildingButtonClickHandler = allEventHandlers[2];
        viewBuildingSpaceClickHandler = allEventHandlers[3];
        toggleGridViewClickHandler = allEventHandlers[4];
        volSlider = allEventHandlers[5];
        bulldozingButtonClickHandler = allEventHandlers[6];
        zoomedBottomLeftX = 0;
        zoomedBottomLeftY = 0;
        calendar = calendarIn;
    }

    public void ConstructUI(object? sender, Graphics g)
    {
        Color uiColor = Color.FromArgb(200, Color.CadetBlue);
        Brush uiBrush = new SolidBrush(uiColor);
        g.FillRectangle(uiBrush, zoomedBottomLeftX, zoomedBottomLeftY, zoomedWidth, zoomedHeight);

        string totalcash = "Musbux: " + map.cash.ToString("F2");
        Font font = new Font("Segoe UI", 11, FontStyle.Bold);
        Brush innerBrush = new SolidBrush(Color.White);
        Brush outerBrush = new SolidBrush(Color.FromArgb(60, 60, 60));

        SizeF textSize = g.MeasureString(totalcash, font);
        int strokeWidth = 3;

        string doing = form.allOperations[form.currentOperation];

        int fps = form.fps;

        //UI

        int padding = 20;
        int leftColumnX = (int)zoomedBottomLeftX + padding;
        int topRowY = (int)zoomedBottomLeftY + 10;
        int lineSpacing = 20;

        //city stats
        int statsY = topRowY;

        string cashText = "Musbux: " + map.cash.ToString("F0");
        string popText = "Population: " + form.populationManager.Population.Count;
        string energyText = "Power: " + form.necessitiesManager.globalPowerStatus;
        string waterText = "Water: " + form.necessitiesManager.globalWaterStatus;
        string fpsText = "FPS: " + form.fps;
        string dateText = calendar.date + "  " + calendar.time;

        string[] leftStats = { cashText, popText, energyText, waterText, fpsText, dateText };

        foreach (string stat in leftStats)
        {
            form.AddStrokeToText(sender, g, stat, 2, font, outerBrush, new Point(leftColumnX, statsY));
            g.DrawString(stat, font, innerBrush, leftColumnX, statsY);
            statsY += lineSpacing;
        }

        //currently doing
        doing = "Currently: " + form.allOperations[form.currentOperation];

        int centerX = (int)(zoomedWidth / 2 - 100);
        int centerY = (int)(zoomedBottomLeftY + zoomedHeight - 30);

        form.AddStrokeToText(sender, g, doing, 2, font, outerBrush, new Point(centerX, centerY - 100));
        g.DrawString(doing, font, innerBrush, centerX, centerY - 100);

        //wellbeing/ wellness

        float actualWellbeing = form.populationManager.AverageWellBeing;

        // Smooth animation (lerp)
        displayWellbeing += (actualWellbeing - displayWellbeing) * 0.08f;

        // Clamp just in case
        displayWellbeing = Math.Clamp(displayWellbeing, 0f, 100f);

        int barWidth = 220;
        int barHeight = 18;

        int wellnessX = (int)(zoomedWidth - barWidth - padding);
        int wellnessY = (int)(zoomedBottomLeftY + 25);

        string wellText = "City Wellbeing: " + displayWellbeing.ToString("F0") + "%";

        form.AddStrokeToText(sender, g, wellText, 2, font, outerBrush, new Point(wellnessX, wellnessY - 22));
        g.DrawString(wellText, font, innerBrush, wellnessX, wellnessY - 22);

        int filledWidth = (int)(barWidth * (displayWellbeing / 100f));

        Color barColor = Color.LimeGreen;
        if (displayWellbeing < 60) barColor = Color.Gold;
        if (displayWellbeing < 40) barColor = Color.OrangeRed;
        if (displayWellbeing < 20) barColor = Color.DarkRed;

        using (Brush backBrush = new SolidBrush(Color.FromArgb(70, 40, 40, 40)))
        using (Brush fillBrush = new SolidBrush(barColor))
        {
            g.FillRectangle(backBrush, wellnessX, wellnessY, barWidth, barHeight);
            g.FillRectangle(fillBrush, wellnessX, wellnessY, filledWidth, barHeight);
        }

        // Top 3 unmet desires
        int desireY = wellnessY + barHeight + 6;

        foreach (var desire in form.populationManager.GlobalDesires.OrderByDescending(d => d.Value).Take(3))
        {
            string desireText = desire.Key + ": " + desire.Value;

            form.AddStrokeToText(sender, g, desireText, 1, font, outerBrush, new Point(wellnessX, desireY));
            g.DrawString(desireText, font, innerBrush, wellnessX, desireY);

            desireY += 18;
        }

        if (!buttonsCreated)
        {
            interactingObjectManager.CreateButton("ROAD", new Point((int)zoomedBottomLeftX + 310, (int)zoomedBottomLeftY + 100), new Size(70, 25), form, 10).Click += roadButtonClickHandler;
            interactingObjectManager.CreateButton("ROAD NAME", new Point((int)zoomedBottomLeftX + 380, (int)zoomedBottomLeftY + 100), new Size(70, 25), form, 6).Click += toggleNamesClickHandler;

            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string pathToHouseImage = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Houses", "house1.png");
            Image i = Image.FromFile(pathToHouseImage);
            interactingObjectManager.CreateButton(new Point((int)zoomedBottomLeftX + 310, (int)zoomedBottomLeftY + 45), new Size(48, 48), form, 6, Image.FromFile(pathToHouseImage))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "house");


            string pathToTurbineImage = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "powerPlant.png");
            interactingObjectManager.CreateButton(new Point((int)zoomedBottomLeftX + 370, (int)zoomedBottomLeftY + 45), new Size(48, 48), form, 6, Image.FromFile(pathToTurbineImage))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "powerplant");

            interactingObjectManager.CreateButton("OPTIONS", new Point((int)zoomedWidth - 100, (int)zoomedBottomLeftY + 100), new Size(70, 25), form, 6).Click += (s, e) =>
            {
                new OptionsForm(true, form.audioManager, form).ShowDialog();
            };

            string pathToBulldozerImage = Path.Combine(projectRoot, "gameAssets", "gameArt", "bulldozer.png");
            interactingObjectManager.CreateButton(new Point((int)zoomedBottomLeftX + 790, (int)zoomedBottomLeftY + 45), new Size(48, 48), form, 6, Image.FromFile(pathToBulldozerImage))
                .Click += (s, e) => bulldozingButtonClickHandler(s, e);

            string pathToPumpImage = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "waterPump.png");
            interactingObjectManager.CreateButton(new Point((int)zoomedBottomLeftX + 430, (int)zoomedBottomLeftY + 45), new Size(48, 48), form, 6, Image.FromFile(pathToPumpImage))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "waterpump");

            string pathToHospitalImage = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "hospital.png");
            interactingObjectManager.CreateButton(new Point((int)zoomedBottomLeftX + 490, (int)zoomedBottomLeftY + 45), new Size(48, 48), form, 6, Image.FromFile(pathToHospitalImage))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "hospital");

            string pathToShopImage = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Shops", "shop2.png");
            interactingObjectManager.CreateButton(new Point((int)zoomedBottomLeftX + 550, (int)zoomedBottomLeftY + 45), new Size(48, 48), form, 6, Image.FromFile(pathToShopImage))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "shop");

            string pathToFactoryImage = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Factories", "factory1.png");
            interactingObjectManager.CreateButton(new Point((int)zoomedBottomLeftX + 610, (int)zoomedBottomLeftY + 45), new Size(48, 48), form, 6, Image.FromFile(pathToFactoryImage))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "factory");

            string pathToPoliceImage = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "police.png");
            interactingObjectManager.CreateButton(new Point((int)zoomedBottomLeftX + 670, (int)zoomedBottomLeftY + 45), new Size(48, 48), form, 6, Image.FromFile(pathToPoliceImage))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "policebuilding");

            string pathToFireImage = Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "fireservicebuilding.png");
            interactingObjectManager.CreateButton(new Point((int)zoomedBottomLeftX + 730, (int)zoomedBottomLeftY + 45), new Size(48, 48), form, 6, Image.FromFile(pathToFireImage))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "fireservice");

            interactingObjectManager.CreateButton("VALID BUILD SPACE", new Point((int)zoomedBottomLeftX + 450, (int)zoomedBottomLeftY + 100), new Size(70, 25), form, 6).Click += viewBuildingSpaceClickHandler;
            interactingObjectManager.CreateButton("GRID VIEW", new Point((int)zoomedBottomLeftX + 520, (int)zoomedBottomLeftY + 100), new Size(70, 25), form, 6).Click += toggleGridViewClickHandler;
            //interactingObjectManager.CreateSlider("VOLUME", new Point((int)zoomedBottomLeftX + 580, (int)zoomedBottomLeftY + 30), new Size(200, 25), form, 6).ValueChanged += volSlider;
            buttonsCreated = true;
        }
    }

    public void UpdateUI()
    {
        var (windowWidth, windowHeight) = getDimensions();
        zoomedWidth = windowWidth * zoomLevel;
        zoomedHeight = 140 * zoomLevel;
        zoomedBottomLeftX = (windowWidth * zoomLevel) - zoomedWidth;
        zoomedBottomLeftY = (windowHeight * zoomLevel) - zoomedHeight;
        interactingObjectManager.RemoveButtons();
        buttonsCreated = false; // Reset button creation flag
    }


}
