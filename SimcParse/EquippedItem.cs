using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimcParse
{
    public class EquippedItem
    {
        public EquippedItem(String slot, String name, String id)
        {
            this.slot = slot;
            this.name = name;
            this.id = Convert.ToInt32(id);
        }
        public String slot;
        public String name;
        public int id;
    }
}
