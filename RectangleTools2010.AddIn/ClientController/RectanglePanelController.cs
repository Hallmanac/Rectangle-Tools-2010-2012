using Inventor;
using QubeItTools.ClientConfig;
using QubeItTools.ClientView;
using QubeItTools.Interfaces;

namespace QubeItTools.ClientController
{
    public class RectanglePanelController : IRectangleController
    {
        private readonly IInventorButton centerPointRectangleBtn;
        private readonly IInventorButton vertMidPointRectangleBtn;
        private readonly IInventorButton hzMidPointRectangleBtn;
        private readonly IInventorButton diagonalCenterPointRectangleButton;

        /// <summary>
        /// Property that gives access to the Rectangle Panel internal name.
        /// </summary>
        public string RectangleControlsPanelInternalName { get; private set; }

        //ctor
        public RectanglePanelController()
        {
            centerPointRectangleBtn = new CenterPointRectangleButton();
            vertMidPointRectangleBtn = new VertMidPointRectangleButton();
            hzMidPointRectangleBtn = new HzMidPointRectangleButton();
            diagonalCenterPointRectangleButton = new DiagonalCenterPointRectangleButton();

            RectangleControlsPanelInternalName = StandardAddInServer.AddInServerId + 
                "RectanglePanelControlManager";
        }

        /// <summary>
        /// This method gets called to add a new panel to the ribbon to be the container for 
        /// the rectangle buttons 
        /// </summary>
        public void CreateRibbonUserInterface()
        {
            //create a reference to the ribbon
            Ribbon partRibbon = StandardAddInServer.InventorApplication.UserInterfaceManager.Ribbons["Part"];
            Ribbon drawingRibbon = StandardAddInServer.InventorApplication.UserInterfaceManager.Ribbons["Drawing"];
            Ribbon assemblyRibbon = StandardAddInServer.InventorApplication.UserInterfaceManager.Ribbons["Assembly"];

            //create a reference to the tab you want to add a control to
            RibbonTab partSketchTab = partRibbon.RibbonTabs["id_TabSketch"];
            RibbonTab drawingSketchTab = drawingRibbon.RibbonTabs["id_TabSketch"];
            RibbonTab assemblySketchTab = assemblyRibbon.RibbonTabs["id_TabSketch"];

            //reference the panels collection in a variable so that you can call the add method in a clean fashion
            RibbonPanels partSketchTabPanels = partSketchTab.RibbonPanels;
            RibbonPanels drawingSketchTabPanels = drawingSketchTab.RibbonPanels;
            RibbonPanels assemblySketchTabPanels = assemblySketchTab.RibbonPanels;

            //Add your panel to the panels collection
            RibbonPanel partRectControlsPanel = partSketchTabPanels.Add("Rectangles",
                RectangleControlsPanelInternalName, StandardAddInServer.AddInServerId, 
                "id_PanelP_2DSketchConstrain", true);
            RibbonPanel drawingRectControlsPanel = drawingSketchTabPanels.Add("Rectangles",
                RectangleControlsPanelInternalName, StandardAddInServer.AddInServerId, 
                "id_PanelD_2DSketchConstrain", true);
            RibbonPanel assemblyRectControlPanel = assemblySketchTabPanels.Add("Rectangles",
                RectangleControlsPanelInternalName, StandardAddInServer.AddInServerId, 
                "id_PanelA_2DSketchConstrain", true);

            //Add the Center Point Rectangle Button to the ribbon panels
            CommandControl partCentPtRecBtn = partRectControlsPanel.CommandControls.AddButton
                (ButtonDefinition: this.centerPointRectangleBtn.ButtonDefinition,
                UseLargeIcon: true);
            CommandControl dwgCentPtRecBtn = drawingRectControlsPanel.CommandControls.AddButton
                (ButtonDefinition: this.centerPointRectangleBtn.ButtonDefinition,
                UseLargeIcon: true);
            CommandControl assyCentPtRecBtn = assemblyRectControlPanel.CommandControls.AddButton
                (ButtonDefinition: this.centerPointRectangleBtn.ButtonDefinition,
                UseLargeIcon: true);

            //Add the Diagonal Center Point Rectangle Button to the ribbon panels
            CommandControl parttDiagCentPtRecBtn = partRectControlsPanel.CommandControls.AddButton
                (ButtonDefinition: this.diagonalCenterPointRectangleButton.ButtonDefinition,
                UseLargeIcon: true);
            CommandControl dwgDiagCentPtRecBtn = drawingRectControlsPanel.CommandControls.AddButton
                (ButtonDefinition: this.diagonalCenterPointRectangleButton.ButtonDefinition,
                UseLargeIcon: true);
            CommandControl assyDiagCentPtRecBtn = assemblyRectControlPanel.CommandControls.AddButton
                (ButtonDefinition: this.diagonalCenterPointRectangleButton.ButtonDefinition,
                UseLargeIcon: true);

            //Add Horizontal Mid-Point Rectangle Button to the ribbon panels
            CommandControl ptHzMPtRectBtn = partRectControlsPanel.CommandControls.AddButton
                (ButtonDefinition: this.hzMidPointRectangleBtn.ButtonDefinition,
                UseLargeIcon: true);
            CommandControl dwgHzMPtRectBtn = drawingRectControlsPanel.CommandControls.AddButton
                (ButtonDefinition: this.hzMidPointRectangleBtn.ButtonDefinition,
                UseLargeIcon: true);
            CommandControl assyHzMPtRectBtn = assemblyRectControlPanel.CommandControls.AddButton
                (ButtonDefinition: this.hzMidPointRectangleBtn.ButtonDefinition,
                UseLargeIcon: true);

            //Add Vertical Mid-Point Rectangle Button to the ribbon panels
            CommandControl ptVertMPtRectBtn = partRectControlsPanel.CommandControls.AddButton
                (ButtonDefinition: this.vertMidPointRectangleBtn.ButtonDefinition,
                UseLargeIcon: true);
            CommandControl dwgVertMPtRectBtn = drawingRectControlsPanel.CommandControls.AddButton
                (ButtonDefinition: this.vertMidPointRectangleBtn.ButtonDefinition,
                UseLargeIcon: true);
            CommandControl assyVertMPtRectBtn = assemblyRectControlPanel.CommandControls.AddButton
                (ButtonDefinition: this.vertMidPointRectangleBtn.ButtonDefinition,
                UseLargeIcon: true);
        }

        /// <summary>
        /// Removes the rectangle tools from Inventor.
        /// </summary>
        public void Deactivate()
        {
            centerPointRectangleBtn.Deactivate();
            diagonalCenterPointRectangleButton.Deactivate();
            vertMidPointRectangleBtn.Deactivate();
            hzMidPointRectangleBtn.Deactivate();
        }
    }
}
