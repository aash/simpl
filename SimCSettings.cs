#region

using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Styx.Common;
using Styx.Helpers;

#endregion

namespace Simcraft
{
    public class SimCSettings
    {
        [XmlIgnore] private static SimCSettings _currentSettings;

        [XmlIgnore]
        public static SimCSettings currentSettings
        {
            get
            {
                if (_currentSettings == null)
                {
                    Load();
                }
                return _currentSettings;
            }
            set { _currentSettings = value; }
        }

        public Hotkey Cooldowns { get; set; }
        public Hotkey Aoe { get; set; }
        public Hotkey Burst { get; set; }
        public Hotkey Execution { get; set; }

        private static SimCSettings DefaultConfig()
        {
            var ret = new SimCSettings
            {
                Cooldowns = new Hotkey(),
                Aoe = new Hotkey(),
                Burst = new Hotkey(),
                Execution = new Hotkey()
            };

            ret.Cooldowns.key = Keys.C;
            ret.Cooldowns.mod = ModifierKeys.NoRepeat;
            ret.Aoe.key = Keys.A;
            ret.Aoe.mod = ModifierKeys.NoRepeat;
            ret.Burst.key = Keys.B;
            ret.Burst.mod = ModifierKeys.NoRepeat;
            ret.Execution.key = Keys.X;
            ret.Execution.mod = ModifierKeys.NoRepeat;

            return ret;
        }

        public static void Save()
        {
            var writer =
                new XmlSerializer(typeof (SimCSettings));
            var file =
                new StreamWriter(Path.Combine(Settings.CharacterSettingsDirectory, "SimCSettings.xml"), false);
            writer.Serialize(file, currentSettings);
            file.Close();
        }

        public static void Load()
        {
            try
            {
                SimcraftImpl.Write("Loading configuration");
                var reader =
                    new XmlSerializer(typeof (SimCSettings));
                var file =
                    new StreamReader(Path.Combine(Settings.CharacterSettingsDirectory, "SimCSettings.xml"));
                currentSettings = (SimCSettings) reader.Deserialize(file);
                SimcraftImpl.Write("Configuration successfully loaded.");
            }
            catch 
            {
                SimcraftImpl.Write("Failed to load configuration, creating default configuration.");
                //Write("Exception: " + e);
                _currentSettings = DefaultConfig();
            }
        }

        public class Hotkey
        {
            public Keys key;
            public ModifierKeys mod;
        }
    }
}