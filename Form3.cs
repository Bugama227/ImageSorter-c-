using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageSorter
{
    public partial class Form3 : Form
    {
        Form FirstForm;
        public Form3(Form parentForm)
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default.Language))
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
            }
            
            InitializeComponent();
            FirstForm = parentForm;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = new System.Globalization.CultureInfo[]
            {
                System.Globalization.CultureInfo.GetCultureInfo("ru-RU"),
                System.Globalization.CultureInfo.GetCultureInfo("en-US")
            };

            comboBox1.DisplayMember = "NativeName";
            comboBox1.ValueMember = "Name";

            if(Properties.Settings.Default.GetImageMethod == "random")
            {
                radioButton1.Checked = true;
            }
            else
            {
                radioButton2.Checked = true;
            }

            if(Properties.Settings.Default.SkipImageMethod == "folder")
            {
                radioButton3.Checked = true;
            }
            else
            {
                radioButton4.Checked = true;
            }

            if(Properties.Settings.Default.SortImageMethod == "top")
            {
                radioButton5.Checked = true;
            }
            else
            {
                radioButton6.Checked = true;
            }

            if(!String.IsNullOrEmpty(Properties.Settings.Default.Language))
            {
                Properties.Settings.Default.Language = comboBox1.SelectedValue.ToString();
                Properties.Settings.Default.Save();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        { 
            Properties.Settings.Default.Language = comboBox1.SelectedValue.ToString();
            
            if(radioButton1.Checked == true)
            {
                Properties.Settings.Default.GetImageMethod = "random";
            }
            else
            {
                Properties.Settings.Default.GetImageMethod = "last";
            }

            if(radioButton3.Checked == true)
            {
                Properties.Settings.Default.SkipImageMethod = "folder";
            }
            else
            {
                Properties.Settings.Default.SkipImageMethod = "clear";
            }

            if(radioButton5.Checked == true)
            {
                Properties.Settings.Default.SortImageMethod = "top";
            }
            else
            {
                Properties.Settings.Default.SortImageMethod = "all";
            }
            Properties.Settings.Default.Save();
            if(System.Threading.Thread.CurrentThread.CurrentUICulture != System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language))
            {
                if((MessageBox.Show("Был изменён язык. Для применения настроек программа будет перезапущена. Подтвердить?", "Внимание", MessageBoxButtons.OKCancel)) == DialogResult.OK)
                {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
                    System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
                    FirstForm.Controls.Clear();
                    Properties.Settings.Default.LanguageChanged = "changed";
                    this.Close();
                }
                else
                {
                    Properties.Settings.Default.LanguageChanged = "not";
                    this.Close();
                }
            }
            else
            {
                Properties.Settings.Default.LanguageChanged = "not";
                this.Close();
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
