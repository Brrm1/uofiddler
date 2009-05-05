/***************************************************************************
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
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class TileDatas : UserControl
    {
        public TileDatas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            InitializeComponent();
            checkedListBox1.BeginUpdate();
            checkedListBox1.Items.Clear();
            string[] EnumNames = System.Enum.GetNames(typeof(TileFlag));
            for (int i = 1; i < EnumNames.Length; ++i)
            {
                checkedListBox1.Items.Add(EnumNames[i], false);
            }
            checkedListBox1.EndUpdate();
            checkedListBox2.BeginUpdate();
            checkedListBox2.Items.Clear();
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Damaging), false);
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Wet), false);
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Impassable), false);
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Wall), false);
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Unknown3), false);
            checkedListBox2.EndUpdate();
            refMarker = this;
        }

        private static TileDatas refMarker = null;
        private bool ChangingIndex = false;


        public static void Select(int graphic, bool land)
        {
            SearchGraphic(graphic, land);
        }
        public static bool SearchGraphic(int graphic, bool land)
        {
            int index = 0;
            if (land)
            {
                for (int i = index; i < refMarker.treeViewLand.Nodes.Count; i++)
                {
                    TreeNode node = refMarker.treeViewLand.Nodes[i];
                    if ((int)node.Tag == graphic)
                    {
                        refMarker.tabcontrol.SelectTab(1);
                        refMarker.treeViewLand.SelectedNode = node;
                        node.EnsureVisible();
                        return true;
                    }
                }
            }
            else
            {
                for (int i = index; i < refMarker.treeViewItem.Nodes.Count; i++)
                {
                    TreeNode node = refMarker.treeViewItem.Nodes[i];
                    if ((int)node.Tag == graphic)
                    {
                        refMarker.tabcontrol.SelectTab(0);
                        refMarker.treeViewItem.SelectedNode = node;
                        node.EnsureVisible();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool SearchName(string name, bool next, bool land)
        {
            int index = 0;
            Regex regex = new Regex(@name, RegexOptions.IgnoreCase);
            if (land)
            {
                if (next)
                {
                    if (refMarker.treeViewLand.SelectedNode.Index >= 0)
                        index = refMarker.treeViewLand.SelectedNode.Index + 1;
                    if (index >= refMarker.treeViewLand.Nodes.Count)
                        index = 0;
                }
                for (int i = index; i < refMarker.treeViewLand.Nodes.Count; i++)
                {
                    TreeNode node = refMarker.treeViewLand.Nodes[i];
                    if (regex.IsMatch(TileData.LandTable[(int)node.Tag].Name))
                    {
                        refMarker.tabcontrol.SelectTab(1);
                        refMarker.treeViewLand.SelectedNode = node;
                        node.EnsureVisible();
                        return true;
                    }
                }
            }
            else
            {
                if (next)
                {
                    if (refMarker.treeViewItem.SelectedNode.Index >= 0)
                        index = refMarker.treeViewItem.SelectedNode.Index + 1;
                    if (index >= refMarker.treeViewItem.Nodes.Count)
                        index = 0;
                }
                for (int i = index; i < refMarker.treeViewItem.Nodes.Count; i++)
                {
                    TreeNode node = refMarker.treeViewItem.Nodes[i];
                    if (regex.IsMatch(TileData.ItemTable[(int)node.Tag].Name))
                    {
                        refMarker.tabcontrol.SelectTab(0);
                        refMarker.treeViewItem.SelectedNode = node;
                        node.EnsureVisible();
                        return true;
                    }
                }
            }
            return false;
        }

        private bool Loaded = false;
        private void Reload()
        {
            if (Loaded)
                OnLoad(this, EventArgs.Empty);
        }
        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            treeViewItem.BeginUpdate();
            treeViewItem.Nodes.Clear();
            if (TileData.ItemTable != null)
            {
                for (int i = 0; i < TileData.ItemTable.Length; ++i)
                {
                    TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name));
                    node.Tag = i;
                    treeViewItem.Nodes.Add(node);
                }
            }
            treeViewItem.EndUpdate();
            treeViewLand.BeginUpdate();
            treeViewLand.Nodes.Clear();
            if (TileData.LandTable != null)
            {
                for (int i = 0; i < TileData.LandTable.Length; ++i)
                {
                    TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name));
                    node.Tag = i;
                    treeViewLand.Nodes.Add(node);
                }
            }
            treeViewLand.EndUpdate();
            if (!Loaded)
                FiddlerControls.Options.FilePathChangeEvent += new FiddlerControls.Options.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void AfterSelectTreeViewItem(object sender, TreeViewEventArgs e)
        {
            int index = (int)e.Node.Tag;
            try
            {
                Bitmap bit = Ultima.Art.GetStatic(index);
                Bitmap newbit = new Bitmap(pictureBoxItem.Size.Width, pictureBoxItem.Size.Height);
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.Clear(Color.FromArgb(-1));
                newgraph.DrawImage(bit, (pictureBoxItem.Size.Width - bit.Width) / 2, 1);
                pictureBoxItem.Image = newbit;
            }
            catch
            {
                pictureBoxItem.Image = new Bitmap(pictureBoxItem.Width, pictureBoxItem.Height);
            }
            ItemData data = TileData.ItemTable[index];
            ChangingIndex = true;
            textBoxName.Text = data.Name;
            textBoxAnim.Text = data.Animation.ToString();
            textBoxWeight.Text = data.Weight.ToString();
            textBoxQuality.Text = data.Quality.ToString();
            textBoxQuantity.Text = data.Quantity.ToString();
            textBoxHue.Text = data.Hue.ToString();
            textBoxStackOff.Text = data.StackingOffset.ToString();
            textBoxValue.Text = data.Value.ToString();
            textBoxHeigth.Text = data.Height.ToString();
            textBoxUnk1.Text = data.Unk1.ToString();
            textBoxUnk2.Text = data.Unk2.ToString();
            textBoxUnk3.Text = data.Unk3.ToString();
            Array EnumValues = System.Enum.GetValues(typeof(TileFlag));
            for (int i = 1; i < EnumValues.Length; i++)
            {
                if ((data.Flags & (TileFlag)EnumValues.GetValue(i)) != 0)
                    checkedListBox1.SetItemChecked(i - 1, true);
                else
                    checkedListBox1.SetItemChecked(i - 1, false);
            }
            ChangingIndex = false;
        }

        private void AfterSelectTreeViewLand(object sender, TreeViewEventArgs e)
        {
            int index = (int)e.Node.Tag;
            try
            {
                Bitmap bit = Ultima.Art.GetLand(index);
                Bitmap newbit = new Bitmap(pictureBoxLand.Size.Width, pictureBoxLand.Size.Height);
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.Clear(Color.FromArgb(-1));
                newgraph.DrawImage(bit, (pictureBoxLand.Size.Width - bit.Width) / 2, 1);
                pictureBoxLand.Image = newbit;
            }
            catch
            {
                pictureBoxLand.Image = new Bitmap(pictureBoxLand.Width, pictureBoxLand.Height);
            }
            LandData data = TileData.LandTable[index];
            textBoxNameLand.Text = data.Name;
            textBoxTexID.Text = data.TextureID.ToString();
            ChangingIndex = true;
            if ((data.Flags & TileFlag.Damaging) != 0)
                checkedListBox2.SetItemChecked(0, true);
            else
                checkedListBox2.SetItemChecked(0, false);
            if ((data.Flags & TileFlag.Wet) != 0)
                checkedListBox2.SetItemChecked(1, true);
            else
                checkedListBox2.SetItemChecked(1, false);
            if ((data.Flags & TileFlag.Impassable) != 0)
                checkedListBox2.SetItemChecked(2, true);
            else
                checkedListBox2.SetItemChecked(2, false);
            if ((data.Flags & TileFlag.Wall) != 0)
                checkedListBox2.SetItemChecked(3, true);
            else
                checkedListBox2.SetItemChecked(3, false);
            if ((data.Flags & TileFlag.Unknown3) != 0)
                checkedListBox2.SetItemChecked(4, true);
            else
                checkedListBox2.SetItemChecked(4, false);
            ChangingIndex = false;
        }

        private void OnClickSaveTiledata(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, "tiledata.mul");
            Ultima.TileData.SaveTileData(FileName);
            MessageBox.Show(
                String.Format("TileData saved to {0}", FileName),
                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["TileData"] = false;
        }

        private void OnClickSaveChanges(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex == 0) //items
            {
                if (treeViewItem.SelectedNode == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                string name = textBoxName.Text;
                if (name.Length > 20)
                    name = name.Substring(0, 20);
                item.Name = name;
                treeViewItem.SelectedNode.Text = String.Format("0x{0:X4} ({0}) {1}", index, name);
                byte byteres;
                short shortres;
                if (short.TryParse(textBoxAnim.Text, out shortres))
                    item.Animation = shortres;
                if (byte.TryParse(textBoxWeight.Text, out byteres))
                    item.Weight = byteres;
                if (byte.TryParse(textBoxQuality.Text, out byteres))
                    item.Quality = byteres;
                if (byte.TryParse(textBoxQuantity.Text, out byteres))
                    item.Quantity = byteres;
                if (byte.TryParse(textBoxHue.Text, out byteres))
                    item.Hue = byteres;
                if (byte.TryParse(textBoxStackOff.Text, out byteres))
                    item.StackingOffset = byteres;
                if (byte.TryParse(textBoxValue.Text, out byteres))
                    item.Value = byteres;
                if (byte.TryParse(textBoxHeigth.Text, out byteres))
                    item.Height = byteres;
                if (short.TryParse(textBoxUnk1.Text, out shortres))
                    item.Unk1 = shortres;
                if (byte.TryParse(textBoxUnk2.Text, out byteres))
                    item.Unk2 = byteres;
                if (byte.TryParse(textBoxUnk3.Text, out byteres))
                    item.Unk3 = byteres;
                item.Flags = TileFlag.None;
                Array EnumValues = System.Enum.GetValues(typeof(TileFlag));
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (checkedListBox1.GetItemChecked(i))
                        item.Flags |= (TileFlag)EnumValues.GetValue(i + 1);
                }
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                if (memorySaveWarningToolStripMenuItem.Checked)
                    MessageBox.Show(
                        String.Format("Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index), "Saved", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information, 
                        MessageBoxDefaultButton.Button1);
            }
            else //land
            {
                if (treeViewLand.SelectedNode == null)
                    return;
                int index = (int)treeViewLand.SelectedNode.Tag;
                LandData land = TileData.LandTable[index];
                string name = textBoxNameLand.Text;
                if (name.Length > 20)
                    name = name.Substring(0, 20);
                land.Name = name;
                treeViewLand.SelectedNode.Text = String.Format("0x{0:X4} {1}", index, name);
                short shortres;
                if (short.TryParse(textBoxTexID.Text, out shortres))
                    land.TextureID = shortres;
                land.Flags = TileFlag.None;
                if (checkedListBox2.GetItemChecked(0))
                    land.Flags |= TileFlag.Damaging;
                if (checkedListBox2.GetItemChecked(1))
                    land.Flags |= TileFlag.Wet;
                if (checkedListBox2.GetItemChecked(2))
                    land.Flags |= TileFlag.Impassable;
                if (checkedListBox2.GetItemChecked(3))
                    land.Flags |= TileFlag.Wall;
                if (checkedListBox2.GetItemChecked(4))
                    land.Flags |= TileFlag.Unknown3;

                TileData.LandTable[index] = land;
                Options.ChangedUltimaClass["TileData"] = true;
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                if (memorySaveWarningToolStripMenuItem.Checked)
                    MessageBox.Show(
                        String.Format("Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index), "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
            }
        }

        #region SaveDirectEvents
        private void OnFlagItemCheckItems(object sender, ItemCheckEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (e.CurrentValue != e.NewValue)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    Array EnumValues = System.Enum.GetValues(typeof(TileFlag));
                    TileFlag changeflag = (TileFlag)EnumValues.GetValue(e.Index + 1);
                    if ((item.Flags & changeflag) != 0) //better doublecheck
                    {
                        if (e.NewValue == CheckState.Unchecked)
                        {
                            item.Flags ^= changeflag;
                            TileData.ItemTable[index] = item;
                            treeViewItem.SelectedNode.ForeColor = Color.Red;
                            Options.ChangedUltimaClass["TileData"] = true;
                        }
                    }
                    else if ((item.Flags & changeflag) == 0)
                    {
                        if (e.NewValue == CheckState.Checked)
                        {
                            item.Flags |= changeflag;
                            TileData.ItemTable[index] = item;
                            treeViewItem.SelectedNode.ForeColor = Color.Red;
                            Options.ChangedUltimaClass["TileData"] = true;
                        }
                    }
                    
                }
            }
        }

        private void OnKeyDownItemName(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    string name = textBoxName.Text;
                    if (name.Length > 20)
                        name = name.Substring(0, 20);
                    item.Name = name;
                    treeViewItem.SelectedNode.Text = String.Format("0x{0:X4} ({0}) {1}", index, name);
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemAnim(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    short shortres;
                    if (short.TryParse(textBoxAnim.Text, out shortres))
                        item.Animation = shortres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemWeight(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    byte byteres;
                    if (byte.TryParse(textBoxWeight.Text, out byteres))
                        item.Weight = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemQuali(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    byte byteres;
                    if (byte.TryParse(textBoxQuality.Text, out byteres))
                        item.Quality = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemQuanti(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    byte byteres;
                    if (byte.TryParse(textBoxQuantity.Text, out byteres))
                        item.Quantity = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemHue(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    byte byteres;
                    if (byte.TryParse(textBoxHue.Text, out byteres))
                        item.Hue = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemStackOff(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    byte byteres;
                    if (byte.TryParse(textBoxStackOff.Text, out byteres))
                        item.StackingOffset = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemValue(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    byte byteres;
                    if (byte.TryParse(textBoxValue.Text, out byteres))
                        item.Value = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemHeigth(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    byte byteres;
                    if (byte.TryParse(textBoxHeigth.Text, out byteres))
                        item.Height = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemUnk1(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    short shortres;
                    if (short.TryParse(textBoxUnk1.Text, out shortres))
                        item.Unk1 = shortres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemUnk2(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    byte byteres;
                    if (byte.TryParse(textBoxUnk2.Text, out byteres))
                        item.Unk2 = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownItemUnk3(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewItem.SelectedNode == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    byte byteres;
                    if (byte.TryParse(textBoxUnk3.Text, out byteres))
                        item.Unk3 = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnFlagItemCheckLandtiles(object sender, ItemCheckEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (e.CurrentValue != e.NewValue)
                {
                    if (treeViewLand.SelectedNode == null)
                        return;
                    int index = (int)treeViewLand.SelectedNode.Tag;
                    LandData land = TileData.LandTable[index];
                    TileFlag changeflag;
                    switch (e.Index)
                    {
                        case 0: changeflag = TileFlag.Damaging;
                            break;
                        case 1: changeflag = TileFlag.Wet;
                            break;
                        case 2: changeflag = TileFlag.Impassable;
                            break;
                        case 3: changeflag = TileFlag.Wall;
                            break;
                        case 4: changeflag = TileFlag.Unknown3;
                            break;
                        default: changeflag = TileFlag.None;
                            break;
                    }

                    if ((land.Flags & changeflag) != 0)
                    {
                        if (e.NewValue == CheckState.Unchecked)
                        {
                            land.Flags ^= changeflag;
                            TileData.LandTable[index] = land;
                            treeViewLand.SelectedNode.ForeColor = Color.Red;
                            Options.ChangedUltimaClass["TileData"] = true;
                        }
                    }
                    else if ((land.Flags & changeflag) == 0)
                    {
                        if (e.NewValue == CheckState.Checked)
                        {
                            land.Flags |= changeflag;
                            TileData.LandTable[index] = land;
                            treeViewLand.SelectedNode.ForeColor = Color.Red;
                            Options.ChangedUltimaClass["TileData"] = true;
                        }
                    }
                }
            }
        }

        private void OnKeyDownLandName(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewLand.SelectedNode == null)
                        return;
                    int index = (int)treeViewLand.SelectedNode.Tag;
                    LandData land = TileData.LandTable[index];
                    string name = textBoxNameLand.Text;
                    if (name.Length > 20)
                        name = name.Substring(0, 20);
                    land.Name = name;
                    treeViewLand.SelectedNode.Text = String.Format("0x{0:X4} {1}", index, name);
                    TileData.LandTable[index] = land;
                    treeViewLand.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }

        private void OnKeyDownLandTexId(object sender, KeyEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (treeViewLand.SelectedNode == null)
                        return;
                    int index = (int)treeViewLand.SelectedNode.Tag;
                    LandData land = TileData.LandTable[index];
                    short shortres;
                    if (short.TryParse(textBoxTexID.Text, out shortres))
                        land.TextureID = shortres;
                    TileData.LandTable[index] = land;
                    treeViewLand.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                }
            }
        }
        #endregion


        private void OnClickExport(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (tabcontrol.SelectedIndex == 0) //items
            {
                string FileName = Path.Combine(path, "ItemData.csv");
                Ultima.TileData.ExportItemDataToCSV(FileName);
                MessageBox.Show(String.Format("ItemData saved to {0}", FileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                string FileName = Path.Combine(path, "LandData.csv");
                Ultima.TileData.ExportLandDataToCSV(FileName);
                MessageBox.Show(String.Format("LandData saved to {0}", FileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private TileDatasSearch showform1 = null;
        private TileDatasSearch showform2 = null;
        private void OnClickSearch(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex == 0) //items
            {
                if ((showform1 == null) || (showform1.IsDisposed))
                {
                    showform1 = new TileDatasSearch(false);
                    showform1.TopMost = true;
                    showform1.Show();
                }
            }
            else //landtiles
            {
                if ((showform2 == null) || (showform2.IsDisposed))
                {
                    showform2 = new TileDatasSearch(true);
                    showform2.TopMost = true;
                    showform2.Show();
                }
            }
        }

        private void OnClickSelectItem(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
                return;
            int index = (int)treeViewItem.SelectedNode.Tag;
            if (Options.DesignAlternative)
                FiddlerControls.ItemShowAlternative.SearchGraphic(index);
            else
                FiddlerControls.ItemShow.SearchGraphic(index);
        }

        private void OnClickSelectInLandtiles(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
                return;
            int index = (int)treeViewLand.SelectedNode.Tag;
            if (Options.DesignAlternative)
                FiddlerControls.LandTilesAlternative.SearchGraphic(index);
            else
                FiddlerControls.LandTiles.SearchGraphic(index);
        }

        private void OnClickSelectRadarItem(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
                return;
            int index = (int)treeViewItem.SelectedNode.Tag;
            FiddlerControls.RadarColor.Select(index, false);
        }

        private void OnClickSelectRadarLand(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
                return;
            int index = (int)treeViewLand.SelectedNode.Tag;
            FiddlerControls.RadarColor.Select(index, true);
        }
    }
}
