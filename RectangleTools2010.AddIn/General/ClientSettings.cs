using System;
using QubeItTools.Interfaces;

namespace QubeItTools.General
{
    public enum RectangleInterfaceStyle
    {
        Panel,
        DropDown
    };
    
    public class ClientSettings : IClientSettings
    {
        const string RECTANGLE_SETTINGS_FILE_NAME = @"Rectangle Settings.txt";
        
        #region Properties
        RectangleInterfaceStyle currentRectangleInterface;
        public RectangleInterfaceStyle CurrentRectangleInterface
        {
            get
            {
                var retrievedStyleFromFile = ClientHelper.LoadFromIsolatedStorage(RECTANGLE_SETTINGS_FILE_NAME);

                currentRectangleInterface = !String.IsNullOrEmpty(retrievedStyleFromFile)
                                                            ? ConvertStringToEnum(retrievedStyleFromFile)
                                                             : RectangleInterfaceStyle.Panel;
                return currentRectangleInterface;
            }
            set
            {
                currentRectangleInterface = value;
                string interfaceStyle = ConvertEnumToString(value);

                ClientHelper.SaveToIsolatedStorage(RECTANGLE_SETTINGS_FILE_NAME, interfaceStyle);
            }
        }
        #endregion
        
        //CTOR
        public ClientSettings()
        {
            
        }

        private RectangleInterfaceStyle ConvertStringToEnum(string settingReadFromFile)
        {
            RectangleInterfaceStyle returnValue;
            switch(settingReadFromFile)
            {
                case "Panel":
                    returnValue = RectangleInterfaceStyle.Panel;
                    break;
                case "DropDown":
                    returnValue = RectangleInterfaceStyle.DropDown;
                    break;
                default:
                    returnValue = RectangleInterfaceStyle.Panel;
                    break;
            }

            return returnValue;
        }

        private string ConvertEnumToString(RectangleInterfaceStyle enumValue)
        {
            string convertedValue;

            switch(enumValue)
            {
                case RectangleInterfaceStyle.DropDown:
                    convertedValue = "DropDown";
                    break;
                case RectangleInterfaceStyle.Panel:
                    convertedValue = "Panel";
                    break;
                default:
                    //TODO: add a logging error here
                    convertedValue = "Panel";
                    break;
            }

            return convertedValue;
        }

    }
}
