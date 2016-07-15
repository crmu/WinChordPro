using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysIO = System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace WinChordPro
{
    [Serializable()]
    class Settings
    {
        public int fontSize { get; set; }
        public string font { get; set; }
        public string lastSongFolder { get; set; }
        public string lastSongLoaded { get; set; }
        private static string appDataFolder { get; set; }
        string fullPathToSettings;
        private static string settingsFileName = @"WinChordProSettings.ini";
        private static Log l = new Log();
        private int minFontSize = 8;

        public Settings()
        {
            initialize();
        }

        private void initialize()
        {
            l.log("Enter Settings initialize");
            fontSize = 14;
            font = "Courier New";
            lastSongFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic);
            lastSongLoaded = "";
            string appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            appDataFolder =  Path.Combine(appDataRoot, "WinChordPro");
            fullPathToSettings = Path.Combine(appDataFolder, settingsFileName);


            if (SysIO.File.Exists(fullPathToSettings))
            {
                loadSettings();
            }
            else if (!SysIO.File.Exists(appDataFolder))
            {
                l.log("Folder [" + appDataFolder + "] does not exist");
                try
                {
                    SysIO.Directory.CreateDirectory(appDataFolder);
                }
                catch (Exception e)
                {
                    l.log("Could not create settings directory");
                    l.log("Exception [" + e.InnerException + "]");
                }
            }
        }


        public void loadSettings()
        {
            l.log("Enter loadSettings");
            Stream myStream;
            BinaryFormatter formatter = new BinaryFormatter();
            Settings tempSettings;

            if (!SysIO.File.Exists(fullPathToSettings))
            {
                l.log("Could not find settings file[" + fullPathToSettings + "]");
                return;
            }
            try
            {
                myStream = File.Open(fullPathToSettings, FileMode.Open);
            }
            catch (Exception e)
            {
                l.log("Could not open [" + fullPathToSettings + "]");
                l.log("Exception [" + e.InnerException + "]");
                return;
            }
            formatter = new BinaryFormatter();
            tempSettings = (Settings)formatter.Deserialize(myStream);
            myStream.Dispose();

            if (tempSettings.lastSongFolder != null)
            {
                lastSongFolder = tempSettings.lastSongFolder;
                l.log("Loaded lastSongFolder [" + lastSongFolder + "]");
            }

            if (tempSettings.lastSongLoaded != null)
            {
                lastSongLoaded = tempSettings.lastSongLoaded;
                l.log("Loaded lastSongLoaded [" + lastSongLoaded + "]");
            }

            if (tempSettings.font != null)
            {
                font = tempSettings.font;
                l.log("Loaded font [" + font + "]");
            }

            if (tempSettings.fontSize > 0 )
            {
                fontSize = tempSettings.fontSize;
                l.log("Loaded fontSize [" + fontSize + "]");
            }
        }

        public void saveSettings()
        {
            //Serialize
            if (!SysIO.Directory.Exists(appDataFolder))
            {
                l.log("Folder [" + appDataFolder + "] does not exist");
                try
                {
                    SysIO.Directory.CreateDirectory(appDataFolder);
                }
                catch(Exception e)
                {
                    l.log("Could not create settings directory");
                    l.log("Exception [" + e.InnerException + "]");
                    return;
                }
            }
            l.log("Opening Stream to write [" + settingsFileName + "]");
            using (Stream myStream = File.Open(fullPathToSettings, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                try
                {
                    l.log("Attempting to serialize [" + settingsFileName + "]");
                    formatter.Serialize(myStream, this);
                }
                catch (Exception e)
                {
                    l.log("Exception [" + e.InnerException + "]");
                }
                finally
                {
                    myStream.Close();
                    myStream.Dispose();
                }
            }
        }

        internal void increaseFontSize()
        {
            fontSize++;
            saveSettings();
        }

        internal void decreaseFontSize()
        {
            if (fontSize > minFontSize)
            {
                fontSize--;
                saveSettings();
            }
        }
    }
}
