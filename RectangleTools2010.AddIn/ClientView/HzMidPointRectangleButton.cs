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
    class HzMidPointRectangleButton : ClientRectangleButton, IInventorButton
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
        public HzMidPointRectangleButton()
        {
            invApplication = StandardAddInServer.InventorApplication;
            ClientButtonInternalName = StandardAddInServer.AddInServerId +
                "HorizontalMidPointRectangleButton";
            clientRectangleLogicInstance = new HzMidPointRectangle(this, invApplication);

            try
            {
                CreateButtonDefinition();
                buttonEventsLibrary = new ButtonEventsLib(ButtonDefinition);

                buttonEventsLibrary.OnExecuteDelegate += ClientRectangleButtonDefinition_OnExecute;
                buttonEventsLibrary.ButtonDef.OnExecute += buttonEventsLibrary.OnExecuteDelegate;
            }
            catch (Exception e)
            {
                MessageBox.Show("Horizontal Mid-Point Rectangle button failed to initialize.\n\n"
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
                ("QubeItTools.Resources.Horizontal Mid-Point Rectangle.ico");

            int largeIconSize = 32;
            int standardIconSize = 16;

            Icon HzMidPtRectangleIcon = new Icon(centerPtRectangleButtonPath);
            Icon largeHzMidPtRectangleIcon = new Icon(HzMidPtRectangleIcon, largeIconSize,
                largeIconSize);
            Icon standardHzMidPtRectangleIcon = new Icon(HzMidPtRectangleIcon,
                standardIconSize, standardIconSize);

            stdole.IPictureDisp largeIconPictureDisp =
                    (stdole.IPictureDisp)Support.IconToIPicture(largeHzMidPtRectangleIcon);
            stdole.IPictureDisp standardIconPictureDisp =
                (stdole.IPictureDisp)Support.IconToIPicture(standardHzMidPtRectangleIcon);

            ButtonDefinition = invApplication.CommandManager.ControlDefinitions.
                    AddButtonDefinition("H.M.P.R.", ClientButtonInternalName,
                    CommandTypesEnum.kShapeEditCmdType, StandardAddInServer.AddInServerId,
                    "Create Horizontal Mid Point Rectangle",
                    "Creates a rectangle that is constrained to the mid-point of one of the horizontal lines.",
                    standardIconPictureDisp, largeIconPictureDisp,
                    ButtonDisplayEnum.kDisplayTextInLearningMode);

            ButtonDefinition.Enabled = true;
            buttonPressed = ButtonDefinition.Pressed;
            CommandIsRunning = false;
        }
    }
}
