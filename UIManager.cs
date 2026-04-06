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
    private bool buttonsCreated = false; // flag to avoid recreating buttons every frame
    private float displayWellbeing = 100f; // smoothed display value for the wellbeing bar


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
        var (windowWidth, windowHeight) = getDimensions();

        Color uiColor = Color.FromArgb(200, Color.CadetBlue);
        Brush uiBrush = new SolidBrush(uiColor);
        g.FillRectangle(uiBrush, zoomedBottomLeftX, zoomedBottomLeftY, zoomedWidth, zoomedHeight);

        Font font = new Font("Segoe UI", 11, FontStyle.Bold);
        Brush innerBrush = new SolidBrush(Color.White);
        Brush outerBrush = new SolidBrush(Color.FromArgb(60, 60, 60));

        int padding = 20;
        int lineSpacing = 20;

        //left column - city stats
        int leftColumnX = padding;
        int statsY = (int)zoomedBottomLeftY + 10;

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

        //centre - current tool label
        string doing = "Currently: " + form.allOperations[form.currentOperation];
        int centerX = (int)(windowWidth / 2 - 100);
        int centerY = (int)(zoomedBottomLeftY + zoomedHeight - 30);

        form.AddStrokeToText(sender, g, doing, 2, font, outerBrush, new Point(centerX, centerY - 100));
        g.DrawString(doing, font, innerBrush, centerX, centerY - 100);

        //right side - smoothed wellbeing bar with lerp to avoid jumpy display
        float actualWellbeing = form.populationManager.AverageWellBeing;
        displayWellbeing += (actualWellbeing - displayWellbeing) * 0.08f; //lerp towards actual value
        displayWellbeing = Math.Clamp(displayWellbeing, 0f, 100f);

        int barWidth = 220;
        int barHeight = 18;

        int wellnessX = (int)(windowWidth - barWidth - padding);
        int wellnessY = (int)(zoomedBottomLeftY + 25);

        string wellText = "City Wellbeing: " + displayWellbeing.ToString("F0") + "%";

        form.AddStrokeToText(sender, g, wellText, 2, font, outerBrush, new Point(wellnessX, wellnessY - 22));
        g.DrawString(wellText, font, innerBrush, wellnessX, wellnessY - 22);

        int filledWidth = (int)(barWidth * (displayWellbeing / 100f));

        //colour the bar based on wellbeing thresholds
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

        //display the top 3 most common unmet desires below the wellbeing bar
        int desireY = wellnessY + barHeight + 6;

        foreach (var desire in form.populationManager.GlobalDesires.OrderByDescending(d => d.Value).Take(3))
        {
            string desireText = desire.Key + ": " + desire.Value;

            form.AddStrokeToText(sender, g, desireText, 1, font, outerBrush, new Point(wellnessX, desireY));
            g.DrawString(desireText, font, innerBrush, wellnessX, desireY);

            desireY += 18;
        }

        //buttons are only created once and recreated by UpdateUI when the window is resized
        if (!buttonsCreated)
        {
            string projectRoot = AppContext.BaseDirectory;

            int buttonSize = 48;
            int spacing = 12;

            //calculate start X so the row of buttons is horizontally centred
            int totalButtons = 9;
            int totalWidth = (totalButtons * buttonSize) + ((totalButtons - 1) * spacing);

            int startX = (int)(windowWidth / 2 - totalWidth / 2);
            int buttonY = (int)zoomedBottomLeftY + 45;

            int x = startX;

            interactingObjectManager.CreateButton(new Point(x, buttonY), new Size(buttonSize, buttonSize), form, 6,
                Image.FromFile(Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Houses", "house1.png")))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "house");
            x += buttonSize + spacing;

            interactingObjectManager.CreateButton(new Point(x, buttonY), new Size(buttonSize, buttonSize), form, 6,
                Image.FromFile(Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "powerPlant.png")))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "powerplant");
            x += buttonSize + spacing;

            interactingObjectManager.CreateButton(new Point(x, buttonY), new Size(buttonSize, buttonSize), form, 6,
                Image.FromFile(Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "waterPump.png")))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "waterpump");
            x += buttonSize + spacing;

            interactingObjectManager.CreateButton(new Point(x, buttonY), new Size(buttonSize, buttonSize), form, 6,
                Image.FromFile(Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "hospital.png")))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "hospital");
            x += buttonSize + spacing;

            interactingObjectManager.CreateButton(new Point(x, buttonY), new Size(buttonSize, buttonSize), form, 6,
                Image.FromFile(Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Shops", "shop2.png")))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "shop");
            x += buttonSize + spacing;

            interactingObjectManager.CreateButton(new Point(x, buttonY), new Size(buttonSize, buttonSize), form, 6,
                Image.FromFile(Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "Factories", "factory1.png")))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "factory");
            x += buttonSize + spacing;

            interactingObjectManager.CreateButton(new Point(x, buttonY), new Size(buttonSize, buttonSize), form, 6,
                Image.FromFile(Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "police.png")))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "policebuilding");
            x += buttonSize + spacing;

            interactingObjectManager.CreateButton(new Point(x, buttonY), new Size(buttonSize, buttonSize), form, 6,
                Image.FromFile(Path.Combine(projectRoot, "gameAssets", "gameArt", "Buildings", "fireservicebuilding.png")))
                .Click += (s, e) => form.Form1_BuildingBuilder(s, e, "fireservice");
            x += buttonSize + spacing;

            interactingObjectManager.CreateButton(new Point(x, buttonY), new Size(buttonSize, buttonSize), form, 6,
                Image.FromFile(Path.Combine(projectRoot, "gameAssets", "gameArt", "bulldozer.png")))
                .Click += (s, e) => bulldozingButtonClickHandler(s, e);

            //second row of smaller utility toggle buttons
            int smallButtonWidth = 100;
            int smallSpacing = 10;
            int smallY = buttonY + 60;

            int smallTotalWidth = (5 * smallButtonWidth) + (4 * smallSpacing);
            int smallStartX = (int)(windowWidth / 2 - smallTotalWidth / 2);

            int sx = smallStartX;

            interactingObjectManager.CreateButton("ROAD", new Point(sx, smallY), new Size(smallButtonWidth, 25), form, 10)
                .Click += roadButtonClickHandler;
            sx += smallButtonWidth + smallSpacing;

            interactingObjectManager.CreateButton("ROAD NAME", new Point(sx, smallY), new Size(smallButtonWidth, 25), form, 6)
                .Click += toggleNamesClickHandler;
            sx += smallButtonWidth + smallSpacing;

            interactingObjectManager.CreateButton("VALID BUILD SPACE", new Point(sx, smallY), new Size(smallButtonWidth, 25), form, 6)
                .Click += viewBuildingSpaceClickHandler;
            sx += smallButtonWidth + smallSpacing;

            interactingObjectManager.CreateButton("GRID VIEW", new Point(sx, smallY), new Size(smallButtonWidth, 25), form, 6)
                .Click += toggleGridViewClickHandler;
            sx += smallButtonWidth + smallSpacing;

            interactingObjectManager.CreateButton("OPTIONS", new Point(sx, smallY), new Size(smallButtonWidth, 25), form, 6)
                .Click += (s, e) => new OptionsForm(true, form.audioManager, form).ShowDialog();

            buttonsCreated = true;
        }
    }

    //recalculates UI dimensions and forces buttons to be recreated at the new positions
    public void UpdateUI()
    {
        var (windowWidth, windowHeight) = getDimensions();
        zoomedWidth = windowWidth * zoomLevel;
        zoomedHeight = 140 * zoomLevel;
        zoomedBottomLeftX = (windowWidth * zoomLevel) - zoomedWidth;
        zoomedBottomLeftY = (windowHeight * zoomLevel) - zoomedHeight;
        interactingObjectManager.RemoveButtons();
        buttonsCreated = false;
    }


}