using Inventor;

namespace QubeItTools.Interfaces
{
    public interface IChangeProcessorParent
    {
        void ChangeProcessor_OnExecute(_Document Document, NameValueMap Context, ref bool Succeeded);
    }
}
