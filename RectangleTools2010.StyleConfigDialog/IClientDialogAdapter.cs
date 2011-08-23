namespace StyleConfigDialog
{
    /// <summary>
    /// This is how the AddIn project interacts gets information from the dialog box.
    /// This interface is implemented in the "General" namespace in the "ClientDialogAdapter" class.
    /// </summary>
    public interface IClientDialogAdapter
    {
        bool PanelLayout { get; set; }
        bool DropDown { get; set; }
    }
}