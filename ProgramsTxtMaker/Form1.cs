using System;
using System.IO;
using System.Linq;
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

        private bool IsoName(string filename) //this does not allow _ even though iso9660 allows it to save time
        {
            try
            {
                string oldfn = Path.GetFileNameWithoutExtension(filename);
                string oldext = Path.GetExtension(filename).Substring(1);
                if (!filename.All(char.IsAscii))
                {
                    return false;
                }
                if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
                {
                    return false;
                }
                // ascii is already checked for and this avoids the . problem
                if (!oldext.All(char.IsLetterOrDigit))
                {
                    return false;
                }
                if (!oldfn.All(char.IsLetterOrDigit))
                {
                    return false;
                }
                if (oldext.Length > 3)
                {
                    return false;
                }
                if (oldfn.Length > 8)
                {
                    return false;
                }
                return true;
            }
            catch { return false; }
		}

        private int AddFiles(string ext, string stack, StreamWriter writer, StreamWriter psf, string tbone, int sn)
        {
            int dn = 0;
            string oldfn;
            string cdromdir;
            string g;
            foreach (string f in Directory.GetFiles(tbone, "*" + ext, SearchOption.AllDirectories))
            {
                try
                {
                    oldfn = Path.GetFileNameWithoutExtension(f);
                    if (checkBox3.Checked)
                    {
                        if (sn % ((int)numericUpDown3.Value) == 0)
                        {
                            dn++;
                        }

                        string h = Path.GetDirectoryName(f) + Path.DirectorySeparatorChar + dn.ToString();
                        if (!Directory.Exists(h))
                        {
                            Directory.CreateDirectory(h);
                        }

                        sn++;
                        g = h + Path.DirectorySeparatorChar + sn.ToString() + ext;

                        File.Move(Path.GetFullPath(f), g);

                    }

                    else if (checkBox1.Checked && !IsoName(Path.GetFileName(f)))
                    {
                        g = Path.GetDirectoryName(f) + Path.DirectorySeparatorChar + sn.ToString() + ext;
                        sn++;
                        File.Move(Path.GetFullPath(f), g);
                    }

                    else
                    {
                        g = Path.GetFullPath(f);
                    }

                    cdromdir = g.Substring(Path.GetFullPath(tbone).Length).ToUpperInvariant();
                    //MessageBox.Show('"' + oldfn.Substring(0, Math.Min(23, oldfn.Length)) + "\"cdrom:" + cdromdir + ";1\"");
                    //MessageBox.Show(cdromdir);
                    if (writer != null)
                    {
                        textBox2.Text += '"' + oldfn.Substring(0, Math.Min(23, oldfn.Length)) + "\"cdrom:" + cdromdir + ";1\"\r\n";
                        writer.WriteLine('"' + oldfn.Substring(0, Math.Min(23, oldfn.Length)) + "\"cdrom:" + cdromdir + ";1\"");
                    }
                    if (psf != null)
                    {
                        textBox3.Text += '"' + oldfn.Substring(0, Math.Min(63, oldfn.Length)) + "\" \"" + cdromdir + stack + "\r\n";
                        psf.WriteLine('"' + oldfn.Substring(0, Math.Min(63, oldfn.Length)) + "\" \"" + cdromdir + stack);
                    }
                }
                catch (Exception fx)
                {
                    this.Text = "File Error: " + fx.Message;
                }
            }
            return sn;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox3.Clear();
            textBox2.Text = "START\r\n";
            string tbone = textBox1.Text;
            tbone = tbone.Trim();
            tbone = tbone.TrimEnd('\\');
            int sn = 1;

            string progfile = tbone + Path.DirectorySeparatorChar + "PROGRAMS.TXT";
            if (checkBox3.Checked) { checkBox1.Checked = true; }
            /*
            if (File.Exists(progfile))
            {
                File.Move(progfile, tbone + Path.DirectorySeparatorChar + sn.ToString() + ".BAK");
                
            }
            */
            StreamWriter writer = File.CreateText(progfile);
            StreamWriter psf = File.CreateText(tbone + Path.DirectorySeparatorChar + "TITLES.TXT");
            writer.WriteLine("START");
            foreach (string d in Directory.GetDirectories(tbone, "*", SearchOption.TopDirectoryOnly)) //can't rename inner directories
            {
                //textBox2.Text += d + "\r\n";
                if (checkBox2.Checked && Path.GetFileName(d).Length > 8)
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
            sn = AddFiles(".EXE", "\" \"801FFFF0\"", writer, psf, tbone, sn);
            if (checkBox4.Checked)
            {
                sn = AddFiles(".VFS", "\" \"FFFFFFFE\"", null, psf, tbone, sn);
            }
            
            textBox2.Text += "\"END\"";
            writer.Write("\"END\"");
            writer.Flush();
            writer.Close();
            psf.Flush();
            psf.BaseStream.WriteByte(0x80);
            psf.Close();
            textBox3.Text += '€';
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.BackgroundImageLayout = ImageLayout.Center;
            //MessageBox.Show(textBox1.Text, IsoName(textBox1.Text).ToString());
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
            //label2.Visible = !label2.Visible;
            checkBox1.Visible = !checkBox1.Visible;
            checkBox2.Visible = !checkBox2.Visible;
            //numericUpDown1.Visible = !numericUpDown1.Visible;
            //numericUpDown2.Visible = !numericUpDown2.Visible;
            checkBox3.Visible = !checkBox3.Visible;
            numericUpDown3.Visible = !numericUpDown3.Visible;
            checkBox4.Visible = !checkBox4.Visible;
            textBox3.Visible = !textBox3.Visible;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }


    }
}
