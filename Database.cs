using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Styx.Common;

namespace Simcraft
{


      /*{ SD_TYPE_UNSIGNED, "id"             },
      { SD_TYPE_UNSIGNED, "flags"          },
      { SD_TYPE_UNSIGNED, "spell_id"       },
      { SD_TYPE_UNSIGNED, "index"          },
      { SD_TYPE_INT,      "type"           },
      { SD_TYPE_INT,      "sub_type"       },
      { SD_TYPE_DOUBLE,   "m_average"      },
      { SD_TYPE_DOUBLE,   "m_delta"        },
      { SD_TYPE_DOUBLE,   "m_bonus"        },
      { SD_TYPE_DOUBLE,   "coefficient"    },
      { SD_TYPE_DOUBLE,   "ap_coefficient" },
      { SD_TYPE_DOUBLE,   "amplitude"      },
      { SD_TYPE_DOUBLE,   "radius"         },
      { SD_TYPE_DOUBLE,   "max_radius"     },
      { SD_TYPE_INT,      "base_value"     },
      { SD_TYPE_INT,      "misc_value"     },
      { SD_TYPE_INT,      "misc_value2"    },
      { SD_TYPE_UNSIGNED, ""               }, // Family flags 1
      { SD_TYPE_UNSIGNED, ""               }, // Family flags 2
      { SD_TYPE_UNSIGNED, ""               }, // Family flags 3
      { SD_TYPE_UNSIGNED, ""               }, // Family flags 4
      { SD_TYPE_INT,      "trigger_spell"  },
      { SD_TYPE_DOUBLE,   "m_chain"        },
      { SD_TYPE_DOUBLE,   "p_combo_points" },
      { SD_TYPE_DOUBLE,   "p_level"        },
      { SD_TYPE_INT,      "damage_range"   },*/

    [Serializable]
    public class Effect
    {
        public uint id;
        public uint flags;
        public uint spell_id;
        public uint index;
        public string type;
        public string sub_type;
        public double m_average;
        public double m_delta;
        public double m_bonus;
        public double coefficient;
        public double ap_coefficient;
        public double amplitude;
        public double radius;
        public double max_radius;
        public int base_value;
        public int misc_value;
        public int misc_value2;
        public uint flags0; // Family flags 1
        public uint flags1; // Family flags 2
        public uint flags2; // Family flags 3
        public uint flags3; // Family flags 4
        public int trigger_spell;
        public double m_chain;
        public double p_combo_points;
        public double p_level;
        public int damage_range;
    }

    public sealed class CurrentAssemblyDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            //if (assemblyName.Contains("Simcraft")){
            //Write("Exchanging: " + assemblyName + " for " + Assembly.GetExecutingAssembly().FullName);
            assemblyName = assemblyName.Replace("Simcraft, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", Assembly.GetExecutingAssembly().FullName);
            typeName = typeName.Replace("Simcraft, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", Assembly.GetExecutingAssembly().FullName);
            //}
            //Write(String.Format("{0}, {1}", typeName, assemblyName));
            return Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
        }
    }

    [Serializable]
    public class spell_data_t
    {
        public String token;
        public String name;
        public uint id; //1
        public uint flags;//2
        public double speed;//3
        public uint school; //4
        public uint _class; //5 
        public uint race; //6
        public int scaling; //7
        public uint max_scaling_level;//8
        public uint level;//9
        public uint max_level;//10
        public double min_range;//11
        public double max_range;//12
        public uint cooldown;//13
        public uint gcd;//14
        public uint charges;//15
        public uint charge_cooldown;//16
        public uint category;//17
        public int duration;//18
        public uint runes; //19
        public uint power_gain;//20
        public uint max_stack;//21
        public uint proc_chance;//22
        public uint initial_stack;//23
        public uint pflags; //24
        public uint icd;//25
        public double rppm;//26
        public uint equip_class;//27
        public uint equip_imask;//28
        public uint equip_scmask;//29
        public int cast_min;//30
        public int cast_max;//31
        public int cast_div;//32
        public double m_scaling;//33
        public uint scaling_level;//34
        public uint replace_spellid;//35
        public uint attr0; // Attributes, 0..11, not done for now
        public uint attr1;
        public uint attr2;
        public uint attr3;
        public uint attr4;
        public uint attr5;
        public uint attr6;
        public uint attr7;
        public uint attr8;
        public uint attr9;
        public uint attr10;
        public uint attr11;
        public uint flags0; // Family flags 1
        public uint flags1; // Family flags 1
        public uint flags2; // Family flags 1
        public uint flags3; // Family flags 1
        public uint family; // Family
        public String desc;
        public String tooltip;
        public String desc_vars;
        public String icon;
        public String active_icon;
        public String rank;

        public List<uint> effects = new List<uint>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (System.Reflection.FieldInfo property in this.GetType().GetFields())
            {
                sb.Append(property.Name);
                sb.Append(": ");

                sb.Append(property.GetValue(this));
                sb.Append(System.Environment.NewLine);
            }

            return sb.ToString();
        }

    };

    [Serializable]
    public class Database
    {
        public TwoKeyedDictionary<uint, String, spell_data_t> Spells = new TwoKeyedDictionary<uint, String, spell_data_t>();
        public Dictionary<String, uint> ClassSpells = new Dictionary<string, uint>();
        public Dictionary<String, uint> Glyphs = new Dictionary<string, uint>();
        public Dictionary<uint, Effect> Effects = new Dictionary<uint, Effect>();
        public Dictionary<String, Dictionary<uint, uint>> Sets = new Dictionary<string, Dictionary<uint, uint>>();
        public Dictionary<uint, String> ItemProcs = new Dictionary<uint, String>();
    }

    public class Serializer
    {

        public static void SerializeObject(string filename, Database objectToSerialize)
        {
            Stream stream = File.Open(filename, FileMode.Create);
            //DeflateStream _stream = new DeflateStream(stream, CompressionLevel.Optimal);
            GZipStream __stream = new GZipStream(stream, CompressionLevel.Optimal);

            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(__stream, objectToSerialize);
            //_stream.Close();
            __stream.Close();
            stream.Close();
        }

        public static Database DeSerializeObject(string filename)
        {
            Database objectToSerialize;
            Stream stream = File.Open(filename, FileMode.Open);
            GZipStream __stream = new GZipStream(stream, CompressionMode.Decompress);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Binder = new CurrentAssemblyDeserializationBinder();
            objectToSerialize = (Database)bFormatter.Deserialize(__stream);
            stream.Close();
            return objectToSerialize;
        }
    }

    [Serializable]
    public class TwoKeyedDictionary<K, KT, V> : Dictionary<K, V>
    {
        Dictionary<KT, List<K>> relation = new Dictionary<KT, List<K>>();

        public new V this[K i]
        {
            get { return base[i]; }
        }

        public List<K> this[KT i]
        {

            get
            {
                //var rel = relation[i];
                return relation[i];
            }
        }

        public object this[K i, KT k]
        {
            set
            {
                base[i] = (V)value;
                if (!relation.ContainsKey(k))
                    relation[k] = new List<K>();
                relation[k].Add(i);
            }
        }

        public bool RelationContainsKey(KT kt)
        {
            return relation.ContainsKey(kt);
        }

        public TwoKeyedDictionary()
        {

        }

        public TwoKeyedDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

    }
}
