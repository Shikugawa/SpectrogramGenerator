using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading.Tasks;
using Sound.WAVE;
using Signal.FrequencyAnalysis;
using Graphic.ColorClass;

namespace SpectrogramWithFft
{
    public partial class Form1 : Form
    {
        WaveReader reader = new WaveReader();

        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(
                () =>
                {
                    string basePath = @"C:\Users\shikugawa\Downloads\test\audio";
                    IEnumerable<string> files = Directory.EnumerateFiles(basePath, "*", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        Console.WriteLine(file);
                        var regexp = new Regex(@"audio\\(.+)\\(.+)");
                        var matchedData = regexp.Match(file);

                        var folderName = matchedData.Groups[1];
                        var fileName = matchedData.Groups[2];

                        if (!Directory.Exists("C:\\Users\\shikugawa\\Documents\\Test\\" + folderName))
                        {
                            Directory.CreateDirectory("C:\\Users\\shikugawa\\Documents\\Test\\" + folderName);
                        }

                        this.reader.Open(file);


                        if (this.reader.IsOpen && this.reader.ReadLimit == false)
                        {
                            int i = 0;
                            List<FFTresult> listBuff = new List<FFTresult>();
                            while (i < 16)                                   
                            {
                                var data = this.reader.Read(1024);  
                                double[] sound;
                                if (data.UsableCH == MusicData.UsableChannel.Left)
                                    sound = data.GetData(MusicData.Channel.Left);
                                else
                                    sound = data.GetData(MusicData.Channel.Right);

                                var dft = new DFT(sound.Length, Window.WindowKind.Hanning);
                                FFTresult result = dft.FFT(sound, (long)this.reader.SamplingRate);
                                listBuff.Add(result);

                                i++;
                            }
                            
                            double max = double.MinValue;
                            for (int j = 0; j < listBuff.Count; j++)
                                if (max < listBuff[j].MaxAbs) max = listBuff[j].MedianAbs;
                            int height = 512;

                            Bitmap img = new Bitmap(listBuff.Count, height);

                            foreach(var l in listBuff)
                            {
                                Debug.WriteLine(l);
                            }

                            Colormap cmap = new Colormap();
                            for (int k = 0; k < height; k++)
                            {
                                for (int j = 0; j < listBuff.Count; j++)
                                {
                                    int index = listBuff[j].Length * k / height;
                                    double power = 3.0;             
                                    var value = (Math.Log10(listBuff[j].GetData(index).data.Abs / max) + power) / (power * 2.0);
                                    if (value > 1.0) value = 1.0;
                                    if (value < 0.0) value = 0.0;
                                    Color c = cmap.GetColor(value);
                                    
                                        img.SetPixel(j, height - k - 1, c);
                                    
                                }
                            }

                            Action appendText = () => resultMessageBox.AppendText("succeeded!: FolderName->" + folderName + " FileName->" + fileName + "\n");
                            Invoke(appendText);
                            img.Save("C:\\Users\\shikugawa\\Documents\\Test\\" + folderName + "\\" + fileName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            );
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
