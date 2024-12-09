using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;

namespace QRCodeScanner
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices; 
        private VideoCaptureDevice videoSource;    
        private Timer scanTimer;                   

        public Form1()
        {
            InitializeComponent();

            
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Камера не найдена!");
                return;
            }

            
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;

            
            scanTimer = new Timer
            {
                Interval = 500 // Интервал проверки (500 мс)
            };
            scanTimer.Tick += ScanTimer_Tick;

            videoSource.Start(); // Запуск камеры
            scanTimer.Start();   // Запуск таймера
        }

        private Bitmap currentFrame;

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            
            currentFrame?.Dispose();
            currentFrame = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = currentFrame; // Отображение кадра на PictureBox
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            if (currentFrame == null) return;

            try
            {
               
                var reader = new BarcodeReader();
                var result = reader.Decode(currentFrame);

                if (result != null)
                {
                    scanTimer.Stop();
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();

                    MessageBox.Show($"QR-код успешно считан: {result.Text}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сканировании: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
           
            if (videoSource?.IsRunning == true)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }
    }
}