using System;
using System.Drawing;
using System.Windows.Forms;
using Inventor;
using InventorEvents2010;
using Microsoft.VisualBasic.Compatibility.VB6;
using QubeItTools.ClientConfig;
using QubeItTools.ClientModel;
using QubeItTools.Interfaces;

namespace QubeItTools.ClientView
{
    class VertMidPointRectangleButton : ClientRectangleButton, IInventorButton
    {
        #region Constructor
        /// <summary>
        /// Constructor for the VerticalMidPointRectangle.
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
        public VertMidPointRectangleButton()
        {
            invApplication = StandardAddInServer.InventorApplication;
            ClientButtonInternalName = StandardAddInServer.AddInServerId
                + "VerticalMidPointRectangleButton";
            clientRectangleLogicInstance = new VertMidPointRectangle(this, StandardAddInServer.InventorApplication);

            try
            {
                CreateButtonDefinition();
                buttonEventsLibrary = new ButtonEventsLib(ButtonDefinition);

                buttonEventsLibrary.OnExecuteDelegate += ClientRectangleButtonDefinition_OnExecute;
                buttonEventsLibrary.ButtonDef.OnExecute += buttonEventsLibrary.OnExecuteDelegate;
            }
            catch(Exception e)
            {
                MessageBox.Show("Vertical Mid-Point Rectangle button failed to initialize.\n\n"
                    + e.ToString());
            }
        }
        #endregion

        void CreateButtonDefinition()
        {
            System.Reflection.Assembly currentAssembly = System.Reflection.Assembly
                .GetExecutingAssembly();
            System.IO.Stream centerPtRectangleButtonPath = currentAssembly
                .GetManifestResourceStream
                ("QubeItTools.Resources.Vertical Mid Point Rectangle.ico");

            int largeIconSize = 32;
            int standardIconSize = 16;

            Icon VerticalMidPointRectangleIcon = new Icon(centerPtRectangleButtonPath);
            Icon largeVerticalMidPointRectangleIcon = new Icon(VerticalMidPointRectangleIcon, largeIconSize,
                largeIconSize);
            Icon standardVerticalMidPointRectangleIcon = new Icon(VerticalMidPointRectangleIcon,
                standardIconSize, standardIconSize);

            stdole.IPictureDisp largeIconPictureDisp =
                    (stdole.IPictureDisp)Support.IconToIPicture(largeVerticalMidPointRectangleIcon);
            stdole.IPictureDisp standardIconPictureDisp =
                (stdole.IPictureDisp)Support.IconToIPicture(standardVerticalMidPointRectangleIcon);

            ButtonDefinition = StandardAddInServer.InventorApplication.CommandManager.ControlDefinitions.
                    AddButtonDefinition("V.M.P.R.", ClientButtonInternalName,
                    CommandTypesEnum.kShapeEditCmdType, StandardAddInServer.AddInServerId,
                    "Create Vertical Mid Point Rectangle",
                    "Creates a rectangle that is constrained to the mid-point of one of the vertical lines",
                    standardIconPictureDisp, largeIconPictureDisp,
                    ButtonDisplayEnum.kDisplayTextInLearningMode);

            ButtonDefinition.Enabled = true;
            buttonPressed = ButtonDefinition.Pressed;
            CommandIsRunning = false;
        }
    }
}
