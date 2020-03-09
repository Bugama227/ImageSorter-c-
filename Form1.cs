using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
// Program made to simplify sorting images in directory. Random image appear on a screen.
// Afterwards you can move it to one of a several directories using hotkeys or double click on a list.
// Directories are being chosen by user
// Or user can click SKIP to make a new folder inside directory with images and move shown image there.
// While image on a screen, user can find some of it's properties such as: name, size(width, height),
//  size(weight(kb,mb)), path to image location, extension, exif(TBA)
namespace ImageSorter
{
    public partial class Form1 : Form
    {
        public string PictureFolderLocation = ""; // created to hold path of chosen picture directory
        public ToolTip toolTip1 = new ToolTip(); // created to hold path of chosen goal directory
        //and actually i don't know where i can dispose that. I'm really sorry that your memory was corrupted
        public List<string> ImagesPaths = new List<string>(); // holds paths of images inside chosen directory
        private string CurrentPath;

        public Form1()
        {
            if(!String.IsNullOrEmpty(Properties.Settings.Default.Language))
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
            }
            InitializeComponent();
        }

        //On Adding new image funcs
        //
        //
        private void GetNewImage()
        {
            GC.Collect(1, GCCollectionMode.Optimized);
            string ImagePath = GetImage();
            if (ImagePath != null)
            {
                ImagePictureBox.ImageLocation = ImagePath;
                CurrentPath = ImagePath;
                SkipButton.Enabled = true;
                FillPropertiesList(ImagePath);
            }
            else
            {
                NewImageFolderButton_Click(new object(), new EventArgs());
            }
        }

        private string GetImage()
        {
            if (ImagesPaths.Count == 0) ImagesPaths = SortByExtension(); // get full list of paths of images in directory

            Random random = new Random();
            int buffPathIndex = (Properties.Settings.Default.GetImageMethod == "random")
                ? random.Next(ImagesPaths.Count)
                : ImagesPaths.Count - 1;
            

            try
            {
                ImagesPaths.RemoveAt(buffPathIndex);
                return ImagesPaths[buffPathIndex];
            }
            catch
            {
                // exception might be occured because there is no image, or there is only 1 image (you cannot random from 1 to 1)
                if (ImagesPaths == null) // if last image in directory was moved
                {
                    MessageBox.Show(Localization.ChooseFolderMSG);
                    SkipButton.Enabled = false;
                    return null;
                }
                else if(ImagesPaths.Count <= 0) // if length lower than 0 directory is empty
                {
                    MessageBox.Show(Localization.FolderIsEmptyMSG);
                    SkipButton.Enabled = false;
                    return null;
                }
                else // if there is only 1 image in directory
                {
                    SkipButton.Enabled = true;
                    return ImagesPaths[0];
                }
            }
        }

        private List<string> SortByExtension()
        {
            return (PictureFolderLocation == "")
                ? null // return nothing if last picture in directory was moved
                : Directory
                    .GetFiles(
                        PictureFolderLocation,
                        "*.*",
                        (Properties.Settings.Default.SortImageMethod == "top")
                            ? SearchOption.TopDirectoryOnly
                            : SearchOption.AllDirectories
                    )
                    .Where(s =>
                        s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        s.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                        s.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                        s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                        s.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
                        s.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                    ).ToList();
        }

        private void FillPropertiesList(string ImagePath)
        {
            //clearing LV on every new image to prevent overflowing it with old info
            ImagePropertiesListView.Items.Clear();

            // FILE NAME
            this.AddIntoDescriptionLV(Localization.NamePropLV, Path.GetFileName(CurrentPath));

            // FILE PATH
            this.AddIntoDescriptionLV(Localization.PathPropLV, CurrentPath);

            // FILE EXTENSION
            this.AddIntoDescriptionLV(Localization.ExtensionPropLV, Path.GetExtension(CurrentPath).Substring(1).ToUpper());

            // FILE WEIGHT
            this.AddIntoDescriptionLV(Localization.WeightPropLV, FormatImageWeight());

            // FILE DIMENSIONS
            var (width, height) = GetPictureDimensions(ImagePath);

            this.AddIntoDescriptionLV(Localization.WidthPropLV, width);

            this.AddIntoDescriptionLV(Localization.HeightPropLV, height);
        }

        private void AddIntoDescriptionLV(string arg1, string arg2)
        {
            ImagePropertiesListView.Items.Add(new ListViewItem(new[] { arg1, arg2 }));
        }

        private string FormatImageWeight()
        {
            float size = new FileInfo(CurrentPath).Length;
            return (size > 1048576)
                ? Convert.ToSingle(Math.Round(size / (1024 * 1024), 1)).ToString() + " Mb"
                : Convert.ToSingle(Math.Round(size / 1024, 1)).ToString() + " Kb";
        }

