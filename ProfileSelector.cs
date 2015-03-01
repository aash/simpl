using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.WoWInternals;

namespace Simcraft
{
    public partial class ProfileSelector : Form
    {
        public ProfileSelector()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            listBox1.Items.Clear();
            SimcraftImpl.GenerateApls(SimcraftImpl.SimcraftProfilePath);
            
            /*if (Directory.Exists(@"Bots\Simcraft\Trunk\"))
                
            else
                SimcraftImpl.GenerateApls(@"Bots\Simcraft\Profiles\");*/
            foreach (var apl in SimcraftImpl.apls )
            {
                    //if (apl.Key.Match(StyxWoW.Me.Class, StyxWoW.Me.Specialization, WoWContext.PvE))
                    //{
                if (apl.Value.Class == StyxWoW.Me.Class) listBox1.Items.Add(apl.Value);
                        //SimcraftImpl.Write("Selecting: " + apl.Value.Name);
                        //apl.Value.CreateBehavior();
                        //return;
                    //}
            }
            //SimcraftImpl.apls
            base.OnShown(e);
        }

        /*protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }*/

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
           var a = (ActionPrioriyList) listBox1.SelectedItem;
           a.CreateBehavior();

           SimcraftImpl.current_action_list = a;
           Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SimcraftImpl.GenerateApls(SimcraftImpl.SimcraftProfilePath);
            listBox1.Items.Clear();

            foreach (var apl in SimcraftImpl.apls)
            {
                //if (apl.Key.Match(StyxWoW.Me.Class, StyxWoW.Me.Specialization, WoWContext.PvE))
                //{
                if (apl.Value.Class == StyxWoW.Me.Class) listBox1.Items.Add(apl.Value);
                //SimcraftImpl.Write("Selecting: " + apl.Value.Name);
                //apl.Value.CreateBehavior();
                //return;
                //}
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SimcraftImpl.Superlog = checkBox1.Checked;
        }
    }
}
