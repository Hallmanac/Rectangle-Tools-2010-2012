using QubeItTools.Interfaces;
using StyleConfigDialog;

namespace QubeItTools.General
{
    class ClientDialogAdapter : IClientDialogAdapter, IButtonDialogAdapter
    {
        private readonly RectangleButtonLayoutDialog rectangleButtonConfigDialog;
        private readonly IClientSettings clientSettings;
        
        public ClientDialogAdapter()
        {
            rectangleButtonConfigDialog = new RectangleButtonLayoutDialog(this);
            clientSettings = new ClientSettings();
        }

        private bool panelLayout;
        public bool PanelLayout
        {
            get
            {
                panelLayout = clientSettings.CurrentRectangleInterface == RectangleInterfaceStyle.Panel;
                return panelLayout;
            }
            set
            {
                panelLayout = value;
                if(value)
                    clientSettings.CurrentRectangleInterface = RectangleInterfaceStyle.Panel;
            }
        }

        private bool dropDown;
        public bool DropDown
        {
            get
            {
                dropDown = clientSettings.CurrentRectangleInterface == RectangleInterfaceStyle.DropDown;
                return dropDown;
            }
            set
            {
                dropDown = value;

                if (value)
                {
                    clientSettings.CurrentRectangleInterface = RectangleInterfaceStyle.DropDown;
                }
            }
        }

        public void LaunchConfigDialog()
        {
            rectangleButtonConfigDialog.ShowDialog();
        }
    }
}