        private (string, string) GetPictureDimensions(string path)
        {
            Stream stream;
            stream = File.OpenRead(path); // getting a new stream and closing it just after dimensions were got
            //that's most efficient way
            try
            {
                // if image is not corrupted
                Image sourceImage = Image.FromStream(stream);

                var width = sourceImage.Width.ToString();
                var height = sourceImage.Height.ToString();

                sourceImage.Dispose();
                stream.Dispose();

                return (width, height);
            }
            catch
            {
                // if image corrupted it's dimensions must be 0x0
                stream.Dispose(); // doubling disposing in case of corrupted image
                return ("0", "0");
            }
        }

        //
        //
        //OnAdding new image funcs ends here


        //OnChanging Folder ListView funcs
        //
        //

        private void UpdateFolderListViewItem()
        {
            if (NameFolderLabel.Text == "") 
                NameFolderLabel.Text = Localization.UnknownNameFolderLV;

            if (HotKeyLabel.Text == "") 
                HotKeyLabel.Text = "�"; 

            FolderListView.SelectedItems[0].SubItems[0].Text = NameFolderLabel.Text;
            FolderListView.SelectedItems[0].SubItems[1].Text = toolTip1.GetToolTip(FolderPathLabel);
            FolderListView.SelectedItems[0].SubItems[2].Text = HotKeyLabel.Text;

            //CLEAR FIELDS
            NameFolderLabel.Text = "";
            FolderPathLabel.Text = "";
            HotKeyLabel.Text = "";
        }

        private void DeleteFolderListViewItem()
        {
            DialogResult dialogResult = MessageBox.Show(
                Localization.DeleteButtonTextFolderLV, 
                Localization.DeleteButtonTitleFolderLV, 
                MessageBoxButtons.OKCancel, 
                MessageBoxIcon.Warning
            );

            if (dialogResult == DialogResult.OK)
            {
                FolderListView.Items.RemoveAt(FolderListView.SelectedIndices[0]);

                //CLEAR FIELDS
                NameFolderLabel.Text = "";
                FolderPathLabel.Text = "";
                HotKeyLabel.Text = "";
            }
        }
        //
        //
        //OnChanging Folder ListView funcs ends here

        //SaveLoad Profile funcs
        //
        //

        private void SaveProfileIntoFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "BPF config files (*.bpf)|*.bpf"; // creating new file format to simplify config exploration
            sfd.FilterIndex = 2;
            sfd.AddExtension = true;
            sfd.DefaultExt = "bpf";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter profileSW = new StreamWriter(sfd.FileName, false);
                //profileSW.WriteLine("Last openned picture folder");
                if (PictureFolderLocation != "")
                    profileSW.WriteLine(PictureFolderLocation);
                else
                    profileSW.WriteLine("nothing");

