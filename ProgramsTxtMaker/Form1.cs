using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;

namespace ProgramsTxtMaker
{
    public partial class Form1 : Form
    {


        //[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        //private static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog()==DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //textBox2.Clear();
            textBox2.Text = "START\r\n";
            string tbone = textBox1.Text;
            tbone = tbone.Trim();
            tbone = tbone.TrimEnd('\\');
            int sn = 1;
            int dn = 0;
            string oldfn;
            string cdromdir;
            string g;
            string progfile = tbone + Path.DirectorySeparatorChar + "PROGRAMS.TXT";
            if (checkBox3.Checked) { checkBox1.Checked = true; }
            if (File.Exists(progfile))
            {
                //File.Move(progfile, tbone + Path.DirectorySeparatorChar + sn.ToString() + ".BAK");
                
            }
            StreamWriter writer = File.CreateText(progfile);
            writer.WriteLine("START");
            foreach (string d in Directory.GetDirectories(tbone, "*", SearchOption.TopDirectoryOnly)) //can't rename inner directories
            {
                //textBox2.Text += d + "\r\n";
                if (checkBox2.Checked)
                {
                    try
                    {
                        Directory.Move(Path.GetFullPath(d), Path.GetDirectoryName(d) + "\\" + sn.ToString());
                    } 
                    catch (Exception dx)
                    {
                        this.Text = "Directory Error: " + dx.Message;
                    }
                    sn++;
                }

            }
            foreach (string f in Directory.GetFiles(tbone, @"*.exe", SearchOption.AllDirectories))
            {
                try
                {
                    oldfn = Path.GetFileNameWithoutExtension(f);
                    if (checkBox3.Checked)
                    {
                        if (sn % ((int)numericUpDown3.Value)==0)
                        {
                            dn++;
                        }

                        string h = Path.GetDirectoryName(f) + Path.DirectorySeparatorChar + dn.ToString();
                        if (!Directory.Exists(h))
                        {
                            Directory.CreateDirectory(h);
                        }
                        
                        sn++;
                        g = h + Path.DirectorySeparatorChar + sn.ToString() + ".EXE";
                        
                        File.Move(Path.GetFullPath(f), g);

                    }
                    else if (checkBox1.Checked)
                    {
                        g = Path.GetDirectoryName(f) + Path.DirectorySeparatorChar + sn.ToString() + ".EXE";
                        sn++;
                        //MessageBox.Show(g);
                        File.Move(Path.GetFullPath(f), g);
                    } 
                    
                    else
                    {
                        g = Path.GetFullPath(f);
                    }
                    
                    cdromdir = g.Substring(Path.GetFullPath(tbone).Length);
                    //MessageBox.Show(cdromdir);
                    textBox2.Text += '"' + oldfn.Substring(Math.Min((int)numericUpDown1.Value, oldfn.Length), Math.Min(((int)numericUpDown2.Value) - (int)numericUpDown1.Value, oldfn.Length - (int)numericUpDown1.Value)) + "\"cdrom:" + cdromdir + ";1\"\r\n";
                    writer.WriteLine('"' + oldfn.Substring(Math.Min((int)numericUpDown1.Value, oldfn.Length), Math.Min(((int)numericUpDown2.Value) - (int)numericUpDown1.Value, oldfn.Length - (int)numericUpDown1.Value)) + "\"cdrom:" + cdromdir + ";1\"");
                }
                catch (Exception fx)
                {
                    this.Text = "File Error: " + fx.Message;
                }
            }
            textBox2.Text += "\"END\"";
            writer.WriteLine("\"END\"");
            writer.Flush();
            writer.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.BackgroundImageLayout = ImageLayout.Center;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            //MessageBox.Show("abc");
            textBox1.Visible = !textBox1.Visible;
            textBox2.Visible = !textBox2.Visible;
            button1.Visible = !button1.Visible;
            button2.Visible = !button2.Visible;
            label1.Visible = !label1.Visible;
            label2.Visible = !label2.Visible;
            checkBox1.Visible = !checkBox1.Visible;
            checkBox2.Visible = !checkBox2.Visible;
            numericUpDown1.Visible = !numericUpDown1.Visible;
            numericUpDown2.Visible = !numericUpDown2.Visible;
            checkBox3.Visible = !checkBox3.Visible;
            numericUpDown3.Visible = !numericUpDown3.Visible;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }


    }
}
