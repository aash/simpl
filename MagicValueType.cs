using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx.Common;

namespace Simcraft
{
    public struct MagicValueType
    {
        public override string ToString()
        {
            return boxee + " as bool: " + (boxee > 0);
        }

        public bool Equals(MagicValueType other)
        {
            return boxee.Equals(other.boxee);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MagicValueType)) return false;
            var d = (MagicValueType)obj;
            return d.boxee == boxee;
        }

        public Decimal nat
        {
            get { return boxee; }
        }

        public MagicValueType max
        {
            get
            {
                return this;
            }
        }

        public override int GetHashCode()
        {
            return boxee.GetHashCode();
        }

        public MagicValueType(bool v)
        {
            boxee = (v ? 1 : 0);
        }

        public MagicValueType(Decimal v)
        {
            boxee = v;
        }

        public MagicValueType(int v)
        {
            boxee = v;
        }

        public MagicValueType(double v)
        {
            boxee = (Decimal)v;
        }

        public MagicValueType(MagicValueType v)
        {
            boxee = v.boxee;
        }


        private Decimal boxee;


        //MagicValueType Operators - Both Magic Double
        public static MagicValueType operator <(MagicValueType op1, MagicValueType op2)
        {
            //SimcraftImpl.LogDebug(op1 + " < " + op2);
            return new MagicValueType(op1.boxee < op2.boxee);
        }

        public static MagicValueType operator >(MagicValueType op1, MagicValueType op2)
        {
            //SimcraftImpl.LogDebug(op1+" > "+op2);
            return new MagicValueType(op1.boxee > op2.boxee);
        }

        public static MagicValueType operator <=(MagicValueType op1, MagicValueType op2)
        {
            return new MagicValueType(op1.boxee <= op2.boxee);
        }

        public static MagicValueType operator >=(MagicValueType op1, MagicValueType op2)
        {
            return new MagicValueType(op1.boxee >= op2.boxee);
        }

        public static MagicValueType operator !=(MagicValueType op1, MagicValueType op2)
        {
            return new MagicValueType(op1.boxee != op2.boxee);
        }

        public static MagicValueType operator ==(MagicValueType op1, MagicValueType op2)
        {
            return new MagicValueType(op1.boxee == op2.boxee);
        }

        public static MagicValueType operator -(MagicValueType op1, MagicValueType op2)
        {
            return new MagicValueType(op1.boxee - op2.boxee);
        }

        public static MagicValueType operator +(MagicValueType op1, MagicValueType op2)
        {
            return new MagicValueType(op1.boxee + op2.boxee);
        }

        public static MagicValueType operator *(MagicValueType op1, MagicValueType op2)
        {
            return new MagicValueType(op1.boxee * op2.boxee);
        }

        public static MagicValueType operator /(MagicValueType op1, MagicValueType op2)
        {
            return new MagicValueType(op1.boxee / op2.boxee);
        }

        public static MagicValueType operator %(MagicValueType op1, MagicValueType op2)
        {
            return new MagicValueType(op1.boxee % op2.boxee);
        }

        //MagicValueType Operators - Magic , normal
        public static MagicValueType operator <(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee < (Decimal)op2);
        }

        public static MagicValueType operator >(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee > (Decimal)op2);
        }

        public static MagicValueType operator <=(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee <= (Decimal)op2);
        }

        public static MagicValueType operator >=(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee >= (Decimal)op2);
        }

        public static MagicValueType operator !=(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee != (Decimal)op2);
        }

        public static MagicValueType operator ==(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee == (Decimal)op2);
        }

        public static MagicValueType operator -(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee - (Decimal)op2);
        }

        public static MagicValueType operator +(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee + (Decimal)op2);
        }

        public static MagicValueType operator *(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee * (Decimal)op2);
        }

        public static MagicValueType operator /(MagicValueType op1, double op2)
        {
            return new MagicValueType(op1.boxee / (Decimal)op2);
        }

        //MagicValueType Operators - Normal, Magic
        public static MagicValueType operator <(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 < op2.boxee);
        }

        public static MagicValueType operator >(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 > op2.boxee);
        }

        public static MagicValueType operator <=(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 <= op2.boxee);
        }

        public static MagicValueType operator >=(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 >= op2.boxee);
        }

        public static MagicValueType operator !=(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 != op2.boxee);
        }

        public static MagicValueType operator ==(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 == op2.boxee);
        }

        public static MagicValueType operator -(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 - op2.boxee);
        }

        public static MagicValueType operator +(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 + op2.boxee);
        }

        public static MagicValueType operator *(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 * op2.boxee);
        }

        public static MagicValueType operator /(double op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 / op2.boxee);
        }

        //MagicValueType Operators - Magic , normal
        public static MagicValueType operator <(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee < (Decimal)op2);
        }

        public static MagicValueType operator >(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee > (Decimal)op2);
        }

        public static MagicValueType operator <=(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee <= (Decimal)op2);
        }

        public static MagicValueType operator >=(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee >= (Decimal)op2);
        }

        public static MagicValueType operator !=(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee != (Decimal)op2);
        }

        public static MagicValueType operator ==(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee == (Decimal)op2);
        }

        public static MagicValueType operator -(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee - (Decimal)op2);
        }

        public static MagicValueType operator +(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee + (Decimal)op2);
        }

        public static MagicValueType operator *(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee * (Decimal)op2);
        }

        public static MagicValueType operator /(MagicValueType op1, int op2)
        {
            return new MagicValueType(op1.boxee / (Decimal)op2);
        }

        //MagicValueType Operators - Normal, Magic
        public static MagicValueType operator <(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 < op2.boxee);
        }

        public static MagicValueType operator >(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 > op2.boxee);
        }

        public static MagicValueType operator <=(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 <= op2.boxee);
        }

        public static MagicValueType operator >=(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 >= op2.boxee);
        }

        public static MagicValueType operator !=(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 != op2.boxee);
        }

        public static MagicValueType operator ==(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 == op2.boxee);
        }

        public static MagicValueType operator -(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 - op2.boxee);
        }

        public static MagicValueType operator +(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 + op2.boxee);
        }

        public static MagicValueType operator *(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 * op2.boxee);
        }

        public static MagicValueType operator /(int op1, MagicValueType op2)
        {
            return new MagicValueType((Decimal)op1 / op2.boxee);
        }

        //Bool Tricks

        //MagicValueType Operators - Magic , bool
        public static MagicValueType operator <(MagicValueType op1, bool op2)
        {
            return new MagicValueType(op1.boxee < (op2 ? 1 : 0));
        }

        public static MagicValueType operator >(MagicValueType op1, bool op2)
        {
            return new MagicValueType(op1.boxee > (op2 ? 1 : 0));
        }

        public static MagicValueType operator <=(MagicValueType op1, bool op2)
        {
            return new MagicValueType(op1.boxee <= (op2 ? 1 : 0));
        }

        public static MagicValueType operator >=(MagicValueType op1, bool op2)
        {
            return new MagicValueType(op1.boxee >= (op2 ? 1 : 0));
        }

        public static MagicValueType operator !=(MagicValueType op1, bool op2)
        {
            return !(op1 == op2);
        }

        public static MagicValueType operator ==(MagicValueType op1, bool op2)
        {
            if (op2 && op1 > 0) return new MagicValueType(true);
            if (!op2 && op1 > 0) return new MagicValueType(false);
            if (!op2 && op1 <= 0) return new MagicValueType(true);
            return new MagicValueType(false);
        }

        public static MagicValueType operator -(MagicValueType op1, bool op2)
        {
            return new MagicValueType(op1.boxee - (op2 ? 1 : 0));
        }

        public static MagicValueType operator +(MagicValueType op1, bool op2)
        {
            return new MagicValueType(op1.boxee + (op2 ? 1 : 0));
        }

        public static MagicValueType operator *(MagicValueType op1, bool op2)
        {
            return new MagicValueType(op1.boxee * (op2 ? 1 : 0));
        }


        //MagicValueType Operators - Normal, Magic
        public static MagicValueType operator <(bool op1, MagicValueType op2)
        {
            return new MagicValueType((op1 ? 1 : 0) < op2.boxee);
        }

        public static MagicValueType operator >(bool op1, MagicValueType op2)
        {
            return new MagicValueType((op1 ? 1 : 0) > op2.boxee);
        }

        public static MagicValueType operator <=(bool op1, MagicValueType op2)
        {
            return new MagicValueType((op1 ? 1 : 0) <= op2.boxee);
        }

        public static MagicValueType operator >=(bool op1, MagicValueType op2)
        {
            return new MagicValueType((op1 ? 1 : 0) >= op2.boxee);
        }

        public static MagicValueType operator !=(bool op1, MagicValueType op2)
        {
            return !(op1 == op2);
        }

        public static MagicValueType operator ==(bool op1, MagicValueType op2)
        {
            if (op1 && op2 > 0) return new MagicValueType(true);
            if (!op1 && op2 > 0) return new MagicValueType(false);
            if (!op1 && op2 <= 0) return new MagicValueType(true);
            return new MagicValueType(false);
        }

        public static MagicValueType operator -(bool op1, MagicValueType op2)
        {
            return new MagicValueType((op1 ? 1 : 0) - op2.boxee);
        }

        public static MagicValueType operator +(bool op1, MagicValueType op2)
        {
            return new MagicValueType((op1 ? 1 : 0) + op2.boxee);
        }

        public static MagicValueType operator *(bool op1, MagicValueType op2)
        {
            return new MagicValueType((op1 ? 1 : 0) * op2.boxee);
        }



        //Implicit Conversions
        public static implicit operator bool(MagicValueType d)
        {
            return d.boxee > 0;
        }

        /*public static implicit operator MagicValueType(bool d)
        {
            return new MagicValueType(d);
        }*/

        public static implicit operator double(MagicValueType d)
        {
            return (double)d.boxee;
        }

        public static implicit operator int(MagicValueType d)
        {
            return (int)d.boxee;
        }

        public static MagicValueType operator !(MagicValueType d)
        {
            //var _d = -d.boxee;
            return new MagicValueType(d.boxee > 0 ? 0 : 1);
        }

    }
}
