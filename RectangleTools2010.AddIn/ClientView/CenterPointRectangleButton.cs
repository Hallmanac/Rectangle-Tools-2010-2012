using System;
using System.Drawing;
using System.Windows.Forms;
using Inventor;
using InventorEvents2010;
using Microsoft.VisualBasic.Compatibility.VB6;
using QubeItTools.ClientModel;
using QubeItTools.Interfaces;
using QubeItTools.Properties;


namespace QubeItTools.ClientView
{
    public class CenterPointRectangleButton : ClientRectangleButton, IInventorButton
    {
        #region Constructor
        /// <summary>
        /// Constructor for the CenterPointRectangleButton. 
        /// </summary>
        /// <param name="inventorApplication"></param>
        /// <param name="displayName"></param>
        /// <param name="internalName"></param>
        /// <param name="commandType"></param>
        /// <param name="clientID"></param>
        /// <param name="description"></param>
        /// <param name="toolTip"></param>
        /// <param name="standardIcon"></param>
        /// <param name="largeIcon"></param>
        /// <param name="buttonDisplayType"></param>
        public CenterPointRectangleButton()
        {
            invApplication = StandardAddInServer.InventorApplication;
            ClientButtonInternalName = StandardAddInServer.AddInServerId + "CenterPointRectangleButton";
            clientRectangleLogicInstance = new CenterPointRectangle(this, invApplication);

            try
            {
                CreateButtonDefinition();
                buttonEventsLibrary = new ButtonEventsLib(ButtonDefinition);

                buttonEventsLibrary.OnExecuteDelegate += ClientRectangleButtonDefinition_OnExecute;
                buttonEventsLibrary.ButtonDef.OnExecute += buttonEventsLibrary.OnExecuteDelegate;
            }
            catch (Exception e)
            {
                MessageBox.Show(Resources.CenterPointRectangleButtonButtonFailedToInitialize 
                    + e.ToString());
            }
        }
        #endregion

        void CreateButtonDefinition()
        {
            var currentAssembly = System.Reflection.Assembly
                .GetExecutingAssembly();
            var centerPtRectangleButtonPath = currentAssembly
                .GetManifestResourceStream
                ("QubeItTools.Resources.CenterPointRectangle.ico");

            if (centerPtRectangleButtonPath != null)
            {
                var centerPointRectangleIcon = new Icon(centerPtRectangleButtonPath);
                var largeCenterPointRectangleIcon = new Icon(centerPointRectangleIcon, largeIconSize,
                                                             largeIconSize);
                var standardCenterPointRectangleIcon = new Icon(centerPointRectangleIcon,
                                                                standardIconSize, standardIconSize);

                var largeIconPictureDisp =
                    (stdole.IPictureDisp)Support.IconToIPicture(largeCenterPointRectangleIcon);
                var standardIconPictureDisp =
                    (stdole.IPictureDisp)Support.IconToIPicture(standardCenterPointRectangleIcon);

                ButtonDefinition = invApplication.CommandManager.ControlDefinitions.
                    AddButtonDefinition("C.P.R.", ClientButtonInternalName,
                                        CommandTypesEnum.kShapeEditCmdType, StandardAddInServer.AddInServerId,
                                        "Create Center Point Rectangle", 
                                        "Creates a Center Point rectangle around the origin of where the user picks.", 
                                        standardIconPictureDisp, largeIconPictureDisp, 
                                        ButtonDisplayEnum.kDisplayTextInLearningMode);
            }

            ButtonDefinition.Enabled = true;
            buttonPressed = ButtonDefinition.Pressed;
            CommandIsRunning = false;

        }
    }
}
