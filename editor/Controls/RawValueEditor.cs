using System.Windows.Forms;
using sth1edwv.GameObjects;

namespace sth1edwv.Controls
{
    public partial class RawValueEditor : UserControl
    {
        private readonly RawValue _value;

        public RawValueEditor(RawValue value)
        {
            _value = value;
            InitializeComponent();

            numericUpDown1.Minimum = 0;
            numericUpDown1.Maximum = value.Size switch
            {
                1 => 255,
                2 => 65535,
                _ => numericUpDown1.Maximum
            };
            numericUpDown1.Value = value.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, System.EventArgs e)
        {
            _value.Value = (int)numericUpDown1.Value;
        }
    }
}
