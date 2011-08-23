using Inventor;

namespace QubeItTools.Interfaces
{
    public interface IInventorButton 
    {
        ButtonDefinition ButtonDefinition { get; }

        bool ButtonPressed { get; set; }

        bool CommandIsRunning { get; set; }

        void Deactivate();

        string ClientButtonInternalName { get; }
    }
}
