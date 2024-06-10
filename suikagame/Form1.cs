using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace suikagame
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Graphics graphics;

        struct Top
        {
            public int tur;//daire türü
            public int boyut;//yarıçap
            public double x;
            public double y;
            public double ivmex;
            public double ivmey;
            public double kutle;
        }
        List<Top> toplar = new List<Top>();
        private int sonrakiTur = 1;
        private int fpsSayac = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            toplar.Clear();
            sonrakiTur = 1;
            graphics = panel1.CreateGraphics();
            backgroundWorker1.RunWorkerAsync();
            button1.Enabled = false;
        }
        System.Drawing.Color arkaplanRengi = System.Drawing.ColorTranslator.FromHtml("#FFD59D");
        private Random rnd = new Random();
        private Image[] resimler = new Image[11];
        private int skor = 0, enYuksekSkor = 0;

        //private Brush[] renkler =
        //{
        //    Brushes.DarkRed,
        //    Brushes.OrangeRed,
        //    Brushes.Orange,
        //    Brushes.Magenta,
        //    Brushes.Cyan,
        //    Brushes.LawnGreen,
        //    Brushes.DarkSlateBlue,
        //    Brushes.Thistle,
        //    Brushes.Peru,
        //    Brushes.Turquoise,
        //    Brushes.Olive,
        //};
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Bitmap bitmapBuffer = new Bitmap(panel1.Width, panel1.Height);
                Graphics graphicsBuffer = Graphics.FromImage(bitmapBuffer);
                skor = 0;
                while (true)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        graphicsBuffer.Clear(arkaplanRengi);
                        graphics.DrawImage(bitmapBuffer, Point.Empty);

                        if (skor > enYuksekSkor)
                            enYuksekSkor = skor;
                        Invoke((Action)(() =>
                        {
                            skorun.Text = "Skorun: 0";
                            enYuksekSkorun.Text = "En yüksek skor: " + enYuksekSkor;
                        }));
                        MessageBox.Show("Elendin! Skorun: " + skor);

                        break;
                    }

                    graphicsBuffer.Clear(arkaplanRengi);


                    //bool breakIkinciDongu = false;
                    for (int i = 0; i < toplar.Count; i++)
                    {
                        for (int j = i + 1; j < toplar.Count; j++)
                        {
                            if (carpismaKontrol(i, j))
                            {
                                //breakIkinciDongu = true;
                                break;
                            }

                            //if (breakIkinciDongu) break;
                        }
                    }

                    for (int i = 0; i < toplar.Count; i++)
                    {
                        Top top = toplar[i];

                        top.x += top.ivmex;
                        top.y -= top.ivmey;
                        //top.ivmey -= 1f;
                        top.x = (float)Math.Max(top.boyut, Math.Min(panel1.Width - top.boyut, top.x));
                        top.y = (float)Math.Min(panel1.Height - top.boyut, top.y);

                        RectangleF rect = new RectangleF((float)(top.x - top.boyut), (float)(top.y - top.boyut),
                            top.boyut * 2, top.boyut * 2);
                        //graphicsBuffer.FillEllipse(renkler[top.tur], rect);
                        graphicsBuffer.DrawImage(resimler[top.tur - 1], rect);
                        //graphicsBuffer.DrawImage();
                        graphicsBuffer.DrawString(top.boyut.ToString(), this.Font, Brushes.Black, (float)top.x,
                            (float)top.y);
                        toplar[i] = top;
                    }

                    RectangleF sonrakiTopRectangle =
                        new RectangleF(panel1.Width / 2 - sonrakiTur * 10, 40, sonrakiTur * 20, sonrakiTur * 20);

                    Invoke((Action)(() => //farklı threadden erişim sağlandığı için invoke
                    {
                        graphicsBuffer.DrawImage(resimler[sonrakiTur - 1], sonrakiTopRectangle);
                    }));
                    graphics.DrawImage(bitmapBuffer, Point.Empty);

                    fpsSayac++;
                    //Thread.Sleep(3);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        bool carpismaKontrol(int topIndex1, int topIndex2)
        {
            Top top1 = toplar[topIndex1];
            Top top2 = toplar[topIndex2];
            if (top1.y <= 0 || top2.y <= 0)
                Invoke((Action)(() =>
                {
                    backgroundWorker1.CancelAsync();
                    button1.Enabled = true;
                }));
            double uzaklik = Math.Sqrt(Math.Pow(top1.x - top2.x, 2) + Math.Pow(top1.y - top2.y, 2));
            if (uzaklik < top1.boyut + top2.boyut)//yarıçaptan temas kontrolü
            {
                if (top1.tur == top2.tur && top1.tur < 11)
                {
                    skor += top1.tur*10;
                    Invoke((Action)(() =>
                    {
                        skorun.Text = "Skorun: " + skor;
                    }));
                    toplar.RemoveAt(topIndex1);
                    toplar.RemoveAt(topIndex2 - 1);
                    Top topYeni = new Top();
                    topYeni.tur = top1.tur + 1;
                    //r^2son=r1^2+r2^2
                    //r= sqrt(r^21*r^22
                    topYeni.boyut = topYeni.tur * 10;
                    //topYeni.boyut = top1.boyut + top2.boyut;
                    topYeni.x = (top1.x+top2.x)/2;
                    topYeni.y = (top1.y+top2.y)/2;
                    topYeni.ivmex = (top1.ivmex+top2.ivmex)/2;
                    topYeni.ivmey = (top1.ivmey + top2.ivmey) / 2;
                    //topYeni.kutle = topYeni.boyut;
                    topYeni.kutle = Math.PI * Math.Pow(topYeni.boyut, 2);

                    toplar.Add(topYeni);
                    return true;//toplar birleşti
                }
                double ustUsteKisim = (top1.boyut + top2.boyut) - uzaklik;

                //toplar arasındaki vektör oluşturulup uzaklığa bölünerek
                //0 ile 1 arasına oranlanır
                double vektorx = (top2.x - top1.x) / uzaklik;
                double vektory = (top2.y - top1.y) / uzaklik;


                //topları ayır
                top1.x -= (float)(vektorx * ustUsteKisim / 2);
                top1.y -= (float)(vektory * ustUsteKisim / 2);
                top2.x += (float)(vektorx * ustUsteKisim / 2);
                top2.y += (float)(vektory * ustUsteKisim / 2);

                float ivmeFarkX = (float)(top2.ivmex - top1.ivmex);
                float ivmeFarkY = (float)(top2.ivmey - top1.ivmey);

                float ayirilacakIvme = ivmeFarkX * (float)vektorx + 
                                            ivmeFarkY * (float)vektory;

                // birbirlerinden uzaklaşıyorlarsa ayırma
                if (ayirilacakIvme > 0)
                    return false;

                float itmeKuvvet = -(2) * ayirilacakIvme;
                itmeKuvvet /= 1 / (float)(top1.kutle) + 1 / (float)(top2.kutle);

                float itmeKuvvetX = itmeKuvvet * (float)vektorx;
                float itmeKuvvetY = itmeKuvvet * (float)vektory;

                top1.ivmex -= itmeKuvvetX / top1.kutle;
                top1.ivmey -= itmeKuvvetY / top1.kutle;
                top2.ivmex += itmeKuvvetX / top2.kutle;
                top2.ivmey += itmeKuvvetY / top2.kutle;
                toplar[topIndex1] = top1;
                toplar[topIndex2] = top2;
            }

            return false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = "FPS: " + fpsSayac.ToString("0.00");
            fpsSayac = 0;
        }


        private void panel1_Click(object sender, EventArgs e)
        {


            Top yeni = new Top();
            yeni.tur = sonrakiTur;
            yeni.boyut = yeni.tur * 10;
            yeni.x =((MouseEventArgs)e).X + (float)rnd.NextDouble();
            yeni.y = 1;
            yeni.ivmex = 0;
            yeni.ivmey = -4;
            //yeni.kutle = yeni.boyut;
            yeni.kutle = Math.PI*Math.Pow(yeni.boyut,2);
            toplar.Add(yeni);
            sonrakiTur = rnd.Next(1, 4);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 11; i++)
            {
                resimler[i] =
                    ((PictureBox)
                        (this.Controls.Find("pictureBox"+(i+1).ToString(),
                            true)[0])).Image;
            }
            ;
        }
    }
}
