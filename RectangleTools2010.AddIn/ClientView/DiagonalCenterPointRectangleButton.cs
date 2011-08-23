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
    public class DiagonalCenterPointRectangleButton : ClientRectangleButton, IInventorButton
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
        public DiagonalCenterPointRectangleButton()
        {
            invApplication = StandardAddInServer.InventorApplication;
            ClientButtonInternalName = StandardAddInServer.AddInServerId
                + "DiagonalCenterPointRectangleButton";
            clientRectangleLogicInstance = new DiagonalCenterPointRectangle(this, invApplication);
            
            try
            {
                CreateButtonDefinition();
                buttonEventsLibrary = new ButtonEventsLib(ButtonDefinition);

                buttonEventsLibrary.OnExecuteDelegate += ClientRectangleButtonDefinition_OnExecute;
                buttonEventsLibrary.ButtonDef.OnExecute += buttonEventsLibrary.OnExecuteDelegate;
            }
            catch(Exception e)
            {
                MessageBox.Show("CenterPointRectangle button failed to initialize.\n\n"
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
                ("QubeItTools.Resources.DiagonalCenterPointRectangle.ico");

            Icon DiagonalCenterPointRectangleIcon = new Icon(centerPtRectangleButtonPath);
            Icon largeDiagonalCenterPointRectangleIcon = new Icon(DiagonalCenterPointRectangleIcon, largeIconSize,
                largeIconSize);
            Icon standardDiagonalCenterPointRectangleIcon = new Icon(DiagonalCenterPointRectangleIcon,
                standardIconSize, standardIconSize);

            stdole.IPictureDisp largeIconPictureDisp =
                    (stdole.IPictureDisp)Support.IconToIPicture(largeDiagonalCenterPointRectangleIcon);
            stdole.IPictureDisp standardIconPictureDisp =
                (stdole.IPictureDisp)Support.IconToIPicture(standardDiagonalCenterPointRectangleIcon);

            ButtonDefinition = invApplication.CommandManager.ControlDefinitions.
                    AddButtonDefinition("D.C.P.R.", ClientButtonInternalName,
                    CommandTypesEnum.kShapeEditCmdType, StandardAddInServer.AddInServerId,
                    "Create Diagonal Center Point Rectangle",
                    "Creates a Diagonal Center Point Rectangle around the origin of where the " +
                    "user picks using a diagonal line.",
                    standardIconPictureDisp, largeIconPictureDisp,
                    ButtonDisplayEnum.kDisplayTextInLearningMode);

            ButtonDefinition.Enabled = true;
            buttonPressed = ButtonDefinition.Pressed;
            CommandIsRunning = false;
        }
    }
}
