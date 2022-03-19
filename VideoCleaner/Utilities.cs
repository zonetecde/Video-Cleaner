using Hazdryx.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SharpAvi;
using SharpAvi.Output;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WpfApp3
{
    class Utilities
    {
        private static List<FastBitmap> bitmaps;
        private static bool Debug = false;
        private static int DebugWidth = 640;
        private static int DebugHeight = 480;

        public static void VideoToBitmap(string filename, int precision, bool debug = false)
        {
            Précision = precision;
            Debug = debug;

            // Bitmap finalBitmap = new Bitmap(null);
            Mat mat = new Mat();

            VideoCapture videoCapture = new VideoCapture(filename);

            bitmaps = new List<FastBitmap>();

            for (int i = 10; i < videoCapture.FrameCount; i += videoCapture.FrameCount/Précision)
            {
                double t = ((double)i / (double)(videoCapture.FrameCount)) * 100;
                MainWindow.backgroundWorker.ReportProgress(2, t);

                try
                {
                    videoCapture.Read(mat);
                    videoCapture.Set(CaptureProperty.PosFrames, i);
                    videoCapture.Read(mat);

                    if(Debug)
                    {
                        FastBitmap btm = new FastBitmap(new Bitmap(mat.ToBitmap(), new System.Drawing.Size(DebugWidth, DebugHeight)));
                        bitmaps.Add(btm);
                    }
                    else
                        bitmaps.Add(new FastBitmap(mat.ToBitmap()));
                }
                catch { }
            }


            int xMax = videoCapture.FrameWidth;
            if (Debug)
                xMax = DebugWidth;
            int yMax = videoCapture.FrameHeight;
            if (Debug)
                yMax = DebugHeight;

            FastBitmap bitmap = new FastBitmap(xMax, yMax);

            int i2 = 0;

            for (int x = 0; x < xMax; x++)
            {
                if (i2 % 50 == 0)
                {
                    double t = ((double)x / (double)xMax) * 100;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        MainWindow.backgroundWorker.ReportProgress(0, new PourcentAndPreview(t, (Bitmap)bitmap.BaseBitmap.Clone()));
                    }));

                    i2++;
                }
                else
                {
                    i2++;
                }

                for (int y = 0; y < yMax; y++)
                {
                    try
                    {
                        Color colorQuiRevientLePlus = CouleurQuiRevientLePlus(x, y);

                        bitmap.Set(x, y, colorQuiRevientLePlus);
                    }
                    catch { }
                }              
            }

            MainWindow.backgroundWorker.ReportProgress(3, new PourcentAndPreview(100, (Bitmap)bitmap.BaseBitmap.Clone()));

            bitmap.Save(@"F:\source\clash royal ai\resources\test.png");
        }

        internal static int Précision;

        public static Color[] ColorArray { get; private set; }

        private static Color CouleurQuiRevientLePlus(int x, int y)
        {
            List<Color> allColor = new List<Color>();

            for(int i = 0; i < Précision; i += (bitmaps.Count / Précision))
            {
                try
                {
                    allColor.Add(bitmaps[i].Get(x, y));
                }
                catch { }
            }

            //return Color.FromArgb(Convert.ToInt32(allColor.Average(item => item.R)), Convert.ToInt32(allColor.Average(item => item.G)), Convert.ToInt32(allColor.Average(item => item.B)));
            
            Color couleur = (from item in allColor
                    group item by item into g
                orderby g.Count() descending
                select g.Key).First();

            if(couleur.R == 255 && couleur.G == 255 && couleur.B == 255)
            {
                couleur = /*DarkerColor*/(Color.FromArgb(Convert.ToInt32(allColor.Average(item => item.R)), Convert.ToInt32(allColor.Average(item => item.G)), Convert.ToInt32(allColor.Average(item => item.B))));
            }

            return couleur;
        }

        private static Color DarkerColor(Color color, float correctionfactory = 50f)
        {
            const float hundredpercent = 100f;
            return Color.FromArgb((int)(((float)color.R / hundredpercent) * correctionfactory),
                (int)(((float)color.G / hundredpercent) * correctionfactory), (int)(((float)color.B / hundredpercent) * correctionfactory));
        }

        public static Bitmap GetVideoThumbail(string filename)
        {
            OpenCvSharp.VideoCapture videoCapture = new OpenCvSharp.VideoCapture(filename);
            OpenCvSharp.Mat mat = new OpenCvSharp.Mat();
            videoCapture.Read(mat);
            videoCapture.Set(CaptureProperty.PosFrames, videoCapture.FrameCount / 2);
            videoCapture.Read(mat);

            return mat.ToBitmap();
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static System.Windows.Media.ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        /// <summary>
        /// Noir sur tout ce qui n'est pas les voitures
        /// </summary>
        /// <param name="v"></param>
        internal static void GetVideoAI(string v, FastBitmap bkg)
        {
            List<Color> allColors = new List<Color>();
            allColors.Add(Color.Black);
            allColors.Add(Color.White);
            allColors.Add(Color.Gray);

            ColorArray = allColors.ToArray();

            VideoCapture videoCapture = new VideoCapture(v);
            List<Bitmap> btms = new List<Bitmap>();
            OpenCvSharp.Mat mat = new OpenCvSharp.Mat();

            for (int i = 0; i < videoCapture.FrameCount; i++)
            {

                videoCapture.Read(mat);
                videoCapture.Set(CaptureProperty.PosFrames, i);
                videoCapture.Read(mat);

                FastBitmap bmp = new FastBitmap(BitmapConverter.ToBitmap(mat));
                                
                videoCapture.Read(mat);
                videoCapture.Set(CaptureProperty.PosFrames, i + 1);
                videoCapture.Read(mat);

                FastBitmap bmp2 = new FastBitmap(BitmapConverter.ToBitmap(mat));

                for (int x = 0; x < mat.Width; x++)
                {
                    for (int y = 0; y < mat.Height; y++)
                    {
                        if ((bmp.Get(x,y)) == (bmp2.Get(x, y)))
                        {
                            bmp.Set(x, y, Color.Black);
                        }
                    }
                }

                bmp.Save(@"C:\Users\rayane staszewski\source\repos\img\image_" + i + ".png", ImageFormat.Png);
                bmp.Dispose();
            }




            CreateMovie(Directory.GetFiles(@"C:\Users\rayane staszewski\Documents\outpuy\img").ToList(), videoCapture.FrameWidth, videoCapture.FrameHeight, Convert.ToInt32(videoCapture.Fps));
        }

        private static int GetDiff(Color color, Color baseColor)
        {
            int a = color.A - baseColor.A,
                r = color.R - baseColor.R,
                g = color.G - baseColor.G,
                b = color.B - baseColor.B;
            return a * a + r * r + g * g + b * b;
        }

        private static Color GetClosestColor(Color baseColor)
        {
            var colors = ColorArray.Select(x => new { Value = x, Diff = GetDiff(x, baseColor) }).ToList();
            var min = colors.Min(x => x.Diff);
            return colors.Find(x => x.Diff == min).Value;
        }


        private static void CreateMovie(List<string> frames, int w, int h, int fps)
        {
            int width = w;
            int height = h;
            var framRate = fps;

            var writer = new AviWriter(@"C:\Users\rayane staszewski\Documents\outpuy\t.avi")
            {
                FramesPerSecond = framRate,
                EmitIndex1 = true
            };

            var stream = writer.AddVideoStream();
            stream.Width = width;
            stream.Height = height;
            stream.Codec = KnownFourCCs.Codecs.Xvid;
            stream.BitsPerPixel = BitsPerPixel.Bpp32;

            foreach (var frame in frames)
            {
                Bitmap bmp = new Bitmap(frame);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);
                byte[] byteImage = ms.ToArray();

                byte[] arr = Convert.FromBase64String(Convert.ToBase64String(byteImage));
                stream.WriteFrame(true, arr, 0, arr.Length);

                ms.Dispose();
                bmp.Dispose();
            }

            writer.Close();
        }

        public static Bitmap ToBitmap(byte[] byteArrayIn)
        {
            var ms = new MemoryStream(byteArrayIn);
            var returnImage = Image.FromStream(ms);
            var bitmap = new Bitmap(returnImage);

            return bitmap;
        }

        public static Bitmap ReduceBitmap(Bitmap original, int reducedWidth, int reducedHeight)
        {
            var reduced = new Bitmap(reducedWidth, reducedHeight);
            using (var dc = Graphics.FromImage(reduced))
            {
                dc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                dc.DrawImage(original, new Rectangle(0, 0, reducedWidth, reducedHeight), new Rectangle(0, 0, original.Width, original.Height), GraphicsUnit.Pixel);
            }

            return reduced;
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                {
                    bitmap.UnlockBits(bmpdata);
                }
            }
        }

        private static void ExecuteArg(List<string> lists)
        {
            throw new NotImplementedException();
        }

        public static byte[] ImageToByteArray(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
