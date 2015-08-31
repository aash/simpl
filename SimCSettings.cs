#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Styx;
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
        public SerializableDictionary<WoWSpec, string> Specs { get; set; }
        public bool SuperLog { get; set; }
        private static SimCSettings DefaultConfig()
        {
            var ret = new SimCSettings
            {
                Cooldowns = new Hotkey(),
                Aoe = new Hotkey(),
                Burst = new Hotkey(),
                Execution = new Hotkey(),
                Specs = new SerializableDictionary<WoWSpec, string>(),
                SuperLog = false,
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

        public static string SettingsFileName = Path.Combine(Settings.CharacterSettingsDirectory, "SimCSettings.xml");

        public static void Save()
        {
            using (var writer = XmlWriter.Create(
                SettingsFileName,
                new XmlWriterSettings() { Indent = true }))
            {
                var serializer = new XmlSerializer(typeof(SimCSettings));
                serializer.Serialize(writer, currentSettings);
            }
        }

        public static void Load()
        {
            try
            {
                SimcraftImpl.Write("Loading configuration");
                using (var reader = XmlReader.Create(SettingsFileName))
                {
                    var serializer = new XmlSerializer(typeof (SimCSettings));
                    currentSettings = serializer.Deserialize(reader) as SimCSettings;
                    SimcraftImpl.Write("Configuration successfully loaded.");
                }
            }
            catch 
            {
                SimcraftImpl.Write("Failed to load configuration, creating default configuration.");
                _currentSettings = DefaultConfig();
            }
        }

        public class Hotkey
        {
            public Keys key;
            public ModifierKeys mod;
        }

    }

    [Serializable]
    [XmlRoot("Dictionary")]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {
        private const string DefaultTagItem = "Item";
        private const string DefaultTagKey = "Key";
        private const string DefaultTagValue = "Value";
        private static readonly XmlSerializer KeySerializer =
                                        new XmlSerializer(typeof(TKey));

        private static readonly XmlSerializer ValueSerializer =
                                        new XmlSerializer(typeof(TValue));

        public SerializableDictionary()
            : base()
        {
        }

        protected SerializableDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected virtual string ItemTagName
        {
            get { return DefaultTagItem; }
        }

        protected virtual string KeyTagName
        {
            get { return DefaultTagKey; }
        }

        protected virtual string ValueTagName
        {
            get { return DefaultTagValue; }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (wasEmpty)
            {
                return;
            }

            try
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadStartElement(this.ItemTagName);
                    try
                    {
                        TKey tKey;
                        TValue tValue;

                        reader.ReadStartElement(this.KeyTagName);
                        try
                        {
                            tKey = (TKey)KeySerializer.Deserialize(reader);
                        }
                        finally
                        {
                            reader.ReadEndElement();
                        }

                        reader.ReadStartElement(this.ValueTagName);
                        try
                        {
                            tValue = (TValue)ValueSerializer.Deserialize(reader);
                        }
                        finally
                        {
                            reader.ReadEndElement();
                        }

                        this.Add(tKey, tValue);
                    }
                    finally
                    {
                        reader.ReadEndElement();
                    }

                    reader.MoveToContent();
                }
            }
            finally
            {
                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
            {
                writer.WriteStartElement(this.ItemTagName);
                try
                {
                    writer.WriteStartElement(this.KeyTagName);
                    try
                    {
                        KeySerializer.Serialize(writer, keyValuePair.Key);
                    }
                    finally
                    {
                        writer.WriteEndElement();
                    }

                    writer.WriteStartElement(this.ValueTagName);
                    try
                    {
                        ValueSerializer.Serialize(writer, keyValuePair.Value);
                    }
                    finally
                    {
                        writer.WriteEndElement();
                    }
                }
                finally
                {
                    writer.WriteEndElement();
                }
            }
        }
    }

}