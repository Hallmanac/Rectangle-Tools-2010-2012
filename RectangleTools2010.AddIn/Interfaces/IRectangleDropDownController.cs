using Inventor;

namespace QubeItTools.Interfaces
{
    public interface IRectangleDropDownController
    {
        bool DisplayButtonPressed {get; set;}

        ButtonDefinition DefaultDisplayControl { get; }

        void ChangeDisplayedControl(ButtonDefinition buttonDefinition);
    }
}
