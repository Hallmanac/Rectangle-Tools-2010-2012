using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;

namespace QubeItTools.General
{
    public static class ClientHelper
    {
        public static System.Reflection.Assembly CurrentAssembly
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly();
            }
        }

        public static Stream GetClientResource(string resourcePath)
        {
            Stream clientResource = ClientHelper.CurrentAssembly.GetManifestResourceStream(resourcePath);

            if(clientResource != null)
            {
                return clientResource;
            }

            return null;
        }

        public static void SaveToIsolatedStorage(string fileName, string content)
        {
            using(var isoFile = IsolatedStorageFile.GetStore(IsolatedStorageScope.User
                                               | IsolatedStorageScope.Assembly, null, null))
            {
                using(var writer = new StreamWriter(new IsolatedStorageFileStream(fileName,
                    FileMode.Create, isoFile)))
                {
                    writer.WriteLine(content);
                }
            }
        }

        public static string LoadFromIsolatedStorage(string fileName)
        {
            using(IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                IsolatedStorageScope.Assembly, null, null))
            {
                if(isoStore.GetFileNames(fileName).Length > 0)
                {
                    using(StreamReader reader = new StreamReader
                        (new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, isoStore)))
                    {
                        return reader.ReadLine();
                    }
                }
            }

            return null;
        }
    }
}
