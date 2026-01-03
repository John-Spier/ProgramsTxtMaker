using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProgramsTxtMaker
{
	public partial class Form1 : Form
    {

        bool CacheDirectories = true;
        Encoding encoding = Encoding.ASCII;
        bool sjis = false;
        //[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        //private static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);

        public Form1()
        {
            InitializeComponent();
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //encoding = Encoding.GetEncoding(932);
		}

        private void Button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog()==DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
            
        }

        private static bool IsoName(string filename)
        {
            try
            {
                string oldfn = Path.GetFileNameWithoutExtension(filename).Replace('_', 'A');
                string oldext = Path.GetExtension(filename)[1..];
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

        private static bool IsoDir(string filename)
        {
            try
            {
				if (!filename.All(char.IsAscii))
				{
					return false;
				}
				if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
				{
					return false;
				}
				if (!filename.All(char.IsLetterOrDigit))
				{
					return false;
				}
				if (filename.Length > 8)
				{
					return false;
				}
				return true;
            } catch { return false; }
        }

        private int AddFiles(string ext, string stack, StreamWriter writer, StreamWriter psf, string tbone, int sn, bool cachedir = false)
        {
            int dn = 0;
            int fsn = 0;
			string oldfn;
			string cdromdir;
            string g;
            Dictionary<string, int> dirsizes = [];

            if (cachedir)
            {
                foreach (string d in Directory.EnumerateDirectories(tbone, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        dirsizes.Add(d, Directory.EnumerateFileSystemEntries(d).Count());
                    }
                    catch (Exception e)
                    {
                        this.Text = "Directory " + d + " Exception " + e.Message;
					}
                }
                try
                {
                    dirsizes.Add(Path.GetFullPath(tbone), Directory.EnumerateFileSystemEntries(tbone).Count());
                }
                catch (Exception e)
                {
					this.Text = "Root directory " + Path.GetFullPath(tbone) + " Exception " + e.Message;
				}
            }

            foreach (string f in Directory.GetFiles(tbone, "*" + ext, SearchOption.AllDirectories).OrderBy(x => x))
            {
                if (!dirsizes.TryGetValue(Path.GetDirectoryName(f), out int dir))
                {
                    if (dirsizes.TryAdd(Path.GetDirectoryName(f), Directory.EnumerateFileSystemEntries(Path.GetDirectoryName(f)).Count()))
                    {
                        dir = dirsizes[Path.GetDirectoryName(f)];
                    }
                    else
                    {
                        this.Text = "Could not find amount of files in " + Path.GetDirectoryName(f);
						dir = (int)numericUpDown3.Value + 1;
                    }
                }
                try
                {
                    oldfn = Path.GetFileNameWithoutExtension(f);
                    if (checkBox3.Checked && dir > numericUpDown3.Value)
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
                        if (IsoName(Path.GetFileName(f).Replace('_', 'A')))
                        {
                            g = g = h + Path.DirectorySeparatorChar + Path.GetFileName(f);
						}
                        else
                        {
                            fsn++;
                            g = h + Path.DirectorySeparatorChar + fsn.ToString() + ext;
                        }
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

                    cdromdir = g[Path.GetFullPath(tbone).Length..].ToUpperInvariant();
                    //MessageBox.Show('"' + oldfn.Substring(0, Math.Min(23, oldfn.Length)) + "\"cdrom:" + cdromdir + ";1\"");
                    //MessageBox.Show(cdromdir);
                    if (writer != null)
                    {
                        textBox2.Text += $"\"{oldfn[..Math.Min(23, oldfn.Length)]}\"cdrom:{cdromdir};1\"\r\n";
                        writer.WriteLine('"' + oldfn[..Math.Min(23, oldfn.Length)] + "\"cdrom:" + cdromdir + ";1\"");
                    }
                    if (psf != null)
                    {
                        textBox3.Text += $"\"{oldfn[..Math.Min(63, oldfn.Length)]}\" \"{cdromdir}{stack}\r\n";
                        psf.WriteLine('"' + oldfn[..Math.Min(63, oldfn.Length)] + "\" \"" + cdromdir + stack);
                    }
                }
                catch (Exception fx)
                {
                    this.Text = "File Error: " + fx.Message;
                }
            }
            return sn;
        }

        private void Button2_Click(object sender, EventArgs e)
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
            StreamWriter writer = new(progfile, false, encoding);//File.CreateText(progfile);
            StreamWriter psf = new(tbone + Path.DirectorySeparatorChar + "TITLES.TXT", false, encoding);//File.CreateText(tbone + Path.DirectorySeparatorChar + "TITLES.TXT");
            writer.WriteLine("START");
            if (checkBox2.Checked)
            {
                foreach (string d in Directory.GetDirectories(tbone, "*", SearchOption.TopDirectoryOnly).OrderBy(x => x)) //can't rename inner directories
                {
                    //textBox2.Text += d + "\r\n";
                    if (!IsoDir(Path.GetFileName(d).Replace('_', 'A')))
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
            }
            sn = AddFiles(".EXE", "\" \"801FFFF0\"", writer, psf, tbone, sn, CacheDirectories);
            if (checkBox4.Checked)
            {
                sn = AddFiles(".VFS", "\" \"FFFFFFFE\"", null, psf, tbone, sn, CacheDirectories);
				sn = AddFiles(".STR", "\" \"FFFFFF0A\"", null, psf, tbone, sn, CacheDirectories);
				sn = AddFiles(".BS1", "\" \"FFFFFF0B\"", null, psf, tbone, sn, CacheDirectories);
				sn = AddFiles(".BS2", "\" \"FFFFFF0C\"", null, psf, tbone, sn, CacheDirectories);
				sn = AddFiles(".XA", "\" \"FFFFFF06\"", null, psf, tbone, sn, CacheDirectories);
				sn = AddFiles(".XA1", "\" \"FFFFFF0D\"", null, psf, tbone, sn, CacheDirectories);
				sn = AddFiles(".XA2", "\" \"FFFFFF0E\"", null, psf, tbone, sn, CacheDirectories);
				sn = AddFiles(".CNF", "\" \"FFFFFF16\"", null, psf, tbone, sn, CacheDirectories);
			}
            
            textBox2.Text += "\"END\"";
            writer.Write("\"END\"");
            writer.Flush();
            writer.Close();
            writer.Dispose();
            psf.Flush();
            psf.BaseStream.WriteByte(0x80);
            psf.Flush();
            psf.Close();
            psf.Dispose();
            textBox3.Text += '€';
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            this.BackgroundImageLayout = ImageLayout.Center;
            CacheDirectories = !CacheDirectories;
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

        private void Label2_Click(object sender, EventArgs e)
        {
            this.BackgroundImageLayout = ImageLayout.Stretch;
            if (sjis)
            {
                encoding = Encoding.ASCII;
                sjis = false;
            }
            else
            {
                encoding = Encoding.GetEncoding(932);
                sjis = true;
            }
        }


    }
}
