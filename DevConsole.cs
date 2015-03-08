using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Navigation;
using Bots.DungeonBuddy.Helpers;
using JetBrains.Annotations;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Helpers;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace Simcraft
{
    public partial class DevConsole : Form
    {

        public void reset()
        {
            try
            {
                DebuffGrid.DataSource = SimcraftImpl.inst.debuff.AsList;
                BuffGrid.DataSource = SimcraftImpl.inst.buff.AsList;


            }
            catch (Exception e)
            {

            }
        }

        public DevConsole()
        {
            InitializeComponent();
            DebuffGrid.AutoGenerateColumns = true;


        }

        private void DevConsole_Load(object sender, EventArgs e)
        {
            //Logging.Write("Load");
            timer1.Start();
        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            //Logging.Write("TICKSS");
            DebuffGrid.DataSource = SimcraftImpl.inst.debuff.AsList;
        }

        private void DevConsole_Paint(object sender, PaintEventArgs e)
        {
          
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void DebuffGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void BuffGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }
    }
}
