﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace sth1edwv
{
    public partial class BlockChooser : Form
    {
        public int SelectedBlockIndex { get; set; }
        private readonly Level _level;

        public BlockChooser(Level level)
        {
            _level = level;
            InitializeComponent();
        }

        private void FormLoad(object sender, EventArgs e)
        {
            var bmp = new Bitmap(528, 528);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                for (var i = 0; i < _level.BlockMapping.Blocks.Count; ++i)
                {
                    var x = i % 16;
                    var y = i / 16;
                    g.DrawImageUnscaled(_level.BlockMapping.Blocks[i].Image, x*33, y*33);
                }
            }

            pb1.Image = bmp;
        }

        private void pb1_MouseClick(object sender, MouseEventArgs e)
        {
            var x = e.X / 33;
            var y = e.Y / 33;
            var index = x + y * 16;
            if (index < _level.BlockMapping.Blocks.Count)
            {
                SelectedBlockIndex = index;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}