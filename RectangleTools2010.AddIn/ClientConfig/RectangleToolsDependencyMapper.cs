using System;
using System.Windows.Forms;
using Inventor;
using InventorEvents2010;
using InventorEvents2010.Interfaces;
using QubeItTools.ClientController;
using QubeItTools.ClientView;
using QubeItTools.Interfaces;
using QubeItTools.General;

namespace QubeItTools.ClientConfig
{
    /// <summary>
    /// This Class is used to be the control point for managing how the rectangle controls get 
    /// instantiated in the add-in
    /// </summary>
    public class RectangleToolsDependencyMapper : IClientConfig                                                        
    {
        #region Member Variables

        private readonly IRectangleController rectangleController;
        private readonly IUserInterfaceEventsLib userInterfaceEvents;
        private readonly IClientSettings clientSettings;
        private readonly IRectangleCongigButton rectangleCongigButton;
        #endregion

        #region Properties

        
       
        #endregion

        #region Constructor(s)

        public RectangleToolsDependencyMapper()
        {
            clientSettings = new ClientSettings();
            rectangleController = GetRectangleUiController();
            rectangleCongigButton = new RectangleConfigButton();
            userInterfaceEvents = new UserInterfaceEventsLib(StandardAddInServer.InventorApplication);
            userInterfaceEvents.OnResetRibbonInterfaceDelegate += UserInterfaceEvents_OnResetRibbonInterface;
            userInterfaceEvents.UserInterfaceEvents.OnResetRibbonInterface += userInterfaceEvents
                                                                              .OnResetRibbonInterfaceDelegate;
        }

        private IRectangleController GetRectangleUiController()
        {
            IRectangleController rectangleControllerReturned;

            switch(clientSettings.CurrentRectangleInterface)
            {
                case RectangleInterfaceStyle.Panel:
                    rectangleControllerReturned = new RectanglePanelController();
                    break;
                case RectangleInterfaceStyle.DropDown:
                    rectangleControllerReturned = new RectangleDropDownController();
                    break;
                default:
                    MessageBox.Show("The \"GetRectangleController\" method didn't work. Line 61(ish) inside RectangleToolsDependanceyMapper");
                    rectangleControllerReturned = new RectanglePanelController();
                    break;
            }
            return rectangleControllerReturned;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method used to instantiate the Rectangle buttons into the Ribbon UI. There are two ways 
        /// of achieving this: 
        /// 1) Creating a Ribbon Panel and putting the rectangle buttons there or
        /// 2) Adding the rectangle buttons to the existing Inventor Rectangle Split Button control
        /// </summary>
        public void InitializeUserInterface()
        {
            try
            {
                rectangleController.CreateRibbonUserInterface();
                rectangleCongigButton.AddRibbonInterface();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Removes the rectangle tools from Inventor.
        /// </summary>
        public void Deactivate()
        {
            rectangleController.Deactivate();
        }

        /// <summary>
        /// Event handling method for the Reset Ribbon event
        /// </summary>
        /// <param name="context"></param>
        private void UserInterfaceEvents_OnResetRibbonInterface(NameValueMap context)
        {
            InitializeUserInterface();
        }
        #endregion
    }
}
