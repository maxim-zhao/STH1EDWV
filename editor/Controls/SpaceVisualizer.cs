using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace sth1edwv.Controls
{
    public sealed partial class SpaceVisualizer : UserControl
    {
        private FreeSpace _space;
        private FreeSpace _initialSpace;

        public SpaceVisualizer()
        {
            DoubleBuffered = true;
            InitializeComponent();
        }

        public void SetData(FreeSpace space, FreeSpace initialSpace)
        {
            _space = space;
            _initialSpace = initialSpace;
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Invalidate();
            base.OnSizeChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_space == null)
            {
                base.OnPaint(e);
                return;
            }
            
            const int padding = 1;
            const int banksGap = 1;

            // Clear the background to the "control" colour
            e.Graphics.Clear(SystemColors.Control);

            // Draw some text on the top
            var freeBytes = _space.Spans.Sum(x => x.Size);
            var text = $"{freeBytes} bytes ({freeBytes/1024.0:F1} KB) free of {_space.Maximum / 1024} KB";
            e.Graphics.DrawString(text, Font, SystemBrushes.ControlText, new RectangleF(padding, 0, Width - padding * 2, Height), new StringFormat {Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center});
            var textWidth = (int)e.Graphics.MeasureString(text, Font).Width + padding;

            // Fill the background
            e.Graphics.FillRectangle(SystemBrushes.ControlDarkDark, new Rectangle(padding + textWidth, padding, Width - textWidth - padding * 2, Height - padding * 2));

            // The pixel width of the area we want to fill inside this
            var totalWidth = Width - textWidth - padding * 2 - 2;
            const int top = padding + 1;
            var left = textWidth + padding + 1;
            var height = Height - padding * 2 - 2;
            var banks = _space.Maximum / (16 * 1024);
            var pixelsPerByte = (float)(totalWidth - (banks - 1) * banksGap) / _space.Maximum;

            int BankStart(int bank)
            {
                return left + banksGap * bank + (int)(bank * 0x4000 * pixelsPerByte);
            }

            for (var bank = 0; bank < banks; ++bank)
            {
                // Get the bank area
                var bankLeft = BankStart(bank);
                var bankWidth = BankStart(bank + 1) - bankLeft - banksGap;
                var offset = bank * 0x4000;
                var limit = offset + 0x4000;
                var dotsPerByte = (double)bankWidth * height / 0x4000;

                void FillSpan(int start, int end, Brush brush)
                {
                    // Convert to relative offsets...
                    start -= offset;
                    end -= offset;
                    // We convert to "dots" in the current bank...
                    start = (int)(start * dotsPerByte);
                    end = (int)(end * dotsPerByte);
                    // We require at least one pixel...
                    if (end == start)
                    {
                        ++end;
                    }
                    // We convert to x,y pairs
                    var startX = start / height;
                    var startY = start % height;
                    var endX = end / height;
                    var endY = end % height;
                    if (startX == endX)
                    {
                        // Less than one line -> draw the bit we have
                        e.Graphics.FillRectangle(brush, bankLeft + startX, top + startY, 1, endY - startY);
                    }
                    else
                    {
                        // We draw the ends...
                        e.Graphics.FillRectangle(brush, bankLeft + startX, top + startY, 1, height - startY);
                        e.Graphics.FillRectangle(brush, bankLeft + endX, top, 1, endY);
                        // And the middle...
                        if (endX - startX > 1)
                        {
                            e.Graphics.FillRectangle(brush, bankLeft + startX + 1, top, endX - startX - 1, height);
                        }
                    }
                }

                // Draw it in grey first
                e.Graphics.FillRectangle(SystemBrushes.ControlDark, bankLeft, top, bankWidth, height);
                // Then draw "initial free space" over it in the "consumed" colour
                foreach (var span in _initialSpace.Spans.Where(x => x.End > offset && x.Start < limit))
                {
                    FillSpan(Math.Max(offset, span.Start), Math.Min(limit, span.End), SystemBrushes.Highlight);
                }
                // And then "remaining free space" on the top. (This is kind of inefficient...)
                foreach (var span in _space.Spans.Where(x => x.End > offset && x.Start < limit))
                {
                    FillSpan(Math.Max(offset, span.Start), Math.Min(limit, span.End), SystemBrushes.Control);
                }
            }
        }
    }
}
