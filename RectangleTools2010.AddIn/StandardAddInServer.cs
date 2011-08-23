using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Inventor;
using Microsoft.Win32;
using QubeItTools.ClientConfig;
using QubeItTools.Interfaces;
using Attribute = System.Attribute;

namespace QubeItTools
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("ff90a8ab-f4dd-4539-810f-adbf72f7ee95")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {
        #region Properties
        //variable to hold the control manager(s)
        public IClientConfig RectangleDependencyManager { get; private set; }

        // Inventor application object.
        public static Inventor.Application InventorApplication { get; private set; }

        public static string AddInServerId { get; private set; }
        #endregion

        //Constructor
        public StandardAddInServer()
        {
        }

        #region ApplicationAddInServer Methods

        /// <summary>
        ///  This method is called by Inventor when it loads the addin.
        ///  The AddInSiteObject provides access to the Inventor Application object.
        ///  The FirstTime flag indicates if the addin is loaded for the first time.
        /// </summary>
        /// <param name="addInSiteObject"></param>
        /// <param name="firstTime"></param>
        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {            
            try
            {
                InventorApplication = addInSiteObject.Application;

                //retrieve the GUID for this class and assign it to the string member variable 
                //intended to hold it
                GuidAttribute addInClsid = (GuidAttribute)Attribute.GetCustomAttribute
                                                (typeof(StandardAddInServer), typeof(GuidAttribute));
                string addInClsidString = "{" + addInClsid.Value + "}";
                AddInServerId = addInClsidString;

                //Set a reference to the user interface manager to determine the interface style
                UserInterfaceManager userInterfaceManager = InventorApplication.UserInterfaceManager;
                InterfaceStyleEnum interfaceStyle = userInterfaceManager.InterfaceStyle;

                RectangleDependencyManager = new RectangleToolsDependencyMapper();

                if (interfaceStyle == InterfaceStyleEnum.kRibbonInterface)
                {
                    if (firstTime == true)
                    {
                        RectangleDependencyManager.InitializeUserInterface();
                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        /// <summary>
        /// This method is called by Inventor when the AddIn is unloaded.
        /// The AddIn will be unloaded either manually by the user or
        /// when the Inventor session is terminated
        /// </summary>
        public void Deactivate()
        {
            RectangleDependencyManager.Deactivate();

            // Release objects.
            Marshal.ReleaseComObject(InventorApplication);
            InventorApplication = null;

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Note:this method is now obsolete, you should use the 
        /// ControlDefinition functionality for implementing commands.
        /// </summary>
        /// <param name="commandID"></param>
        public void ExecuteCommand(int commandID)
        {
            
        }

        /// <summary>
        ///  This property is provided to allow the AddIn to expose an API 
        ///  of its own to other programs. Typically, this  would be done by
        ///  implementing the AddIn's API interface in a class and returning 
        ///  that class object through this property.
        /// </summary>
        public object Automation
        {
            get
            {
                // TODO: Add ApplicationAddInServer.Automation getter implementation
                return null;
            }
        }

        #endregion

        #region COM Registration functions

        /// <summary>
        /// Registers this class as an Add-In for Autodesk Inventor.
        /// This function is called when the assembly is registered for COM.
        /// </summary>
        [ComRegisterFunctionAttribute()]
        public static void Register(Type t)
        {
            RegistryKey clssRoot = Registry.ClassesRoot;
            RegistryKey clsid = null;
            RegistryKey subKey = null;

            try
            {
                clsid = clssRoot.CreateSubKey("CLSID\\" + AddInGuid(t));
                clsid.SetValue(null, "Qube-It Tools");
                subKey = clsid.CreateSubKey("Implemented Categories\\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}");
                subKey.Close();

                subKey = clsid.CreateSubKey("Settings");
                subKey.SetValue("AddInType", "Standard");
                subKey.SetValue("LoadOnStartUp", "1");

                //subKey.SetValue("SupportedSoftwareVersionLessThan", "");
                subKey.SetValue("SupportedSoftwareVersionGreaterThan", "13..");
//                subKey.SetValue("SupportedSoftwareVersionEqualTo", "15");
//                subKey.SetValue("SupportedSoftwareVersionEqualTo", "15");
                //subKey.SetValue("SupportedSoftwareVersionNotEqualTo", "");
                //subKey.SetValue("Hidden", "0");
                //subKey.SetValue("UserUnloadable", "1");
                subKey.SetValue("Version", 1);
                subKey.Close();

                subKey = clsid.CreateSubKey("Description");
                subKey.SetValue(null, "Rectangle drawing tools created by Brian Hall");
            }
            catch
            {
                System.Diagnostics.Trace.Assert(false);
            }
            finally
            {
                if (subKey != null) subKey.Close();
                if (clsid != null) clsid.Close();
                if (clssRoot != null) clssRoot.Close();
            }

        }

        /// <summary>
        /// Unregisters this class as an Add-In for Autodesk Inventor.
        /// This function is called when the assembly is unregistered.
        /// </summary>
        [ComUnregisterFunctionAttribute()]
        public static void Unregister(Type t)
        {
            RegistryKey clssRoot = Registry.ClassesRoot;
            RegistryKey clsid = null;

            try
            {
                clssRoot = Microsoft.Win32.Registry.ClassesRoot;
                clsid = clssRoot.OpenSubKey("CLSID\\" + AddInGuid(t), true);
                clsid.SetValue(null, "");
                clsid.DeleteSubKeyTree("Implemented Categories\\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}");
                clsid.DeleteSubKeyTree("Settings");
                clsid.DeleteSubKeyTree("Description");
            }
            catch { }
            finally
            {
                if (clsid != null) clsid.Close();
                if (clssRoot != null) clssRoot.Close();
            }
        }

        // This function uses reflection to get the value for the GuidAttribute attached to the class.
        private static String AddInGuid(Type t)
        {
            string guid = "";

            try
            {
                Object[] customAttributes = t.GetCustomAttributes(typeof(GuidAttribute), false);
                GuidAttribute guidAttribute = (GuidAttribute)customAttributes[0];
                guid = "{" + guidAttribute.Value.ToString() + "}";
            }
            catch
            {
            }

            return guid;

        }

        #endregion

    }
}
