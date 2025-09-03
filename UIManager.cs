using CitySkylines0._5alphabeta;
using NAudio.Gui;

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
    private Form1 form;
    private EventHandler roadButtonClickHandler;
    private EventHandler toggleNamesClickHandler;
    private EventHandler houseBuildingClickHandler;
    private EventHandler viewBuildingSpaceClickHandler;
    private EventHandler toggleGridViewClickHandler;
    private EventHandler volSlider;
    private bool buttonsCreated = false; // Flag to track if buttons are already created


    public UIManager(float zoomLevel, Func<(float, float)> getDimensions, Grid map, InteractingObjectManager buttonManager, Form1 form, List<EventHandler> allEventHandlers)
    {
        this.zoomLevel = zoomLevel;
        this.getDimensions = getDimensions;
        this.map = map;
        this.interactingObjectManager = buttonManager;
        this.form = form;
        roadButtonClickHandler = allEventHandlers[0];
        toggleNamesClickHandler = allEventHandlers[1];
        houseBuildingClickHandler = allEventHandlers[2];
        viewBuildingSpaceClickHandler = allEventHandlers[3];
        toggleGridViewClickHandler = allEventHandlers[4];
        volSlider = allEventHandlers[5];
        zoomedBottomLeftX = 0;
        zoomedBottomLeftY = 0;
    }

    public void ConstructUI(object? sender, Graphics g)
    {
        Color uiColor = Color.FromArgb(200, Color.CadetBlue);
        Brush uiBrush = new SolidBrush(uiColor);
        g.FillRectangle(uiBrush, zoomedBottomLeftX, zoomedBottomLeftY, zoomedWidth, zoomedHeight);

        string totalcash = "Musbux: " + map.cash.ToString("F2");
        Font font = new Font("Comic Sans", 11);
        Brush whiteBrush = new SolidBrush(Color.White);
        Brush blackBrush = new SolidBrush(Color.Black);

        SizeF textSize = g.MeasureString(totalcash, font);
        int strokeWidth = 1;

        string doing = form.allOperations[form.currentOperation];

        int fps = form.fps;
        form.AddStrokeToText(sender, g, totalcash, strokeWidth, font, blackBrush, new Point(20, 20));
        form.AddStrokeToText(sender, g, "Currently doing: " + doing, strokeWidth, font, blackBrush, new Point((int)zoomedBottomLeftX + 300, (int)zoomedBottomLeftY + 10));
        form.AddStrokeToText(sender, g, "-------------VOLUME-------------", strokeWidth, font, blackBrush, new Point((int)zoomedBottomLeftX + 580, (int)zoomedBottomLeftY + 10));
        form.AddStrokeToText(sender, g, "FPS: " + Convert.ToString(fps), strokeWidth, font, blackBrush, new Point(20,0));
        form.AddStrokeToText(sender, g, "Energy Demand: " + form.necessitiesManager.globalElectricityStatus, strokeWidth, font, blackBrush, new Point(20, 40));
        form.AddStrokeToText(sender, g, "Water Demand: " + form.necessitiesManager.globalWaterStatus, strokeWidth, font, blackBrush, new Point(20, 60));

        g.DrawString("Energy Demand: " + form.necessitiesManager.globalElectricityStatus, font, whiteBrush, 20, 40);
        g.DrawString("Water Demand: " + form.necessitiesManager.globalWaterStatus, font, whiteBrush, 20, 60);
        g.DrawString(totalcash, font, whiteBrush, 20,20);
        g.DrawString("Currently doing: " + doing, font, whiteBrush, zoomedBottomLeftX + 300, zoomedBottomLeftY + 10);
        g.DrawString("-------------VOLUME-------------", font, whiteBrush, zoomedBottomLeftX + 580, zoomedBottomLeftY + 10);
        g.DrawString("FPS: " + Convert.ToString(fps), font, whiteBrush, 20, 0);

        if (!buttonsCreated)
        {
            interactingObjectManager.CreateButton("ROAD", new Point((int)zoomedBottomLeftX + 10, (int)zoomedBottomLeftY + 30), new Size(70, 25), form, 10).Click += roadButtonClickHandler;
            interactingObjectManager.CreateButton("ROAD NAME", new Point((int)zoomedBottomLeftX + 10, (int)zoomedBottomLeftY + 65), new Size(70, 25), form, 6).Click += toggleNamesClickHandler;
            interactingObjectManager.CreateButton("HOUSE", new Point((int)zoomedBottomLeftX + 80, (int)zoomedBottomLeftY + 30), new Size(70, 25), form, 6).Click += houseBuildingClickHandler;
            interactingObjectManager.CreateButton("VALID BUILD SPACE", new Point((int)zoomedBottomLeftX + 80, (int)zoomedBottomLeftY + 65), new Size(70, 25), form, 6).Click += viewBuildingSpaceClickHandler;
            interactingObjectManager.CreateButton("GRID VIEW", new Point((int)zoomedBottomLeftX + 150, (int)zoomedBottomLeftY + 30), new Size(70, 25), form, 6).Click += toggleGridViewClickHandler;
            interactingObjectManager.CreateSlider("VOLUME", new Point((int)zoomedBottomLeftX + 580, (int)zoomedBottomLeftY + 30), new Size(200, 25), form, 6).ValueChanged += volSlider;
            buttonsCreated = true;
        }
    }

    public void UpdateUI()
    {
        var (windowWidth, windowHeight) = getDimensions();
        zoomedWidth = windowWidth * zoomLevel;
        zoomedHeight = 100 * zoomLevel;
        zoomedBottomLeftX = (windowWidth * zoomLevel) - zoomedWidth;
        zoomedBottomLeftY = (windowHeight * zoomLevel) - zoomedHeight;
        interactingObjectManager.RemoveButtons();
        buttonsCreated = false; // Reset button creation flag
    }

    
}
