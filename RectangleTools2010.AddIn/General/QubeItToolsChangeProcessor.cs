using Inventor;
using QubeItTools.Interfaces;

namespace QubeItTools.General
{
    public class QubeItToolsChangeProcessor
    {
        #region Member Variables
        private Inventor.Application _inventorApplication;
        private IChangeProcessorParent _parentClass;
        private string _clientID;
        private ChangeProcessorSink_OnExecuteEventHandler _onExecute_Delegate;
        private ChangeProcessorSink_OnTerminateEventHandler _onTerminate_Delegate;
        private ChangeManager _changeManager;
        private ChangeDefinitions _changeDefinitions;
        private ChangeDefinition _changeDefinition;
        private ChangeProcessor _changeProcessor;
        private string _changeDefInternalName;
        private string _changeDefCommandName;
        #endregion

        public QubeItToolsChangeProcessor(Inventor.Application inventorApplication, IChangeProcessorParent parentClass, string internalName,
            string commandName, string clientID)
        {
            _inventorApplication = inventorApplication;
            _parentClass = parentClass;
            _clientID = clientID;
            _changeDefInternalName = internalName;
            _changeDefCommandName = commandName;
        }

        #region Methods
        /// <summary>
        /// Method that is used to encapsulate a series of actions into a single transaction for the user
        /// to undo or redo.
        /// </summary>
        public void Connect()
        {
            //Establish a reference to the change manager 
            _changeManager = _inventorApplication.ChangeManager;

            //Establish a reference to the Change Definitions collection for this add-in
            _changeDefinitions = _changeManager.Add(_clientID);

            _changeDefinition = _changeDefinitions.Add(_changeDefInternalName, _changeDefCommandName);

            //Create a change processor
            _changeProcessor = _changeDefinition.CreateChangeProcessor();

            //Connect the events
            this._onExecute_Delegate = new ChangeProcessorSink_OnExecuteEventHandler(changeProcessor_OnExecute);
            _changeProcessor.OnExecute += this._onExecute_Delegate;

            this._onTerminate_Delegate = new ChangeProcessorSink_OnTerminateEventHandler(changeProcessor_OnTerminate);
            _changeProcessor.OnTerminate += this._onTerminate_Delegate;

            //Calling the Execute method is what causes the transaction to occur.
            _changeProcessor.Execute(_inventorApplication.ActiveEditDocument);
        }

        /// <summary>
        /// Method that abstracts the OnTerminate event handler and allows for the change processor to be terminated
        /// from other call sites in the program.
        /// </summary>
        public void Disconnect()
        {
            //Disconnect the events sink and set the change processor to null
            if (_changeProcessor != null)
            {
                this._changeProcessor.OnExecute -= this._onExecute_Delegate;
                this._changeProcessor.OnTerminate -= this._onTerminate_Delegate;

                this._changeDefinitions = null;
                this._changeDefinition.Delete();
                this._changeDefinition = null;
                this._changeProcessor = null;
            }
        }

        /// <summary>
        /// Event handler for the OnTerminate event.
        /// </summary>
        void changeProcessor_OnTerminate()
        {
            this.Disconnect();
        }

        /// <summary>
        /// Event handler for the execute event. This method is effectively what gets called from the Connect method
        /// and simply passes execution right along to the parent class
        /// </summary>
        /// <param name="Document"></param>
        /// <param name="Context"></param>
        /// <param name="Succeeded"></param>
        void changeProcessor_OnExecute(_Document Document, NameValueMap Context, ref bool Succeeded)
        {
            _parentClass.ChangeProcessor_OnExecute(Document, Context, ref Succeeded);
        }
        #endregion
    }
}
