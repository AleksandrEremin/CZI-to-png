// Tutorial sample : SimpleImageViewer

namespace WBa.SimpleImageViewer
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;
    using Zeiss.Micro;
    using Zeiss.Micro.Diagnostics;
    using Zeiss.Micro.Imaging;
    using Zeiss.Micro.Imaging.View;
    using System.Windows.Media.Imaging;
    using System.IO;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ImageDocument imageDocument = new ImageDocument();
        ImageAccessorCollection accessors;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnFrameValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.UpdateDisplay();
        }

        /// <summary>
        /// Access via typed Array.
        /// </summary>
        private void Access1()
        {
            foreach (ImageAccessor subset2d in this.imageDocument.CreateAccessor().Split2d())
            {
                Gray8Pixel[] pixels = new Gray8Pixel[subset2d.Bounds.SizeX * subset2d.Bounds.SizeY];
                subset2d.CopyPixelsToArray(pixels, PixelType.Gray8);
                for (int i = 0; i < Math.Min(pixels.Length, 500000); i++)
                {
                    pixels[i].Value = (byte)(pixels[i].Value + 10);
                }

                subset2d.CopyPixelsFromArray(pixels, PixelType.Gray8);
            }

            this.UpdateDisplay();
        }

        /// <summary>
        /// Access via callback (uses buffer data directly).
        /// </summary>
        private void Access2()
        {
            foreach (var subBlock in this.imageDocument.SubBlocks)
            {
                subBlock.DoMemoryOperation(delegate(object parameter, IntPtr buffer, int bufferSize)
                {
                    unsafe
                    {
                        byte* p = (byte*)buffer.ToPointer();
                        for (int i = 0; i < bufferSize; i++)
                        {
                            p[i] = (byte)(p[i] + 10);
                        }
                    }
                }, null, 0, subBlock.DataSize, 1000, MemoryAccessFlags.Read | MemoryAccessFlags.Write);
            }

            this.UpdateDisplay();
        }

        /// <summary>
        /// Updates the display.
        /// </summary>
        private void UpdateDisplay()
        {
            this.image.Source = this.accessors[(int)this.frames.Value].GetBitmapSource();
            this.info.Text = this.accessors[(int)this.frames.Value].Bounds.InfoText;
        }

        private enum SubsetMode
        {
            M2D,
            M2DPalette,
            M3D1,
            M3D2,
        }

        /// <summary>
        /// Loads the image specified by the current fileName.
        /// </summary>
        /// <param name="mode">The Subset Ccreation mode.</param>
        private void Load(SubsetMode mode)
        {
            this.imageDocument.Open(this.fileName.Text);

            if (mode == SubsetMode.M2D)
            {
                this.accessors = this.imageDocument.CreateAccessor().Split2d();
            }
            else if (mode == SubsetMode.M2DPalette)
            {
                this.accessors = this.imageDocument.CreateAccessor().Split2d();
                foreach (var channel in this.imageDocument.Metadata.DisplaySetting.Channels)
                {
                    channel.PaletteName = "rainbow";
                    channel.ColorMode = ChannelColorModes.Palette;
                }
            }
            else if (mode == SubsetMode.M3D1)
            {
                this.accessors = this.imageDocument.CreateAccessor().Split3d(ImageDimension.C);
            }
            else if (mode == SubsetMode.M3D2)
            {
                int index = 0;
                this.accessors = this.imageDocument.CreateAccessor().Split3d(ImageDimension.C);
                foreach (var channel in imageDocument.Metadata.DisplaySetting.Channels)
                {
                    channel.ColorMode = ChannelColorModes.Color;
                    if (index == 0)
                    {
                        channel.Color = Colors.Green;
                    }
                    else if (index == 1)
                    {
                        channel.Color = Colors.Blue;
                    }
                    else if (index == 21)
                    {
                        channel.Color = Colors.Red;
                    }

                    index++;
                }
            }

            this.frames.Maximum = this.accessors.Count - 1;
            this.UpdateDisplay();
        }

        private void OnLoad2D1(object sender, RoutedEventArgs e)
        {
            this.Load(SubsetMode.M2D);
        }

        private void OnLoad2D2(object sender, RoutedEventArgs e)
        {
            this.Load(SubsetMode.M2DPalette);
        }

        private void OnLoad3D1(object sender, RoutedEventArgs e)
        {
            this.Load(SubsetMode.M3D1);
        }

        private void OnLoad3D2(object sender, RoutedEventArgs e)
        {
            this.Load(SubsetMode.M3D2);
        }

        private void OnAccess(object sender, RoutedEventArgs e)
        {
            this.Access1();
        }

        private void OnAccess2(object sender, RoutedEventArgs e)
        {
            this.Access2();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            BitmapSource bitmapSource = this.accessors[(int)this.frames.Value].GetBitmapSource();
            using (var fileStream = new FileStream(this.fileNameTwo.Text, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fileStream);
            }

        }

        private void fileName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void fileNameTwo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
