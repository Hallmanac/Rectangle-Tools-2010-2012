using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Inventor;
using InventorEvents2010;
using InventorEvents2010.Interfaces;
using Microsoft.VisualBasic.Compatibility.VB6;
using QubeItTools.General;
using QubeItTools.Interfaces;
using System.Windows.Forms;

namespace QubeItTools.ClientView
{
    public class RectangleConfigButton : IRectangleCongigButton
    {
        private const int LARGE_ICON_SIZE = 32;
        private const int STANDARD_ICON_SIZE = 16;
        private Inventor.Application invApplication;
        readonly IButtonEventsLib buttonEventsLibrary;
        private bool buttonPressed;

        public RectangleConfigButton()
        {
            this.invApplication = StandardAddInServer.InventorApplication;
            ClientButtonInternalName = StandardAddInServer.AddInServerId + "RectangleConfigButton";

            try
            {
                CreateButtonDefinition();
                buttonEventsLibrary = new ButtonEventsLib(ButtonDefinition);
                buttonEventsLibrary.OnExecuteDelegate += RectangleConfigButtonOnExecute;
                buttonEventsLibrary.ButtonDef.OnExecute += buttonEventsLibrary.OnExecuteDelegate;
            }
            catch (Exception e)
            {
                var dialogResult = MessageBox.Show("Rectangle Config Button did not implement properly.\n\n" + e.ToString());
            }
        }


        public ButtonDefinition ButtonDefinition { get; private set; }

        public bool ButtonPressed { get; set; }

        public bool CommandIsRunning { get; set; }

        public void Deactivate()
        {
            buttonEventsLibrary.Deactivate();
            ButtonDefinition.Delete();
            ButtonDefinition = null;
        }

        public string ClientButtonInternalName { get; private set; }
        public void AddRibbonInterface()
        {
            if(ButtonDefinition == null)
                CreateButtonDefinition();
            CommandControls applicationMenu = invApplication.UserInterfaceManager.FileBrowserControls;
            applicationMenu.AddButton(this.ButtonDefinition, true, TargetControlInternalName: "AppiPropertiesWrapperCmd"
                                      , InsertBeforeTargetControl: true);
        }

        void CreateButtonDefinition()
        {
            var currentAssembly = Assembly
                .GetExecutingAssembly();
            var rectangleConfigButtonPath = currentAssembly
                .GetManifestResourceStream(string.Format("QubeItTools.Resources.RectangleConfigButton.ico"));

            if(rectangleConfigButtonPath != null)
            {
                var rectangleConfigButtonIcon = new Icon(rectangleConfigButtonPath);
                var largeRectangleConfigButtonIcon = new Icon(rectangleConfigButtonIcon, LARGE_ICON_SIZE,
                                                             LARGE_ICON_SIZE);
                var standardRectangleConfigIcon = new Icon(rectangleConfigButtonIcon,
                                                                STANDARD_ICON_SIZE, STANDARD_ICON_SIZE);

                var largeIconPictureDisp =
                    (stdole.IPictureDisp)Support.IconToIPicture(largeRectangleConfigButtonIcon);
                var standardIconPictureDisp =
                    (stdole.IPictureDisp)Support.IconToIPicture(standardRectangleConfigIcon);

                ButtonDefinition = invApplication.CommandManager.ControlDefinitions.
                    AddButtonDefinition("Rectangle Tools Settings", ClientButtonInternalName,
                                        CommandTypesEnum.kShapeEditCmdType, StandardAddInServer.AddInServerId,
                                        "Configure User Interface Settings for Rectangle tools.",
                                        "Launches a dialog box that configures the Rectangle Tools User Interface.",
                                        standardIconPictureDisp, largeIconPictureDisp,
                                        ButtonDisplayEnum.kDisplayTextInLearningMode);
            }

            ButtonDefinition.Enabled = true;
            buttonPressed = ButtonDefinition.Pressed;
            CommandIsRunning = false;

        }

        private void RectangleConfigButtonOnExecute(NameValueMap context)
        {
            invApplication.CommandManager.StopActiveCommand();
            this.ButtonPressed = true;
            CommandIsRunning = true;
            IButtonDialogAdapter buttonDialogAdapter = new ClientDialogAdapter();
            buttonDialogAdapter.LaunchConfigDialog();
        }
    }
}