///////////////////////////////////////////////////////////////////////////////////////////////////
///  Project: MP4toGif
///  Created: April 08, 2025
///  Author: Cole Yaremko - YaremkoC2 on github
/// 
///  Simple project to create a forms app that allows a user to drag and drop an MP4 onto a forms
///  app to create a GIF. - Nothing revolutionary here, using other people's libraries mostly
///  just wanted to have something of my own to use for this purpose.
///  
///  Modification History: See Git
///  Program status: In Progress
///////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using AnimatedGif;
using System.IO;
using System.Diagnostics;

namespace MP4toGIF
{
    public partial class Form1 : Form
    {
        string? mainFileName;  // store the file name of the first dropped file

        public Form1()
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
                    MessageBox.Show("Please drop an excel file");
                    mainFileName = null;
                    BackColor = SystemColors.Control;
                    return;
                }

                
                
            }
        }
    }
}
