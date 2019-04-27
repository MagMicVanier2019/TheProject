using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Emgu;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;


namespace SkinFilter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Load Picture...";
                ofd.Filter = "Jpeg Image (*.jpg)|*.jpg|Bitmap Image (*.bmp)|*.bmp|Png Image (*.png)|*.png|All Files (*.*)|*.*";
                ofd.RestoreDirectory = true;
                ofd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = ofd.FileName;
                    pictureBox1.Load(textBox1.Text);
                }
                GC.Collect();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > numericUpDown2.Value)
            {
                if (numericUpDown4.Value > numericUpDown3.Value)
                {
                    if (pictureBox1.Image != null)
                    {
                        //Fall in (Within range gets removed)
                        //        20, 160              10, 255
                        Bitmap CalculatedMask = BackProjection.BackProject((Bitmap)pictureBox1.Image, new int[] { (int)numericUpDown2.Value, (int)numericUpDown1.Value }, new int[] { (int)numericUpDown3.Value, (int)numericUpDown4.Value });
                        if (checkBox3.Checked)
                        {
                            pictureBox2.Image = CalculatedMask;
                        }
                        else
                        {
                            pictureBox2.Image = BackProjection.SuperPositionedImage(CalculatedMask, (Bitmap)pictureBox1.Image);
                        }
                        GC.Collect();
                    }
                    else
                    {
                        MessageBox.Show("No Image loaded!");
                    }
                }
                else
                {
                    MessageBox.Show("Error! The min Saturation is larger then the max Saturation.");
                }
            }
            else
            {
                MessageBox.Show("Error! The min Hue is larger then the max hue.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "Export Processed Image...";
                    sfd.RestoreDirectory = true;
                    sfd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    sfd.Filter = "Jpeg Image (*.jpg)|*.jpg";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        pictureBox2.Image.Save(sfd.FileName, ImageFormat.Jpeg);
                    }
                }
            }
        }

        private void CreateExampleColor()
        {
            if (checkBox1.Checked)
            {
                button2_Click(null, null);
            }

            //Make Pixel Color Example
            Hsv CurrMax = new Hsv((double)numericUpDown1.Value, (double)numericUpDown4.Value, (255));
            Hsv CurrMin = new Hsv((double)numericUpDown2.Value, (double)numericUpDown3.Value, (255));
            Image<Hsv, Byte> ImgMax = new Image<Hsv, Byte>(1, 1, CurrMax);
            Image<Hsv, Byte> ImgMin = new Image<Hsv, Byte>(1, 1, CurrMin);
            pictureBox3.Image = ImgMax.Bitmap;
            pictureBox4.Image = ImgMin.Bitmap;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            CreateExampleColor();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            CreateExampleColor();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            CreateExampleColor();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

            CreateExampleColor();

        }

        private void CalculateValues(Color CurrColor)
        {
            Image<Bgr, Byte> RGBPixel = new Image<Bgr, Byte>(1, 1, new Bgr(CurrColor));
            Image<Hsv, Byte> HSVPixel = RGBPixel.Convert<Hsv, Byte>();
            Hsv TrueColor = HSVPixel[0,0];
            textBox2.Text = TrueColor.Hue.ToString() + ", " + TrueColor.Satuation.ToString() + ", " + TrueColor.Value.ToString();
            int HueLeway = (int)numericUpDown5.Value;
            int SatLeway = (int)numericUpDown6.Value;
            int[] MaxColor = new int[] { (int)(TrueColor.Hue + (HueLeway / 2)), (int)(TrueColor.Satuation + (SatLeway / 2)), (int)(TrueColor.Value) };
            int[] MinColor = new int[] { (int)(TrueColor.Hue - (HueLeway / 2)), (int)(TrueColor.Satuation - (SatLeway / 2)), (int)(TrueColor.Value) };
            //MessageBox.Show(CurrColor.GetHue().ToString() + ", " + CurrColor.GetSaturation().ToString() + ", " + CurrColor.GetBrightness().ToString(), "Actual Hsv Value");
            //MessageBox.Show(MaxColor[0].ToString() + ", " + MaxColor[1].ToString() + ", " + MaxColor[2].ToString(), "Max Color Values");
            //MessageBox.Show(MinColor[0].ToString() + ", " + MinColor[1].ToString() + ", " + MinColor[2].ToString(), "Min Color Values");
            for (int i = 0; i < MaxColor.Count(); i++)
            {
                if (MaxColor[i] < 0)
                {
                    MaxColor[i] = 0;
                }
            }
            for (int i = 0; i < MinColor.Count(); i++)
            {
                if (MinColor[i] < 0)
                {
                    MinColor[i] = 0;
                }
            }
            if (MaxColor[0] == 0)
            {
                MaxColor[0] = 1;
            }
            if (MinColor[0] == 0)
            {
                MinColor[0] = 1;
            }
            //MessageBox.Show(MaxColor[0].ToString() + ", " + MaxColor[1].ToString() + ", " + MaxColor[2].ToString(), "New Max Color Values");
            //MessageBox.Show(MinColor[0].ToString() + ", " + MinColor[1].ToString() + ", " + MinColor[2].ToString(), "New Min Color Values");
            Hsv ColorMax = new Hsv(MaxColor[0], MaxColor[1], MaxColor[2]);
            Hsv ColorMin = new Hsv(MinColor[0], MinColor[1], MinColor[2]);
            numericUpDown1.Value = (int)ColorMax.Hue;
            numericUpDown4.Value = (int)ColorMax.Satuation;
            numericUpDown2.Value = (int)ColorMin.Hue;
            numericUpDown3.Value = (int)ColorMin.Satuation;
            textBox2.BackColor = CurrColor;
            Image<Hsv, Byte> TrueImage = new Image<Hsv, Byte>(1, 1, TrueColor);
            pictureBox5.Image = TrueImage.Bitmap;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    Color CurrColor = cd.Color;
                    CalculateValues(CurrColor);
                }
            }
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (textBox2.BackColor != null)
            {
                CalculateValues(textBox2.BackColor);
            }
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (textBox2.BackColor != null)
            {
                CalculateValues(textBox2.BackColor);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = BackProjection.BackProject((Bitmap)pictureBox1.Image, new int[] { (int)numericUpDown2.Value, (int)numericUpDown1.Value }, new int[] { (int)numericUpDown3.Value, (int)numericUpDown4.Value });
            GC.Collect();
        }

        private void DebugFinder(Color color)
        {
           
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            DebugFinder(textBox2.BackColor);
        }
    }
}