                if (FolderListView.Items.Count != 0) // if LV isn't empty write info about it
                {
                    //profileSW.WriteLine("Last openned save folders");
                    foreach (ListViewItem item in FolderListView.Items)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                        {
                            sb.Append(string.Format("{0}|", subitem.Text));
                        }
                        profileSW.WriteLine(sb.ToString().Substring(0, sb.Length - 1));
                    }
                }
                profileSW.Close();
            }
            File.Move(sfd.FileName, Path.ChangeExtension(sfd.FileName, ".bpf")); // if user save file as .txt it will be converted to .bpf
            sfd.Dispose();
        }

        private void LoadProfileFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BPF config files (*.bpf)|*.bpf";

            FolderListView.Items.Clear();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                StreamReader profileSR = new StreamReader(ofd.FileName, false);
                PictureFolderLocation = profileSR.ReadLine().ToString();

                if(PictureFolderLocation != "nothing" && Directory.Exists(PictureFolderLocation))
                    GetNewImage();
                else
                    PictureFolderLocation = "";

                while (profileSR.EndOfStream != true)
                    FolderListView.Items.Add(new ListViewItem(profileSR.ReadLine().Split('|')));

                profileSR.Dispose();
            }
            ofd.Dispose();
            this.KeyPreview = true;
        }

        //
        //
        //SaveLoad profile funcs ends here

        private void AddToListView(string Name, string Path, string HotKey)
        {
            FolderListView.Items.Add(new ListViewItem(new[] { Name, Path, HotKey }));
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            // if window getting minimized without that condition,
            //exception of getting splitterdistance lower than 0
            if (WindowState != FormWindowState.Minimized)
            {
                // made to stabilize form of an add folder menu
                // i found only way to do that. With changing splitters location
                MainSplitContainer.SplitterDistance = ClientSize.Width - 280;
                OptionsSplitContainer.SplitterDistance = ClientSize.Width - 200;
                ImageSplitContainer.SplitterDistance = ClientSize.Height - 200;
            }
        }

        private void CollapseOptionsButton_Click(object sender, EventArgs e)
        {
            MainSplitContainer.Panel2Collapsed = !MainSplitContainer.Panel2Collapsed;

            CollapseOptionsButton.Text = (MainSplitContainer.Panel2Collapsed)
                ? "←←←"  // To open
                : "→→→"; // To close
        }

        private void ColapseFoldersButton_Click(object sender, EventArgs e)
        {
            ImageSplitContainer.Panel2Collapsed = !ImageSplitContainer.Panel2Collapsed;

            ColapseFoldersButton.Text = (ImageSplitContainer.Panel2Collapsed)
                ? "↑↑↑"  // To open
                : "↓↓↓"; // To close
        }

        private void NewImageFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;

            // List needs to be cleared to prevent unchosen files from moving
            ImagesPaths.Clear();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                PictureFolderLocation = fbd.SelectedPath;
                GetNewImage();
            }

            fbd.Dispose();
        }

        private void SkipButton_Click(object sender, EventArgs e)
        {
            if (ImagePictureBox.Image == null)
                // if skip button somehow still active after every image were moved
                MessageBox.Show(
                    Localization.SkipButtonText,
                    Localization.SkipButtonTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

            if (Properties.Settings.Default.SkipImageMethod == "folder" && ImagePictureBox.Image != null)
            {
                string paleName = $"{Path.GetDirectoryName(CurrentPath)}\\SKIPPED\\";
                string destName = $"{paleName}{Path.GetFileName(CurrentPath)}";
                ImagePictureBox.Image = null; // disposing image usage to make it moveable

                if (Directory.Exists(paleName)) // if SKIPPED directory exists just move
                    File.Move(CurrentPath, destName);
                else // create and move
                {
                    Directory.CreateDirectory(paleName);
                    File.Move(CurrentPath, destName);
                }
            }

            GetNewImage();
        }

        private void NameFolderTextBox_Enter(object sender, EventArgs e)
        {
            this.KeyPreview = false; // to prevent activation of OnHotkey moving
        }

        private void NameFolderTextBox_Leave(object sender, EventArgs e)
        {
            this.KeyPreview = true; // to return ability to scan keys for HotKeys
            NameFolderLabel.Text = NameFolderTextBox.Text; 
        }

        private void PathButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                // the full path would be on a tooltip
                toolTip1.SetToolTip(FolderPathLabel, fbd.SelectedPath);
                // label would contain only name of last directory
                FolderPathLabel.Text = new DirectoryInfo(fbd.SelectedPath).Name;
            }

            fbd.Dispose();
        }

        private void HotKeyButton_Click(object sender, EventArgs e)
        {
            while (true) // try until user click cancel or input allowed symbol
            {
                string buff = Interaction.InputBox(Localization.HotkeyText, Localization.HotkeyTitle).ToLower();

                if (buff.Length > 1 || new Regex(@"[А-яЁё]$").IsMatch(buff)) // if amount of written symbols is greater than 1 or it's russian
                    MessageBox.Show(Localization.HotkeyErrorText);
                else // cancel was clicked or allowed symbol was input
                {
                    HotKeyLabel.Text = buff; // value would be nothing(cancel) or correct symbol
                    break;
                }
            }
        }

        private void AddFolderButton_Click(object sender, EventArgs e)
        {
            if (toolTip1.GetToolTip(FolderPathLabel) == "")
                MessageBox.Show(
                    Localization.AddButtonClickTextFolderLV, 
                    Localization.AddButtonClickTitleFolderLV, 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning
                );
            else
            {
                if (NameFolderLabel.Text == "")
                    NameFolderLabel.Text = Localization.UnknownNameFolderLV;
                if (HotKeyLabel.Text == "")
                    HotKeyLabel.Text = "�";

                AddToListView(NameFolderLabel.Text, toolTip1.GetToolTip(FolderPathLabel), HotKeyLabel.Text);
                toolTip1.SetToolTip(FolderPathLabel, "");

                NameFolderTextBox.Text = "";
                NameFolderLabel.Text = "";
                FolderPathLabel.Text = "";
                HotKeyLabel.Text = "";
            }

            this.KeyPreview = true;
        }

        private void UpdateFolderButton_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateFolderListViewItem();
            }
            catch
            {
                MessageBox.Show(
                    Localization.UpdateButtonClickTextFolderLV,
                    Localization.UpdateButtonClickTitleFolderLV, 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning
                );
            }
        }

        private void DeleteFolderButton_Click(object sender, EventArgs e)
        {
            try
            {
                DeleteFolderListViewItem();
            }
            catch // the only exception is about cannot find ListView elements
            {
                MessageBox.Show(
                    Localization.DeleteButtonClickTextFolderLV,
                    Localization.DeleteButtonClickTitleFolderLV,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private void ClearFolderListViewButton_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
                Localization.ClearButtonClickTextFolderLV, 
                Localization.ClearButtonClickTitleFolderLV, 
                MessageBoxButtons.OKCancel, 
                MessageBoxIcon.Warning
            );

            if (dialogResult == DialogResult.OK)
                FolderListView.Items.Clear();

            //CLEAR FIELDS
            NameFolderLabel.Text = "";
            FolderPathLabel.Text = "";
            HotKeyLabel.Text = "";
        }

        private void FolderListView_MouseClick(object sender, MouseEventArgs e)
        {
            CheckForDuplicates(
                FolderListView.SelectedItems[0].SubItems[1].Text,
                Path.GetFileNameWithoutExtension(CurrentPath),
                Path.GetExtension(CurrentPath)
            );
            
            if (FolderListView.SelectedItems.Count != 0)
            {
                NameFolderLabel.Text = FolderListView.SelectedItems[0].SubItems[0].Text;
                toolTip1.SetToolTip(FolderPathLabel, FolderListView.SelectedItems[0].SubItems[1].Text);
                FolderPathLabel.Text = new DirectoryInfo(FolderListView.SelectedItems[0].SubItems[1].Text).Name;
                HotKeyLabel.Text = FolderListView.SelectedItems[0].SubItems[2].Text;
            }

            if (
                e.Button == MouseButtons.Right && 
                FolderListView.FocusedItem != null &&
                FolderListView.FocusedItem.Bounds.Contains(e.Location)
            )
            {
                ContextMenu m = new ContextMenu();
                MenuItem OpenFolderMenuItem = new MenuItem(Localization.RightClickOnLVItemTextFolderLV);

                OpenFolderMenuItem.Click += delegate (object sender2, EventArgs e2)
                {
                    Process.Start(FolderListView.SelectedItems[0].SubItems[1].Text);
                };

                m.MenuItems.Add(OpenFolderMenuItem);

                m.Show(FolderListView, new Point(e.X, e.Y));
                m.Dispose();
            }
        }

        private void FolderListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CheckForDuplicates(
                FolderListView.SelectedItems[0].SubItems[1].Text,
                Path.GetFileNameWithoutExtension(CurrentPath),
                Path.GetExtension(CurrentPath)
            );
        }

        private void CheckForDuplicates(string fileDir, string fileName, string fileExt)
        {
            try
            {
                // test on similar name
                if (File.Exists($"{FolderListView.SelectedItems[0].SubItems[1].Text}\\{Path.GetFileName(CurrentPath)}")) // if any familiarities found
                {
                    int i = 1;

                    while (true)
                    {
                        string tempFileName = $"{fileDir}\\{fileName}{$"_{i}"}{fileExt}";

                        if (!File.Exists(tempFileName))
                        {
                            fileName = $"{fileName}{$"_{i}"}";
                            break;
                        }
                        i++;
                    }
                }

                string desFileLoc = $"{fileDir}\\{fileName}{fileExt}";

                ImagePictureBox.Image = null;
                File.Move(CurrentPath, desFileLoc);
                CurrentPath = "";
                GetNewImage();
            }
            catch
            {
                MessageBox.Show(
                    Localization.DoubleClickOnLVItemTextFolderLV, 
                    Localization.DoubleClickOnLVItemTitleFolderLV,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly
                );
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            foreach (ListViewItem item in FolderListView.Items)
                if (item.SubItems[2].Text == e.KeyChar.ToString())
                    CheckForDuplicates(
                        item.SubItems[1].Text,
                        Path.GetFileNameWithoutExtension(CurrentPath),
                        Path.GetExtension(CurrentPath)
                    );
        }

        private void SaveProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FolderListView.Items.Count == 0 && PictureFolderLocation == "")
                MessageBox.Show(
                    Localization.SaveProfileButtonClickText,
                    Localization.SaveProfileButtonClickTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            else
                SaveProfileIntoFile();
        }

        private void LoadProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FolderListView.Items.Count != 0 || PictureFolderLocation != "")
            {
                DialogResult dialogResult = MessageBox.Show(
                    Localization.LoadProfileButtonClickText,
                    Localization.LoadProfileButtonClickTitle,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning
                );

                if (dialogResult == DialogResult.OK)
                    LoadProfileFromFile();
            }
            else
                LoadProfileFromFile();
        }

        private void ImagePictureBox_Click(object sender, EventArgs e)
        {
            if(ImagePictureBox.Image != null)
            {
                Form form2 = new Form2(CurrentPath);
                form2.Show();
            }
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3(this);
            form3.FormClosing += new FormClosingEventHandler(this.OptionsForm_FormClosing);
            this.Visible = false;
            form3.Show();
        }
        
        private void OptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            if (Properties.Settings.Default.LanguageChanged == "changed")
                InitializeComponent();
        }
    }
}
