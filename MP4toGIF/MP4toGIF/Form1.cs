///////////////////////////////////////////////////////////////////////////////////////////////////
///  Project: MP4toGif
///  Created: April 08, 2025
///  Author: Cole Yaremko - YaremkoC2 on github
/// 
///  Simple project to create a forms app that allows a user to drag and drop an MP4 onto a forms
///  app to create a GIF. - Nothing revolutionary here, using other people's libraries. Mostly
///  just wanted to have something of my own to use for this purpose.
///  
///  Modification History: See Git
///  Program status: Completed
///////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using AnimatedGif;
using OpenCvSharp;
using System.Threading;

namespace MP4toGIF
{
    public partial class MP4toGIF : Form
    {
        public MP4toGIF()
        {
            InitializeComponent();

            // Add Drag Drop Controls
            AllowDrop = true;
            DragEnter += new DragEventHandler(MP4toGIF_DragEnter);
            DragDrop += new DragEventHandler(MP4toGIF_DragDrop);
            DragLeave += new EventHandler(MP4toGIF_DragLeave);
        }

        void MP4toGIF_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Change the form background color to indicate drag state
                BackColor = Color.LightGreen;
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                // Reset background color if no valid data
                BackColor = SystemColors.Control;
            }
        }

        void MP4toGIF_DragLeave(object? sender, EventArgs e)
        {
            // Reset the background color when drag leaves the form
            BackColor = SystemColors.Control;
        }

        void MP4toGIF_DragDrop(object? sender, DragEventArgs e)
        {
            string mainFileName;  // store the file name of the first dropped file

            // take the first dropped file only
            string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files?.Length > 0)
            {
                mainFileName = files[0];

                // if the file is not an mp4 tell the user and return
                if (!mainFileName.EndsWith(".mp4"))
                {
                    BackColor = Color.FromArgb(255, 181, 65, 65);
                    MessageBox.Show("Please drop an mp4");
                    BackColor = SystemColors.Control;
                    return;
                }

                // run the conversion in the background
                try
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(ConvertVideoToGif));
                    thread.IsBackground = true;  
                    thread.Start(mainFileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void ConvertVideoToGif(object? obj)
        {
            if (!(obj is string mainFileName)) return;  // make sure it's a string

            try
            {
                // Open the video or tell the user it's failed
                using var capture = new VideoCapture(mainFileName);
                if (!capture.IsOpened())
                {
                    throw new Exception("Failed to open video...");
                }

                // Get the new file name and frame delay
                string gifFileName = Path.ChangeExtension(mainFileName, ".gif");
                double fps = capture.Fps;
                int delayMs = (int)(1000.0 / fps);

                // Convert the video frames to a gif
                using (var gif = AnimatedGif.AnimatedGif.Create(gifFileName, delayMs))
                {
                    Mat frame = new Mat();
                    while (capture.Read(frame))
                    {
                        if (frame.Empty()) break;

                        // Convert Mat to Bitmap
                        Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);

                        // Add frame to the GIF and dispose of the bitmap immediately
                        gif.AddFrame(bitmap, delay: -1, quality: GifQuality.Bit8);
                        bitmap.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                // make the color red and show the error message
                Invoke(new Action(() =>
                {
                    BackColor = Color.FromArgb(255, 181, 65, 65);
                    MessageBox.Show("Error during conversion: " + ex.Message);
                }));
            }
            finally
            {
                // final clean up and restore back color
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Invoke(new Action(() => BackColor = SystemColors.Control));
            }
        }
    }
}
