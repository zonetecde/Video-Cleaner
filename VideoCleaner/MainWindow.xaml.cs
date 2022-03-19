using Hazdryx.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp3
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filename;
        private int precision = 100;
        public static BackgroundWorker backgroundWorker;
        private bool Debug = false;
        private FastBitmap finalResult;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                progressBar_cleaner.Value = (e.UserState as PourcentAndPreview).pourcent;
                label_pourcentage.Content = Math.Round((e.UserState as PourcentAndPreview).pourcent, 1) + "%";
                img_preview.Source = Utilities.ImageSourceFromBitmap((e.UserState as PourcentAndPreview).img);
            }
            else if (e.ProgressPercentage == 1)
            {
                progressBar_cleaner_2.Visibility = Visibility.Visible;
                progressBar_cleaner_2.Value = (double)e.UserState;
            }
            else if(e.ProgressPercentage == 2)
            {
                progressBar_cleaner.Value = (double)e.UserState;
                label_pourcentage.Content = "Chargement de la vidéo... : " + Math.Round((double)e.UserState) + "%";
            }
            else
            {
                progressBar_cleaner.Value = 100;
                label_pourcentage.Content = "Chargement de la vidéo... : 100%";
                progressBar_cleaner_2.Visibility = Visibility.Hidden;
                img_preview.Source = Utilities.ImageSourceFromBitmap((e.UserState as PourcentAndPreview).img);
                finalResult = new FastBitmap((e.UserState as PourcentAndPreview).img);

                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        finalResult.BaseBitmap.Save(fbd.SelectedPath + "\\output.png");
                    }
                }
            }

        }

        private void slider_precision_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (label_precision != null)
                {
                    label_precision.Content = "Précision : " + slider_precision.Value;
                    precision = Convert.ToInt32(slider_precision.Value);

                    if (img_video.Tag.Equals(true))
                        label_prix.Content = "Prix : " + (precision * (img_video.Source.Width * img_video.Source.Height) / 500000);
                }
            }
            catch { }
        }

        /// <summary>
        /// Importer vidéo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".mp4";
            dlg.Filter = "MP4 Files (*.mp4)|*.mp4|AVI Files (*.avi)|*.avi";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                label_video.Content = "Vidéo : " + filename;

                this.filename = filename;

                img_video.Tag = true;
                img_video.Source = Utilities.ImageSourceFromBitmap(Utilities.GetVideoThumbail(filename));

                label_prix.Content = "Prix : " + (precision * (img_video.Source.Width * img_video.Source.Height) / 500000);
            }
        }
        
        /// <summary>
        /// Cleaner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(filename))
            {
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
                backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
                backgroundWorker.WorkerReportsProgress = true;

                Button_cleaner.Visibility = Visibility.Hidden;
                progressBar_cleaner.Visibility = Visibility.Visible;
                label_pourcentage.Visibility = Visibility.Visible;

                backgroundWorker.RunWorkerAsync();
                
            }
        }
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Utilities.VideoToBitmap(filename, precision, Debug);
        }

        private void checkBox_debug_Checked(object sender, RoutedEventArgs e)
        {
            Debug = true;
        }

        private void checkBox_debug_Unchecked(object sender, RoutedEventArgs e)
        {
            Debug = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // debug - 
            finalResult = new FastBitmap(System.Drawing.Image.FromFile(@"C:\Users\rayane staszewski\Documents\precision_500_19m30s.png"));
            // -------

            // video 
            Utilities.GetVideoAI(@"C:\Users\rayane staszewski\Documents\autoroute 15sec.mp4", finalResult);
        }
    }
}
