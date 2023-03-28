using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Accord.Video.FFMPEG;
using AForge.Video;
using AForge.Video.DirectShow;
using Accord.Video.VFW;
using System.Threading;
using System.Xml.Serialization;

namespace VideoCapture
{
    public partial class videoCaptureForm : Form
    {
        
        public videoCaptureForm()
        {
            InitializeComponent();
        }
        private static VideoCaptureDevice videoSource = null;
        private VideoFileWriter writer = null;
        FilterInfoCollection videoDevices;
        internal static int frameCount = 0;
        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in videoDevices)
                comboBox1.Items.Add(filterInfo.Name);
            comboBox1.SelectedIndex = 0;
        }
        void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap image = (Bitmap)eventArgs.Frame.Clone();
            if (!button2.Enabled)
            {
                writer.WriteVideoFrame(image);
                frameCount++;
            }
            if (pix.InvokeRequired)
            {
                try
                {
                    pix.Invoke(new MethodInvoker(delegate { pix.Image = image; }));
                }
                catch { }
            }
            else
            {
                pix.Image = image;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            videoSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
            videoSource.Start();
            writer = new VideoFileWriter();
            writer.Open("video.avi", 640, 480, 30, VideoCodec.MPEG4);
            Thread counterUpdates = new Thread(() =>
            {
                videoCaptureForm form = new videoCaptureForm();
                counterUpdater(form.label3);
            });
            counterUpdates.Start();
        }
        internal static void counterUpdater(Label lbl)
        {
            while (videoSource != null)
            {
                lbl.Text = "Frames captured: " + frameCount;
                Thread.Sleep(10);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource = null;
            }
            if (writer != null && writer.IsOpen)
            {
                writer.Close();
                writer = null;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            videoCaptureForm form = new videoCaptureForm();
            if (form.Width < 454)
                form.Width = 454;
            if (form.Height < 521)
                form.Height = 521;
            PerformLayout();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false; button3.Enabled = true;
            label2.Text = "Recording...";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button2.Enabled = true; button3.Enabled = false;
            label2.Text = "Recording stopped";
        }

        private void videoCaptureForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource = null;
            }
            if (writer != null && writer.IsOpen)
            {
                writer.Close();
                writer = null;
            }
        }
    }
}