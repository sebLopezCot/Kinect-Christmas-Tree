using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;

namespace Kinect_ChristmasTreeing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Runtime nui;
        const int exitPoint = 1000;
        int exitPointOffset = 3;
        Boolean firing = false;

        #region Initialize

        private void SetupKinect() 
        {
            if (Runtime.Kinects.Count == 0)
            {
                this.Title = "No Kinect connected";
            }
            else
            {
                // use the first kinect
                nui = Runtime.Kinects[0];
                nui.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect();
            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_VideoFrameReady);
            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            
            // parameters for smoothing the skeleton data
            nui.SkeletonEngine.TransformSmooth = true;
            TransformSmoothParameters parameters = new TransformSmoothParameters();
            parameters.Smoothing = 0.5f;
            parameters.Correction = 0.3f;
            parameters.Prediction = 0.4f;
            parameters.JitterRadius = 0.7f;
            nui.SkeletonEngine.SmoothParameters = parameters;

            nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution1280x1024, ImageType.Color);

            treeImageContainer.Visibility = Visibility.Visible;
        }

        #endregion

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame allSkeletons = e.SkeletonFrame;

            try
            {
                // makes tree visible
                treeImageContainer.Visibility = Visibility.Visible;

                // get the first tracked skeleton
                SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                         where s.TrackingState == SkeletonTrackingState.Tracked
                                         select s).FirstOrDefault();

                // set position of tree
                SetTreePosition(treeImageContainer, skeleton.Joints[JointID.HandRight]);
                
            }
            catch (Exception ex) 
            {
                treeImageContainer.Visibility = Visibility.Hidden;
            }
        }

        private void SetTreePosition(Image treeImage, Joint joint)
        {
            var scaledJoint = joint.ScaleTo(2560, 2048, 0.7f, 0.7f);

            Canvas.SetLeft(treeImage, scaledJoint.Position.X - 1000);
            Canvas.SetTop(treeImage, scaledJoint.Position.Y - 700);
        }

        void nui_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            rgbImageContainer.Source = e.ImageFrame.ToBitmapSource();
        }

        #region Uninitialize

        private void Window_Closed(object sender, EventArgs e)
        {
            nui.Uninitialize();
        }

        #endregion
    }
}
