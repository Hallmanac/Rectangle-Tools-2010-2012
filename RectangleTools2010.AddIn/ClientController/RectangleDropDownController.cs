using System;
using QubeItTools.Interfaces;
using Inventor;
using System.Windows.Forms;
using QubeItTools.ClientView;

namespace QubeItTools.ClientController
{
    /// <summary>
    /// This Class is used to be the control point for managing how the rectangle controls get instantiated in the add-in
    /// </summary>
    public class RectangleDropDownController : IRectangleController, IRectangleDropDownController
    {
        #region Member Variables
        //Variables representing responding to User Interface Events
        private UserInterfaceEvents userInterfaceEvents;
        private UserInterfaceEventsSink_OnResetRibbonInterfaceEventHandler uIEventsSink_OnResetRibbonInterfaceEventDelegate;
        
        private RibbonPanel partRectControlsPanel;
        private RibbonPanel assemblyRectControlPanel;
        private RibbonPanel drawingRectControlsPanel;

        private IInventorButton centerPointRectangleBtn;
        private IInventorButton vertMidPointRectangleBtn;
        private IInventorButton hzMidPointRectangleBtn;
        private IInventorButton diagonalCenterPointRectangleButton;
        private CommandControl partRectangleSplitButton;
        private CommandControl drawingRectangleSplitButton;
        private CommandControl assemblyRectangleSplitButton;
        private bool displayButtonPressed;
        #endregion

        #region Properties
        /// <summary>
        /// This property is used in the ButtonPressed property notification call back on the Horizontal and Vertical
        /// Mid-Point Rectangle buttons. They call the ChangeDisplayedControl method and pass in this property as 
        /// the argument. This was done because the last used control is persisted in the display after the command is 
        /// done and when the user clicks on the split button control a second time (thinking that the button being 
        /// displayed will be the one that gets fired), the default button is what gets fired even if a different 
        /// button is displayed.
        /// </summary>
        public ButtonDefinition DefaultDisplayControl {get; private set;}

        /// <summary>
        /// Property that gives access to the Rectangle Panel internal name.
        /// </summary>
        public string RectangleControlsPanelInternalName { get; private set; }

        /// <summary>
        /// When the user selects a sub-button in the split button control which isn't the "default" (or first one displayed),
        /// the visual effect of the button being pressed does not occur. To get around this problem, this property provides a
        /// mechanism to set the ButtonPressed property of the default displayed button.
        /// </summary>
        public bool DisplayButtonPressed
        {
            get { return displayButtonPressed; }
            set
            {
                displayButtonPressed = value;
                DefaultDisplayControl.Pressed = value;
            }
        }
        #endregion

        #region Constructor(s)

