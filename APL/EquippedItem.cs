using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcraft
{
    public class EquippedItem : AplExpr
    {
        public EquippedItem(String slot, String name, String id)
        {
            this.slot = slot;
            this.name = name;
            this.id = Convert.ToUInt32(id);
        }
        public String slot;
        public String name;
        public uint id;
    }
}
