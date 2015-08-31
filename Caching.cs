using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Styx;
using Styx.Common;
using Styx.WoWInternals;

namespace Simcraft
{

    public delegate object CacheRetrievalDelegate();

    public abstract class Internal : DynamicObject
    {
        public static SimcraftImpl simc
        {
            get { return SimcraftImpl.inst; }
        }

    }

    public class CacheInternal : Internal
    {

        private String bossname;

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Properties.Keys.ToArray();
        }

        private SimcraftImpl.GetUnitDelegate Retrieve;

        public CacheInternal(SimcraftImpl.GetUnitDelegate del, String bossname)
        {
            this.bossname = bossname;
            Retrieve = del;
        }

        public CacheInternal( String bossname)
        {
            this.bossname = bossname;
            Retrieve = () => StyxWoW.Me;
        }

        protected Dictionary<String, CacheRetrievalDelegate> Properties =
            new Dictionary<string, CacheRetrievalDelegate>();

        protected Dictionary<WoWGuid, ProxyCacheEntry> Cache = new Dictionary<WoWGuid, ProxyCacheEntry>();

        public void AddProperty(String name, CacheRetrievalDelegate del)
        {
            Properties.Add(name, del);
        }

        private ProxyCacheEntry NewProxyCacheEntry()
        {
            var npce = new ProxyCacheEntry();
            foreach (var prop in Properties.Keys)
            {
                npce[prop].SetRetrievalDelegate(Properties[prop]);
            }
            return npce;
        }

        public MagicValueType this[String propertyName]
        {
            get
            {
                if (Retrieve() == null) return new MagicValueType(0);
                var guid = Retrieve().Guid;

                if (!Cache.ContainsKey(guid)) Cache[guid] = NewProxyCacheEntry();
                object val = Cache[guid][propertyName].Value;

                SimcraftImpl.LogDebug(bossname + " Property: " + propertyName + " : " + val);

                if (val is Decimal) return new MagicValueType(Convert.ToDecimal(val));
                if (val is bool) return new MagicValueType((bool) val);
                if (val is int) return new MagicValueType(Convert.ToDecimal(val));
                if (val is double) return new MagicValueType(Convert.ToDecimal(val));
                if (val is MagicValueType) return (MagicValueType) val;
                throw new ConstraintException("Unsupported Caching Types");
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }

  
    }

    public class ProxyCacheEntry
    {
        private readonly Dictionary<String, CacheValue> _values = new Dictionary<string, CacheValue>();

        public CacheValue this[String name]
        {
            get
            {
                if (!_values.ContainsKey(name)) _values[name] = new CacheValue();
                return _values[name];
            }
        }

        public class CacheValue
        {

            public static int CacheInterval = 1;

            protected CacheRetrievalDelegate Retrieve;

            public void SetRetrievalDelegate(CacheRetrievalDelegate del)
            {
                Retrieve = del;
            }

            private int current_ite
            {
                get { return SimcraftImpl.iterationCounter; }
            }

            private int _ite = -1;
            private object _value;

            public object Value
            {
                get
                {
                    if (IsInvalid)
                    {
                        _ite = current_ite;
                        _value = Retrieve();
                    }

                    return _value;
                }
            }

            public MagicValueType GetValue()
            {
                var val = Value;
                if (val is Decimal) return new MagicValueType(Convert.ToDecimal(val));
                if (val is bool) return new MagicValueType((bool) val);
                if (val is int) return new MagicValueType(Convert.ToDecimal(val));
                if (val is double) return new MagicValueType(Convert.ToDecimal(val));
                if (val is MagicValueType) return (MagicValueType) val;
                throw new ConstraintException("Unsupported Caching Types");
            }

            private bool IsInvalid
            {
                get { return _value == null || (_ite + CacheInterval < current_ite || _ite > current_ite); }
            }
        }
    }


    public class DynamicItemCollection<T> : ObservableCollection<T>, IList, ITypedList
        where T : DynamicObject
    {
        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return null;
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            var dynamicDescriptors = new PropertyDescriptor[0];
            if (this.Any())
            {
                var firstItem = this[0];

                dynamicDescriptors =
                    firstItem.GetDynamicMemberNames()
                    .Select(p => new DynamicPropertyDescriptor(p))
                    .Cast<PropertyDescriptor>()
                    .ToArray();
            }

            return new PropertyDescriptorCollection(dynamicDescriptors);
        }
    }

    public class DynamicPropertyDescriptor : PropertyDescriptor
    {
        public DynamicPropertyDescriptor(string name)
            : base(name, null)
        {
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return GetDynamicMember(component, Name);
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            SetDynamicMember(component, Name, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return typeof(object); }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return typeof(object); }
        }

        private static void SetDynamicMember(object obj, string memberName, object value)
        {

            var binder = Binder.SetMember(
                CSharpBinderFlags.None,
                memberName,
                obj.GetType(),
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
            var callsite = CallSite<Action<CallSite, object, object>>.Create(binder);
            callsite.Target(callsite, obj, value);
        }

        private static object GetDynamicMember(object obj, string memberName)
        {
            var binder = Binder.GetMember(
                CSharpBinderFlags.None,
                memberName,
                obj.GetType(),
                new[]
            {
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, obj);
        }


    }



}