        public RectangleDropDownController()
        {
            RectangleControlsPanelInternalName = StandardAddInServer.AddInServerId + 
                                                 ":RectangleDropDownController";

            centerPointRectangleBtn = new CenterPointRectangleButton();
            vertMidPointRectangleBtn = new VertMidPointRectangleButton();
            hzMidPointRectangleBtn = new HzMidPointRectangleButton();
            diagonalCenterPointRectangleButton = new DiagonalCenterPointRectangleButton();

            DefaultDisplayControl = centerPointRectangleBtn.ButtonDefinition;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method used to instantiate the Rectangle buttons into the Ribbon UI. There are two ways 
        /// of achieving this: 
        /// 1) Creating a Ribbon Panel and putting the rectangle buttons there or
        /// 2) Adding the rectangle buttons to the existing Inventor Rectangle Split Button control
        /// </summary>
        public void CreateRibbonUserInterface()
        {
            try
            {
                CreateRectangleRibbonPanel();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Event handling method for the Reset Ribbon event
        /// </summary>
        /// <param name="Context"></param>
        private void UserInterfaceEvents_OnResetRibbonInterface(NameValueMap Context)
        {
            CreateRectangleRibbonPanel();
        }

        /// <summary>
        /// This method gets called to add a new panel to the ribbon to be the container for 
        /// the rectangle buttons 
        /// </summary>
        private void CreateRectangleRibbonPanel()
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
            partRectControlsPanel = partSketchTabPanels.Add("Rectangles", RectangleControlsPanelInternalName, 
                                    StandardAddInServer.AddInServerId,"id_PanelP_2DSketchConstrain", true);
            drawingRectControlsPanel = drawingSketchTabPanels.Add("Rectangles", RectangleControlsPanelInternalName, 
                                       StandardAddInServer.AddInServerId, "id_PanelD_2DSketchConstrain", true);
            assemblyRectControlPanel = assemblySketchTabPanels.Add("Rectangles", RectangleControlsPanelInternalName, 
                                       StandardAddInServer.AddInServerId, "id_PanelA_2DSketchConstrain", true);

            //Create an Inventor Object collection that will hold the buttons for the split control that gets added to
            //the rectangles panel.  To add a split button control you have to pass in a collection.
            ObjectCollection objectCollection = StandardAddInServer.InventorApplication.TransientObjects.CreateObjectCollection();

            //Add the rectangle buttons to the collection
            objectCollection.Add(centerPointRectangleBtn.ButtonDefinition);
            objectCollection.Add(diagonalCenterPointRectangleButton.ButtonDefinition);
            objectCollection.Add(hzMidPointRectangleBtn.ButtonDefinition);
            objectCollection.Add(vertMidPointRectangleBtn.ButtonDefinition);

            //Add the split button control to the rectangles panel
            partRectangleSplitButton =  
                partRectControlsPanel.CommandControls.AddSplitButton
                (DisplayedControl: DefaultDisplayControl,
                ButtonDefinitions: objectCollection, UseLargeIcon: true);
            drawingRectangleSplitButton = 
                drawingRectControlsPanel.CommandControls.AddSplitButton
                (DisplayedControl: DefaultDisplayControl,
                ButtonDefinitions: objectCollection, UseLargeIcon: true);
            assemblyRectangleSplitButton = 
                assemblyRectControlPanel.CommandControls.AddSplitButton
                (DisplayedControl: DefaultDisplayControl,
                ButtonDefinitions: objectCollection, UseLargeIcon: true);
        }

        /// <summary>
        /// This method changes the button that is being displayed on the rectangle control panel.
        /// </summary>
        /// <param name="buttonDefinition"></param>
        public void ChangeDisplayedControl(ButtonDefinition buttonDefinition)
        {
            if (StandardAddInServer.InventorApplication.ActiveDocumentType == DocumentTypeEnum.kPartDocumentObject)
            {
                partRectangleSplitButton.DisplayedControl = buttonDefinition as ControlDefinition;
            }
            else if (StandardAddInServer.InventorApplication.ActiveDocumentType == DocumentTypeEnum.kDrawingDocumentObject)
            {
                drawingRectangleSplitButton.DisplayedControl = buttonDefinition as ControlDefinition;
            }
            else if (StandardAddInServer.InventorApplication.ActiveDocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
            {
                assemblyRectangleSplitButton.DisplayedControl = buttonDefinition as ControlDefinition;
            }
        }

        /// <summary>
        /// Removes the rectangle tools from Inventor.
        /// </summary>
        public void Deactivate()
        {
            DeactivateButtons();
            partRectControlsPanel = null;
            drawingRectControlsPanel = null;
            assemblyRectControlPanel = null;
        }

        /// <summary>
        /// Calls the buttons' deactivate methods
        /// </summary>
        private void DeactivateButtons()
        {
            centerPointRectangleBtn.Deactivate();
            diagonalCenterPointRectangleButton.Deactivate();
            vertMidPointRectangleBtn.Deactivate();
            hzMidPointRectangleBtn.Deactivate();
        }
        #endregion
    }
}
