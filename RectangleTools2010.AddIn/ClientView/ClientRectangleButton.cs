using Inventor;
using InventorEvents2010.Interfaces;
using QubeItTools.Interfaces;

namespace QubeItTools.ClientView
{
    /// <summary>
    /// This is the base class for the different Rectangle buttons. The only thing that the 
    /// sub-classes need to do is create their own constructor.
    /// </summary>
    public abstract class ClientRectangleButton
    {
        #region Member variables
        protected IRectangleLogic clientRectangleLogicInstance;
        protected Inventor.Application invApplication;
        protected IButtonEventsLib buttonEventsLibrary;
        protected IRectangleDropDownController rectDropDownController;

        protected readonly int largeIconSize = 32;
        protected readonly int standardIconSize = 16;
        #endregion

        #region Properties
        public bool IsDefaultDisplayedCommand { get; set; }

        /// <summary>
        /// Property that returns the control's internal name
        /// </summary>
        public string ClientButtonInternalName { get; protected set; }

        /// <summary>
        /// Property that returns the control's button definition 
        /// </summary>
        public ButtonDefinition ButtonDefinition { get; protected set; }

        public bool CommandIsRunning { get; set; }

        protected bool buttonPressed;
        /// <summary>
        /// Property that sets the UI visual effect of the button being pressed
        /// </summary>
        public virtual bool ButtonPressed
        {
            get { return buttonPressed; }

            set
            {
                buttonPressed = value;
                this.ButtonDefinition.Pressed = value;

                //This "if" statement is how the command stays active until the user terminates it. 
                //When the user makes his second click to Lock the rectangle, the code inside that 
                //event handler sets the ButtonPressed property to false, but leaves the 
                //CommandIsRunning property set to true. This causes this "if" statement to execute 
                //and start the process all over again. When the user terminates the command, the 
                //logic that handles that event actually sets the CommandIsRunning to false which 
                //avoids execution of this "if" statement.
                if (CommandIsRunning)
                {
                    CommandIsRunning = false;
                    this.ButtonDefinition.Execute();
                }

                //This decision statement is used when the User Interface is leveraging the 
                //drop-down control.
                //This "if" statement will run when the user terminates the command and the 
                //currently displayed button is NOT the default displayed command.  It basically 
                //changes the displayed command back to the original default displayed command.
                if(rectDropDownController != null && !buttonPressed && !IsDefaultDisplayedCommand)
                {
                    rectDropDownController.ChangeDisplayedControl(rectDropDownController.DefaultDisplayControl);
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Event Handler for the button's execute event
        /// </summary>
        /// <param name="context"></param>
        protected virtual void ClientRectangleButtonDefinition_OnExecute(Inventor.NameValueMap context)
        {
            invApplication.CommandManager.StopActiveCommand();
            this.ButtonPressed = true;
            CommandIsRunning = true;
            clientRectangleLogicInstance.StartRectangleInteraction();
        }

        /// <summary>
        /// Deactivates the button
        /// </summary>
        public virtual void Deactivate()
        {
            buttonEventsLibrary.Deactivate();
            ButtonDefinition.Delete();
            ButtonDefinition = null;
        }
        #endregion
    }
}
