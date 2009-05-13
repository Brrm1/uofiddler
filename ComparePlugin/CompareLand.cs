﻿/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using Ultima;

namespace ComparePlugin
{
    public partial class CompareLand : UserControl
    {
        public CompareLand()
        {
            InitializeComponent();
        }
        Hashtable m_Compare = new Hashtable();
        SHA256Managed shaM = new SHA256Managed();

        private void OnLoad(object sender, EventArgs e)
        {
            listBoxOrg.Items.Clear();
            listBoxOrg.BeginUpdate();
            for (int i = 0; i < 0x4000; i++)
            {
                listBoxOrg.Items.Add(i);
            }
            listBoxOrg.EndUpdate();
        }

        private void OnIndexChangedOrg(object sender, EventArgs e)
        {
            if ((listBoxOrg.SelectedIndex == -1) || (listBoxOrg.Items.Count < 1))
                return;

            int i = int.Parse(listBoxOrg.Items[listBoxOrg.SelectedIndex].ToString());
            if (listBoxSec.Items.Count > 0)
                listBoxSec.SelectedIndex = listBoxSec.Items.IndexOf(i);
            if (Art.IsValidLand(i))
            {
                Bitmap bmp = Art.GetLand(i);
                if (bmp != null)
                    pictureBoxOrg.BackgroundImage = bmp;
                else
                    pictureBoxOrg.BackgroundImage = null;
            }
            else
                pictureBoxOrg.BackgroundImage = null;
            listBoxOrg.Refresh();
        }

        private void DrawitemOrg(object sender, DrawItemEventArgs e)
        {
            Brush fontBrush = Brushes.Gray;

            int i = int.Parse(listBoxOrg.Items[e.Index].ToString());
            if (listBoxOrg.SelectedIndex == e.Index)
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            if (!Art.IsValidLand(i))
                fontBrush = Brushes.Red;
            else if (listBoxSec.Items.Count > 0)
            {
                if (!Compare(i))
                    fontBrush = Brushes.Blue;
            }

            e.Graphics.DrawString(String.Format("0x{0:X}", i), Font, fontBrush,
                new PointF((float)5,
                e.Bounds.Y + ((e.Bounds.Height / 2) -
                (e.Graphics.MeasureString(String.Format("0x{0:X}", i), Font).Height / 2))));
        }

        private void MeasureOrg(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 13;
        }

        private void OnClickLoadSecond(object sender, EventArgs e)
        {
            if (textBoxSecondDir.Text == null)
                return;
            string path = textBoxSecondDir.Text;
            string file = Path.Combine(path, "art.mul");
            string file2 = Path.Combine(path, "artidx.mul");
            if ((File.Exists(file)) && (File.Exists(file2)))
            {
                SecondArt.SetFileIndex(file2, file);
                LoadSecond();
            }
        }

        private void LoadSecond()
        {
            listBoxSec.Items.Clear();
            listBoxSec.BeginUpdate();
            for (int i = 0; i < 0x4000; i++)
            {
                listBoxSec.Items.Add(i);
            }
            listBoxSec.EndUpdate();
        }

        private void DrawItemSec(object sender, DrawItemEventArgs e)
        {
            Brush fontBrush = Brushes.Gray;

            int i = int.Parse(listBoxOrg.Items[e.Index].ToString());
            if (listBoxSec.SelectedIndex == e.Index)
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            if (!SecondArt.IsValidLand(i))
                fontBrush = Brushes.Red;
            else if (!Compare(i))
                fontBrush = Brushes.Blue;

            e.Graphics.DrawString(String.Format("0x{0:X}", i), Font, fontBrush,
                new PointF((float)5,
                e.Bounds.Y + ((e.Bounds.Height / 2) -
                (e.Graphics.MeasureString(String.Format("0x{0:X}", i), Font).Height / 2))));
        }

        private void MeasureSec(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 13;
        }

        private void OnIndexChangedSec(object sender, EventArgs e)
        {
            if ((listBoxSec.SelectedIndex == -1) || (listBoxSec.Items.Count < 1))
                return;

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            listBoxOrg.SelectedIndex = listBoxOrg.Items.IndexOf(i);
            if (SecondArt.IsValidLand(i))
            {
                Bitmap bmp = SecondArt.GetLand(i);
                if (bmp != null)
                    pictureBoxSec.BackgroundImage = bmp;
                else
                    pictureBoxSec.BackgroundImage = null;
            }
            else
                pictureBoxSec.BackgroundImage = null;
            listBoxSec.Refresh();
        }

        private bool Compare(int index)
        {
            if (m_Compare.Contains(index))
                return (bool)m_Compare[index];
            byte[] org = Art.GetRawLand(index);
            byte[] sec = SecondArt.GetRawLand(index);
            if ((org == null) && (sec == null))
            {
                m_Compare[index] = true;
                return true;
            }
            if (((org == null) || (sec == null))
                || (org.Length != sec.Length))
            {
                m_Compare[index] = false;
                return false;
            }

            string hash1string = BitConverter.ToString(shaM.ComputeHash(org));
            string hash2string = BitConverter.ToString(shaM.ComputeHash(sec));
            bool res;
            if (hash1string != hash2string)
                res = false;
            else
                res = true;

            m_Compare[index] = res;
            return res;
        }

        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            if (m_Compare.Count < 1)
            {
                if (checkBox1.Checked)
                {
                    MessageBox.Show("Second Land file is not loaded!");
                    checkBox1.Checked = false;
                }
                return;
            }

            listBoxOrg.BeginUpdate();
            listBoxSec.BeginUpdate();
            if (checkBox1.Checked)
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    if (Compare(i))
                    {
                        listBoxOrg.Items.Remove(i);
                        listBoxSec.Items.Remove(i);
                    }
                }
            }
            else
            {
                listBoxOrg.Items.Clear();
                listBoxSec.Items.Clear();
                for (int i = 0; i < 0x4000; i++)
                {
                    listBoxOrg.Items.Add(i);
                    listBoxSec.Items.Add(i);
                }
            }
            listBoxOrg.EndUpdate();
            listBoxSec.EndUpdate();
        }

        private void ExportAsBmp(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
                return;
            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondArt.IsValidLand(i))
                return;
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, String.Format("Landtile(Sec) 0x{0:X}.bmp", i));
            SecondArt.GetLand(i).Save(FileName, ImageFormat.Bmp);
            MessageBox.Show(
                String.Format("Landtile saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void ExportAsTiff(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
                return;
            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondArt.IsValidLand(i))
                return;
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, String.Format("Landtile(Sec) 0x{0:X}.tiff", i));
            SecondArt.GetLand(i).Save(FileName, ImageFormat.Tiff);
            MessageBox.Show(
                String.Format("Landtile saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
    }
}
