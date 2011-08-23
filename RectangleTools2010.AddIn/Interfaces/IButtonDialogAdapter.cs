namespace QubeItTools.Interfaces
{
    internal interface IButtonDialogAdapter
    {
        bool PanelLayout { get; set; }
        bool DropDown { get; set; }
        void LaunchConfigDialog();
    }
}