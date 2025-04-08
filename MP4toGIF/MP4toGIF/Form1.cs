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

namespace MP4toGIF
{
    public partial class MP4toGIF : Form
    {
        string? mainFileName;  // store the file name of the first dropped file
        List<Bitmap> bitmaps;  // stores the frames of the video

        public MP4toGIF()
        {
            InitializeComponent();

            // Add Drag Drop Controls
            AllowDrop = true;
            DragEnter += new DragEventHandler(MP4toGIF_DragEnter);
            DragDrop += new DragEventHandler(MP4toGIF_DragDrop);
            DragLeave += new EventHandler(MP4toGIF_DragLeave);

            // initialize
            bitmaps = new List<Bitmap>();
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
            // take the first dropped file only
            string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files?.Length > 0)
            {
                // only take the first file
                mainFileName = files[0];

                // if the file is not an mp4 tell the user and return
                if (!mainFileName.EndsWith(".mp4"))
                {
                    BackColor = Color.FromArgb(255, 181, 65, 65);
                    MessageBox.Show("Please drop an mp4");
                    mainFileName = null;
                    BackColor = SystemColors.Control;
                    return;
                }

                try
                {
                    // Open the video or tell the user it's failed
                    using var capture = new VideoCapture(mainFileName);
                    if (!capture.IsOpened())
                    {
                        BackColor = Color.FromArgb(255, 181, 65, 65);
                        MessageBox.Show("Failed to open video...");
                        mainFileName = null;
                        BackColor = SystemColors.Control;
                        return;
                    }

                    // Read the video and convert frames to bitmap
                    Mat frame = new Mat();
                    while (capture.Read(frame))
                    {
                        if (frame.Empty()) break;

                        // Convert Mat to Bitmap and add to list
                        Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                        bitmaps.Add(bitmap);
                    }

                    // Get the new file name and frame delay
                    string gifFileName = Path.ChangeExtension(mainFileName, ".gif");
                    double fps = capture.Fps;
                    int delayMs = (int)(1000.0 / fps);

                    // convert the bitmaps to a gif
                    using (var gif = AnimatedGif.AnimatedGif.Create(gifFileName, delayMs))
                    {
                        foreach (var bitmap in bitmaps)
                        {
                            gif.AddFrame(bitmap, delay: -1, quality: GifQuality.Bit8);
                        }
                    }
                }
                catch (Exception ex)
                {
                    BackColor = Color.FromArgb(255, 181, 65, 65);
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    BackColor = SystemColors.Control;
                }
            }
        }
    }
}
