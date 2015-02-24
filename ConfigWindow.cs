using System;
using System.Windows.Forms;
using Styx;
using Styx.Common;

namespace Simcraft
{
    public partial class ConfigWindow : Form
    {
        public ConfigWindow()
        {
            InitializeComponent();

            fillKeyBox(cdKey);
            fillKeyBox(exKey);
            fillKeyBox(BKey);
            fillKeyBox(AoeKey);

            fillModBox(cdMod);
            fillModBox(bMod);
            fillModBox(exMod);
            fillModBox(aoeMod);

            cdKey.SelectedItem = SimCSettings.currentSettings.Cooldowns.key;
            exKey.SelectedItem = SimCSettings.currentSettings.Execution.key;
            BKey.SelectedItem = SimCSettings.currentSettings.Burst.key;
            AoeKey.SelectedItem = SimCSettings.currentSettings.Aoe.key;

            cdMod.SelectedItem = SimCSettings.currentSettings.Cooldowns.mod;
            exMod.SelectedItem = SimCSettings.currentSettings.Execution.mod;
            bMod.SelectedItem = SimCSettings.currentSettings.Burst.mod;
            aoeMod.SelectedItem = SimCSettings.currentSettings.Aoe.mod;
        }

        private void fillKeyBox(ComboBox b1)
        {
            var values = Enum.GetValues(typeof(Keys));
            foreach (var v in values)
            {
                b1.Items.Add(v);
            }
            /*b1.Items.Add(Keys.A);
            b1.Items.Add(Keys.B);
            b1.Items.Add(Keys.C);
            b1.Items.Add(Keys.D);
            b1.Items.Add(Keys.E);
            b1.Items.Add(Keys.F);
            b1.Items.Add(Keys.G);
            b1.Items.Add(Keys.H);
            b1.Items.Add(Keys.I);
            b1.Items.Add(Keys.J);
            b1.Items.Add(Keys.K);
            b1.Items.Add(Keys.L);
            b1.Items.Add(Keys.M);
            b1.Items.Add(Keys.N);
            b1.Items.Add(Keys.O);
            b1.Items.Add(Keys.P);
            b1.Items.Add(Keys.Q);
            b1.Items.Add(Keys.R);
            b1.Items.Add(Keys.S);
            b1.Items.Add(Keys.T);
            b1.Items.Add(Keys.U);
            b1.Items.Add(Keys.V);
            b1.Items.Add(Keys.W);
            b1.Items.Add(Keys.X);
            b1.Items.Add(Keys.Y);
            b1.Items.Add(Keys.Z);
            b1.Items.Add(Keys.D1);
            b1.Items.Add(Keys.D2);
            b1.Items.Add(Keys.D3);
            b1.Items.Add(Keys.D4);
            b1.Items.Add(Keys.D5);
            b1.Items.Add(Keys.D6);
            b1.Items.Add(Keys.D7);
            b1.Items.Add(Keys.D8);
            b1.Items.Add(Keys.D9);
            b1.Items.Add(Keys.);*/
        }

        private void fillModBox(ComboBox b1)
        {
            b1.Items.Add(Styx.Common.ModifierKeys.Alt);
            b1.Items.Add(Styx.Common.ModifierKeys.Control);
            b1.Items.Add(Styx.Common.ModifierKeys.Shift);
            b1.Items.Add(Styx.Common.ModifierKeys.NoRepeat);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SimCSettings.currentSettings.Cooldowns.key = (Keys) cdKey.SelectedItem;
            SimCSettings.currentSettings.Execution.key = (Keys) exKey.SelectedItem;
            SimCSettings.currentSettings.Burst.key = (Keys) BKey.SelectedItem;
            SimCSettings.currentSettings.Aoe.key = (Keys) AoeKey.SelectedItem;

            SimCSettings.currentSettings.Cooldowns.mod = (ModifierKeys) cdMod.SelectedItem;
            SimCSettings.currentSettings.Execution.mod = (ModifierKeys) exMod.SelectedItem;
            SimCSettings.currentSettings.Burst.mod = (ModifierKeys) bMod.SelectedItem;
            SimCSettings.currentSettings.Aoe.mod = (ModifierKeys) aoeMod.SelectedItem;

            SimCSettings.Save();
            SimcraftImpl.UnregisterHotkeys();
            SimcraftImpl.RegisterHotkeys();
            Close();
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SimcraftImpl.inst.Specialisation = WoWSpec.None;
            SimcraftImpl.inst.ContextChange(null, null);
        }
    }
}