/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/

    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;
using System;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace NAPS2.WinForms
{
    partial class FContrast : FormBase
    {
        private readonly ChangeTracker changeTracker;

        private Bitmap workingImage;
        private bool previewOutOfDate;
        private bool working;
        private Timer previewTimer;

        public FContrast(ChangeTracker changeTracker)
        {
            this.changeTracker = changeTracker;
            InitializeComponent();

            ContrastTransform = new ContrastTransform();
        }

        public IScannedImage Image { get; set; }

        public ContrastTransform ContrastTransform { get; private set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(tbContrast, pictureBox)
                    .WidthToForm()
                .Bind(pictureBox)
                    .HeightToForm()
                .Bind(btnOK, btnCancel, txtContrast)
                    .RightToForm()
                .Bind(tbContrast, txtContrast, btnRevert, btnOK, btnCancel)
                    .BottomToForm()
                .Activate();
            Size = new Size(600, 600);

            workingImage = Image.GetImage();
            pictureBox.Image = (Bitmap)workingImage.Clone();
            UpdatePreviewBox();
        }

        private void UpdateTransform()
        {
            ContrastTransform.Contrast = tbContrast.Value;
            UpdatePreviewBox();
        }

        private void UpdatePreviewBox()
        {
            if (previewTimer == null)
            {
                previewTimer = new Timer((obj) =>
                {
                    if (previewOutOfDate && !working)
                    {
                        working = true;
                        previewOutOfDate = false;
                        var result = ContrastTransform.Perform((Bitmap)workingImage.Clone());
                        Invoke(new MethodInvoker(() =>
                        {
                            if (pictureBox.Image != null)
                            {
                                pictureBox.Image.Dispose();
                            }
                            pictureBox.Image = result;
                        }));
                        working = false;
                    }
                }, null, 0, 100);
            }
            previewOutOfDate = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!ContrastTransform.IsNull)
            {
                Image.AddTransform(ContrastTransform);
                Image.SetThumbnail(Image.RenderThumbnail(UserConfigManager.Config.ThumbnailSize));
                changeTracker.HasUnsavedChanges = true;
            }
            Close();
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            ContrastTransform = new ContrastTransform();
            tbContrast.Value = 0;
            txtContrast.Text = tbContrast.Value.ToString("G");
            UpdatePreviewBox();
        }

        private void FCrop_FormClosed(object sender, FormClosedEventArgs e)
        {
            workingImage.Dispose();
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            if (previewTimer != null)
            {
                previewTimer.Dispose();
            }
        }

        private void txtContrast_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtContrast.Text, out value))
            {
                if (value >= tbContrast.Minimum && value <= tbContrast.Maximum)
                {
                    tbContrast.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbContrast_Scroll(object sender, EventArgs e)
        {
            txtContrast.Text = tbContrast.Value.ToString("G");
            UpdateTransform();
        }
    }
}