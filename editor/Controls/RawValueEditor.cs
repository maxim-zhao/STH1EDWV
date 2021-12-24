using System;
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
                1 => value.Encoding switch
                {
                    RawValue.Encodings.Byte => 255,
                    RawValue.Encodings.Bcd => 99,
                    _ => throw new ArgumentOutOfRangeException()
                },
                2 => value.Encoding switch
                {
                    RawValue.Encodings.Byte => 65535,
                    RawValue.Encodings.Bcd => 9999,
                    _ => throw new ArgumentOutOfRangeException()
                },
                _ => throw new ArgumentOutOfRangeException()
            };
            numericUpDown1.Value = value.Encoding switch
            {
                RawValue.Encodings.Byte => value.Value,
                RawValue.Encodings.Bcd => Memory.FromBcd(value.Value),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void numericUpDown1_ValueChanged(object sender, System.EventArgs e)
        {
            _value.Value = _value.Encoding switch
            {
                RawValue.Encodings.Byte => (int)numericUpDown1.Value,
                RawValue.Encodings.Bcd => Memory.ToBcd((int)numericUpDown1.Value),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
