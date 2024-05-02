using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace LiveModule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private VideoCapture capture;
        private Mat frame;
        private BitmapSource bitmap, croppedBitmap;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            bitmap = new BitmapImage();
            croppedBitmap = new BitmapImage();
            capture = new VideoCapture(0);
            frame = new Mat();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000.0 / 30.0)
            };

            // Set cropMask rectangle position
            Canvas.SetLeft(cropMask, 0);
            Canvas.SetTop(cropMask, 0);
            Canvas.SetRight(cropMask, 0);
            Canvas.SetBottom(cropMask, 0);

            // Set cropMask rectangle size
            cropMask.Width = image.ActualWidth;
            cropMask.Height = image.ActualHeight;

            // Set cropMask rectangle color
            cropMask.Stroke = Brushes.Green;

            // Set cropMask rectangle thickness
            cropMask.StrokeThickness = 2;

            // Set cropMask rectangle opacity
            cropMask.Opacity = 0.75;

            // Set cropMask rectangle border
            cropMask.StrokeDashArray = new DoubleCollection(new double[] { 2, 2 });
            cropMask.StrokeDashOffset = 0;

            // Set cropMask rectangle border color
            cropMask.StrokeDashCap = PenLineCap.Round;

            // Set cropMask rectangle border style
            cropMask.StrokeDashCap = PenLineCap.Round;

            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if(capture.Read(frame))
            {
                UpdateCrop();

                bitmap = frame.ToBitmapSource();
                image.Source = bitmap;

                // Get the cropMask rectangle for cropping
                var cropRect = new System.Windows.Rect(Canvas.GetLeft(cropMask),
                                                      Canvas.GetTop(cropMask),
                                                      cropMask.Width,
                                                      cropMask.Height);

                // Check if the crop rectangle is within the image bounds
                if(cropRect.X < 0)
                {
                    cropRect.X = 0;
                } else if(cropRect.X + cropRect.Width > bitmap.PixelWidth)
                {
                    cropRect.Width = bitmap.PixelWidth - cropRect.X;
                }
                if(cropRect.Y < 0)
                {
                    cropRect.Y = 0;
                } else if(cropRect.Y + cropRect.Height > bitmap.PixelHeight)
                {
                    cropRect.Height = bitmap.PixelHeight - cropRect.Y;
                }

                // Crop the image using the cropMask and update its source
                croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect((int)cropRect.X, (int)cropRect.Y, (int)cropRect.Width, (int)cropRect.Height));

                croppedImage.Source = croppedBitmap;
            }
        }

        private void UpdateCrop()
        {
            // Update the width of the crop rectangle using the slider value(percentage)
            cropMask.Width = widthSlider.Value * (image.ActualWidth / 100);
            cropMask.Height = heightSlider.Value * (image.ActualHeight / 100);
        }

        private void widthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Canvas.SetLeft(cropMask, (image.ActualWidth - cropMask.Width) / 2);
            Canvas.SetRight(cropMask, (image.ActualWidth - cropMask.Width) / 2);
            Canvas.SetBottom(cropMask, (image.ActualHeight - cropMask.Height) / 2);
            Canvas.SetTop(cropMask, (image.ActualHeight - cropMask.Height) / 2);
        }

        private void heightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Canvas.SetLeft(cropMask, (image.ActualWidth - cropMask.Width) / 2);
            Canvas.SetRight(cropMask, (image.ActualWidth - cropMask.Width) / 2);
            Canvas.SetBottom(cropMask, (image.ActualHeight - cropMask.Height) / 2);
            Canvas.SetTop(cropMask, (image.ActualHeight - cropMask.Height) / 2);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Save both the original and cropped images
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JPEG Image|*.jpg",
                Title = "Save an Image File"
            };

            if(saveFileDialog.ShowDialog() == true)
            {
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using(var fileStream = new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }

                var croppedSaveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JPEG Image|*.jpg",
                    Title = "Save an Image File"
                };

                if(croppedSaveFileDialog.ShowDialog() == true)
                {
                    var croppedEncoder = new JpegBitmapEncoder();
                    croppedEncoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
                    using(var fileStream = new System.IO.FileStream(croppedSaveFileDialog.FileName, System.IO.FileMode.Create))
                    {
                        croppedEncoder.Save(fileStream);
                    }
                }

                MessageBox.Show("Images saved successfully!");
            }
        }
    }
}