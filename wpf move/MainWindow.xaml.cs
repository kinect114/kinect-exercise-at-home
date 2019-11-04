using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;







namespace wpf_move
{
    public partial class MainWindow : Window
    {
        //Instantiate the Kinect runtime. Required to initialize the device.
        //IMPORTANT NOTE: You can pass the device ID here, in case more than one Kinect device is connected.
        KinectSensor sensor = KinectSensor.KinectSensors[0]; //宣告 KinectSensor



        //變數初始化定義
        byte[] pixelData;
        Skeleton[] skeletons;
        //bool isRightHelloGestureActive = false;
        //bool isLeftHelloGestureActive = false;
        //bool isRightHandOverHead = false;
        //bool isLeftHandOverHead = false;
        //bool CheckGesture_ready = false;
        bool tra = false;
        bool trb = false;
        bool trc = false;
        int a = 0;
        bool hn_color1rd = true;
        bool hn_color2rd = true;
        bool hn_color3rd = true;
        bool hn_color4rd = true;

        bool hc_color1rd = true;
        bool hc_color2rd = true;
        bool hc_color3rd = true;
        bool hc_color4rd = true;
        bool hc_color5rd = true;



        bool sq_color1rd = true;
       
        bool postrd = false;
        bool hn_open = false;
        bool hc_open = false;
        bool squat_open = false;

        int do_how_many_times =0;
     
        
        bool do_times_open;

        double level1;
        double level2;
        double level3;
        double level_now;
        bool level_open = false;

        int hour1;
        int minute1;
        int second1;
        int current_hour = 0;
        int current_minute = 0;
        int current_second =0;

       

         //動作未完成就換動作 切回來時骨架會一樣是藍色因為在切的時候只有定義大小沒有定義顏色 純屬自然現象 有需要再改
         //切次數的時候同理


















        public MainWindow()
        {
            InitializeComponent();
            successtime.Text = "0";
            //載入與卸載
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
            DispatcherTimer timer1 = new DispatcherTimer();
            DispatcherTimer timer2 = new DispatcherTimer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer2.Tick += new EventHandler(timer2_Tick);
            timer1.Interval = TimeSpan.FromSeconds(6);
            timer2.Interval = TimeSpan.FromSeconds(1);
            timer1.Start();
            timer2.Start();
            hn_point1.Height = 0;
            hn_point1.Width = 0;
            hn_point2.Height = 0;
            hn_point2.Width = 0;
            hn_point3.Height = 0;
            hn_point3.Width = 0;
            hn_point4.Height = 0;
            hn_point4.Width = 0;
            ////
            hc_point1.Height = 0;
            hc_point1.Width = 0;
            hc_point2.Height = 0;
            hc_point2.Width = 0;
            hc_point3.Height = 0;
            hc_point3.Width = 0;
            hc_point4.Height = 0;
            hc_point4.Width = 0;
            hc_point5.Height = 0;
            hc_point5.Width = 0;

          


            sensor.ColorStream.Enable();//開啟，彩色影像

            //平滑處理，防止高頻率微小抖動和突發大跳動造成的關節雜訊
            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters);//載入平滑處理參數
            sensor.SkeletonStream.Enable();//開啟，骨架追蹤

        }

        //MainWindow 載入
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            sensor.SkeletonFrameReady += runtime_SkeletonFrameReady;
            sensor.ColorFrameReady += runtime_VideoFrameReady;
            sensor.Start();
        }

        //MainWindow 卸載
        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            sensor.Stop();
        }

        //彩色影像，處理函數
        void runtime_VideoFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            bool receivedData = false;

            using (ColorImageFrame CFrame = e.OpenColorImageFrame())
            {
                if (CFrame == null)
                {
                    // The image processing took too long. More than 2 frames behind.
                }
                else
                {
                    pixelData = new byte[CFrame.PixelDataLength];
                    CFrame.CopyPixelDataTo(pixelData);
                    receivedData = true;
                }
            }

            if (receivedData)
            {
                BitmapSource source = BitmapSource.Create(640, 480, 96, 96,
                        PixelFormats.Bgr32, null, pixelData, 640 * 4);

                videoImage.Source = source;
            }
        }

        //骨架追蹤，處理函數
        void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            bool receivedData = false;

            using (SkeletonFrame SFrame = e.OpenSkeletonFrame())
            {
                if (SFrame == null)
                {
                    // The image processing took too long. More than 2 frames behind.
                }
                else
                {
                    skeletons = new Skeleton[SFrame.SkeletonArrayLength];
                    SFrame.CopySkeletonDataTo(skeletons);
                    receivedData = true;

                }
            }

            if (receivedData)
            {

                Skeleton currentSkeleton = (from s in skeletons
                                            where s.TrackingState == SkeletonTrackingState.Tracked
                                            select s).FirstOrDefault();

                if (currentSkeleton != null)
                {

                    //取得骨架關節點 3D(X、Y、Z)座標值。
                    var head = currentSkeleton.Joints[JointType.Head];
                    var righthand = currentSkeleton.Joints[JointType.HandRight];
                    var lefthand = currentSkeleton.Joints[JointType.HandLeft];
                    var leftankle = currentSkeleton.Joints[JointType.AnkleLeft];
                    var rightankle = currentSkeleton.Joints[JointType.AnkleRight];
                    var leftknee = currentSkeleton.Joints[JointType.KneeLeft];
                    var rightknee = currentSkeleton.Joints[JointType.KneeRight];
                    var lefthip = currentSkeleton.Joints[JointType.HipLeft];
                    var righthip = currentSkeleton.Joints[JointType.HipRight];
                    var midhip = currentSkeleton.Joints[JointType.HipCenter];
                    var leftelbow = currentSkeleton.Joints[JointType.ElbowLeft];
                    var rightelbow = currentSkeleton.Joints[JointType.ElbowRight];
                    var leftshoulder = currentSkeleton.Joints[JointType.ShoulderLeft];
                    var rightshoulder = currentSkeleton.Joints[JointType.ShoulderRight];
                    var leftfoot = currentSkeleton.Joints[JointType.FootLeft];
                    var rightfoot = currentSkeleton.Joints[JointType.FootRight];
                    var midshoulder = currentSkeleton.Joints[JointType.ShoulderCenter];
                    var spine = currentSkeleton.Joints[JointType.Spine];





                    /////moveopen
                    if ( do_times_open&&level_open)
                    {
                        
                        if (hn_open)
                        {
                            handtoknee(leftelbow, rightelbow, leftknee, rightknee, head, lefthand, righthand);//手碰膝
                        }
                        if (hc_open)
                        {
                            handcross(leftknee, rightknee, lefthand, righthand, spine, leftshoulder, rightshoulder, head, midhip, rightfoot, leftfoot);//手碰腳
                        }
                        if (squat_open)
                        {
                            squat(head, midhip, leftknee, rightknee, leftfoot, rightfoot, lefthand, righthand, leftelbow, rightelbow);//深蹲
                        }
                      
                    }
                    
                    if (do_how_many_times == 0)
                    {
                        SolidColorBrush color1 = new SolidColorBrush();
                        SolidColorBrush color4 = new SolidColorBrush();
                        color1.Color = Color.FromArgb(255, 255, 0, 0);
                        color4.Color = Color.FromArgb(255, 221, 221, 221);
                     
                        do_times_open = false;
                        a = 0;
                        postrd = false;
                        sensoropen.Fill = color1;
                        
                        
                    }
                    


                }
            }
        }

        ////////////////////////////////////////////movedefination1

        private void handtoknee(Joint leftElbow, Joint rightElbow, Joint leftKnee, Joint rightKnee, Joint head, Joint lefthand, Joint righthand)
        {


            string str1 = Convert.ToString(a);//  成功次數
            successtime.Text = str1;

            SolidColorBrush color1 = new SolidColorBrush();
            SolidColorBrush color2 = new SolidColorBrush();
            
            color1.Color = Color.FromArgb(255,255,0,0);
            color2.Color = Color.FromArgb(255,0,255,255);
           
            if (Math.Abs(lefthand.Position.X - righthand.Position.X) < 0.1 && lefthand.Position.Y - head.Position.Y > 0.2 && righthand.Position.Y - head.Position.Y > 0.2)
            {
                postrd = true;
                sensoropen.Fill = color2;
                hour1 = current_hour;
                minute1 = current_minute;
                second1 = current_second;//reset hour1 minute1 second1
                hour.Content = current_hour;
                minute.Content = current_minute;
                second.Content = current_second;




            }
            if (Math.Abs(lefthand.Position.X - righthand.Position.X) > 0.4 && lefthand.Position.Y - head.Position.Y > 0.2 && righthand.Position.Y - head.Position.Y > 0.2)
            {
                postrd = false;
                sensoropen.Fill = color1;
                



            }
            if (postrd)
            {


                hour.Content = hour1;
                minute.Content = minute1;//重製後 繼續用hour1 minute1 計時
                second.Content = second1;
                current_hour = hour1;
                current_minute = minute1;
                current_second = second1;

            }
   

            if (postrd)
            {

                if (Math.Abs(head.Position.X - leftElbow.Position.X) < 0.1+level_now && leftElbow.Position.Z + 0.1 < head.Position.Z 
                    &&Math.Abs(lefthand.Position.X - head.Position.X) < 0.2 && Math.Abs(lefthand.Position.Y - head.Position.Y) < 0.1
                    &&Math.Abs(lefthand.Position.Z - head.Position.Z) < 0.2)//左肘
                {
                    if (hn_color1rd)
                    {
                        hn_point1.Fill = color2;
                    }

                }
                else
                {
                    if (Math.Abs(leftElbow.Position.X - rightKnee.Position.X) > 0.3)//避免邊緣判定錯誤 預留空間
                        hn_color1rd = true;
                }


                if (Math.Abs(head.Position.X - rightElbow.Position.X) < 0.2 && rightElbow.Position.Z + 0.1 < head.Position.Z 
                    && Math.Abs(righthand.Position.Y - head.Position.Y) < 0.1&& Math.Abs(righthand.Position.X - head.Position.X) < 0.2&&
                    Math.Abs(righthand.Position.Z-head.Position.Z)<0.2)//右肘
                {
                    if (hn_color3rd)
                        hn_point3.Fill = color2;
                }
                else
                {
                    if (Math.Abs(leftElbow.Position.X - rightKnee.Position.X) > 0.3 )
                        hn_color3rd = true;

                }

                if (leftKnee.Position.Y - rightKnee.Position.Y > 0.3-level_now && Math.Abs(leftKnee.Position.X-head.Position.X)<0.2+ level_now)//左膝
                {
                    if (hn_color2rd)
                    {
                        hn_point2.Fill = color2;

                    }
                }
                else
                {
                    if (Math.Abs(leftKnee.Position.Y - rightKnee.Position.Y) < 0.1)
                    {
                        hn_color2rd = true;
                    }
                }
                if (rightKnee.Position.Y - leftKnee.Position.Y > 0.3-level_now && Math.Abs(rightKnee.Position.X - head.Position.X) < 0.2+level_now)//右膝
                {
                    if (hn_color4rd)
                    {
                        hn_point4.Fill = color2;
                    }
                }
                else
                {
                    if (Math.Abs(rightKnee.Position.Y - leftKnee.Position.Y) < 0.1)
                    {
                        hn_color4rd = true;
                    }
                }
                if (Math.Abs(rightElbow.Position.X - leftKnee.Position.X) < 0.3+level_now && Math.Abs(rightElbow.Position.Y - leftKnee.Position.Y) < 0.3+level_now)
                {
                    tra = true;


                }

                if (Math.Abs(leftElbow.Position.X - rightKnee.Position.X) < 0.3 + level_now && Math.Abs(leftElbow.Position.Y - rightKnee.Position.Y) < 0.3 + level_now
                    && lefthand.Position.Y > leftElbow.Position.Y
                    &&Math.Abs(lefthand.Position.X-head.Position.X)<0.2 &&Math.Abs(righthand.Position.X-head.Position.X)<0.2
                    &&Math.Abs(lefthand.Position.Y - head.Position.Y) < 0.2 && Math.Abs(righthand.Position.Y - head.Position.Y) < 0.2
                    &&Math.Abs(righthand.Position.Z - head.Position.Z) < 0.2 && Math.Abs(lefthand.Position.Z - head.Position.Z) < 0.2)
                {

                    if (tra && postrd)
                    {
                        tra = false;
                        a++;

                        hn_point1.Fill = color1;
                        hn_point2.Fill = color1;
                        hn_point3.Fill = color1;
                        hn_point4.Fill = color1;

                        hn_color1rd = false;
                        hn_color2rd = false;
                        hn_color3rd = false;
                        hn_color4rd = false;
                        do_how_many_times = do_how_many_times - 1;
                        if (do_how_many_times == 0)
                        {
                           
                            MessageBox.Show("恭喜恭喜!!");
                            a = 0;
                            MessageBox.Show("請再次要做的選擇次數");
                           

                        }
                       


                    }

                }

            }
          
        }
        ////////////////////////////////////////////movedefination2
        private void handcross(Joint leftknee, Joint rightknee, Joint lefthand, Joint righthand, Joint spine, Joint leftshoulder, Joint rightshoulder, Joint head, Joint midhip, Joint rightfoot, Joint leftfoot)
        {
            string str1 = Convert.ToString(a);
            successtime.Text = str1;
            SolidColorBrush color1 = new SolidColorBrush();
            SolidColorBrush color2 = new SolidColorBrush();
            color1.Color = Color.FromArgb(255, 255, 0, 0);
            color2.Color = Color.FromArgb(255, 0, 255, 255);


            if (Math.Abs(lefthand.Position.X - righthand.Position.X) < 0.1 && lefthand.Position.Y - head.Position.Y > 0.2 && righthand.Position.Y - head.Position.Y > 0.2)
            {
                postrd = true;
                sensoropen.Fill = color2;


                hour1 = current_hour;
                minute1 = current_minute;
                second1 = current_second;//reset hour1 minute1 second1
                hour.Content = current_hour;
                minute.Content = current_minute;
                second.Content = current_second;
               

            }
            if (Math.Abs(lefthand.Position.X - righthand.Position.X) > 0.4 && lefthand.Position.Y - head.Position.Y > 0.2 && righthand.Position.Y - head.Position.Y > 0.2)
            {
                postrd = false;
               sensoropen.Fill = color1;
            }//準備動作

            if (postrd)
            {


                hour.Content = hour1;
                minute.Content = minute1;//重製後 繼續用hour1 minute1 計時
                second.Content = second1;
                current_hour = hour1;
                current_minute = minute1;
                current_second = second1;

            }
            if (postrd)
            {

                if (Math.Abs(lefthand.Position.X - rightfoot.Position.X) < 0.3 +level_now && Math.Abs(lefthand.Position.Y - rightfoot.Position.Y) < 0.3+level_now && Math.Abs(leftfoot.Position.Y - rightfoot.Position.Y) < 0.2)//左手
                {
                    if (hc_color1rd)
                    {
                       hc_point1.Fill = color2;

                    }

                }
                else
                {
                    if (Math.Abs(lefthand.Position.Y - rightfoot.Position.Y) > 0.4)//避免邊緣判定錯誤 預留空間
                        hc_color1rd = true;
                }
               

                if (Math.Abs(righthand.Position.X - leftfoot.Position.X) < 0.3 + level_now && Math.Abs(righthand.Position.Y - leftfoot.Position.Y) < 0.3 + level_now && Math.Abs(leftfoot.Position.Y - rightfoot.Position.Y) < 0.2)//右手
                {
                    
                    if (hc_color2rd)
                    {
                        hc_point2.Fill = color2;
                        tra = true;
                    }

                }
                else
                {
                    if (Math.Abs(righthand.Position.Y - leftfoot.Position.Y) > 0.3)
                        hc_color2rd = true;

                }

                if (Math.Abs(leftshoulder.Position.X - rightfoot.Position.X) < 0.3 + level_now && Math.Abs(leftshoulder.Position.Y - rightfoot.Position.Y) < 0.8 + level_now && Math.Abs(leftfoot.Position.Y - rightfoot.Position.Y) < 0.2)//左肩
                {
                    if (hc_color3rd)
                    {
                        hc_point3.Fill = color2;

                    }
                }
                else
                {
                    if (Math.Abs(leftshoulder.Position.Y - rightfoot.Position.Y) > 0.9)
                    {
                        hc_color3rd = true;
                    }
                }
                if (Math.Abs(rightshoulder.Position.X - leftfoot.Position.X) < 0.3 + level_now && Math.Abs(rightshoulder.Position.Y - leftfoot.Position.Y) < 0.8 + level_now && Math.Abs(leftfoot.Position.Y - rightfoot.Position.Y) < 0.2)//右肩
                {
                    if (hc_color4rd)
                    {
                        hc_point4.Fill = color2;
                        trb = true;
                    }
                }
                else
                {
                    if (Math.Abs(rightshoulder.Position.Y - leftfoot.Position.Y) > 0.9)
                    {
                        hc_color4rd = true;
                    }
                }
                if (Math.Abs(spine.Position.Y - leftfoot.Position.Y) < 0.7 + level_now && Math.Abs(spine.Position.Y - rightfoot.Position.Y) < 0.7 + level_now)//胸
                {
                    if (hc_color5rd)
                    {
                        hc_point5.Fill= color2;

                    }
                }
                else
                {
                    if (Math.Abs(spine.Position.Y - leftfoot.Position.Y) > 0.9 && Math.Abs(spine.Position.Y - rightfoot.Position.Y) > 0.9)
                    {
                        hc_color5rd = true;
                    }
                }

                if (Math.Abs(lefthand.Position.X - rightfoot.Position.X) < 0.3 + level_now && Math.Abs(lefthand.Position.Y - rightfoot.Position.Y) < 0.3 + level_now && Math.Abs(leftfoot.Position.Y - rightfoot.Position.Y) < 0.2)
                {

                    if (tra&&trb)
                    {
                        tra = false;
                        trb = false;
                        a++;
                        hc_point1.Fill = color1;
                        hc_point2.Fill = color1;
                        hc_point3.Fill = color1;
                        hc_point4.Fill = color1;
                        hc_point5.Fill = color1;
                        hc_color1rd = false;
                        hc_color2rd = false;
                        hc_color3rd = false;
                        hc_color4rd = false;
                        hc_color5rd = false;
                        do_how_many_times = do_how_many_times - 1;
                        if (do_how_many_times == 0)
                        {

                            MessageBox.Show("恭喜恭喜!!");

                            a = 0;
                            MessageBox.Show("請再次要做的選擇次數");
                            

                        }

                    }

                }

            }
        }

        ////////////////////////////////////////////movedefination3
        private void squat(Joint head, Joint midhip, Joint leftknee, Joint rightknee, Joint leftfeet, Joint rightfeet,Joint lefthand,Joint righthand,Joint leftelbow, Joint rightelbow)
        {
            string str1 = Convert.ToString(a);
            successtime.Text = str1;
            SolidColorBrush color1 = new SolidColorBrush();
            SolidColorBrush color2 = new SolidColorBrush();
            color1.Color = Color.FromArgb(255, 255, 0, 0);
            color2.Color = Color.FromArgb(255, 0, 255, 255);

            if (Math.Abs(lefthand.Position.X - righthand.Position.X) < 0.1 && lefthand.Position.Y - head.Position.Y > 0.2 && righthand.Position.Y - head.Position.Y > 0.2)
            {
                postrd = true;
                sensoropen.Fill = color2;

                hour1 = current_hour;
                minute1 = current_minute;
                second1 = current_second;//reset hour1 minute1 second1
                hour.Content = current_hour;
                minute.Content = current_minute;
                second.Content = current_second;
            }
            if (Math.Abs(lefthand.Position.X - righthand.Position.X) > 0.4 && lefthand.Position.Y - head.Position.Y > 0.2 && righthand.Position.Y - head.Position.Y > 0.2)
            {
                postrd = false;
                sensoropen.Fill = color1;
            }//準備動作


            if (postrd)
            {

                hour.Content = hour1;
                minute.Content = minute1;//重製後 繼續用hour1 minute1 計時
                second.Content = second1;
                current_hour = hour1;
                current_minute = minute1;
                current_second = second1;

            }




            if (postrd)
            {
                if  (Math.Abs(lefthand.Position.X - righthand.Position.X) < 0.2 && Math.Abs(lefthand.Position.Y - righthand.Position.Y) < 0.2//左手&&右手//狀態 不用開關
                     && lefthand.Position.Y - leftelbow.Position.Y > 0.1 && righthand.Position.Y - rightelbow.Position.Y > 0.1
                     &&Math.Abs(lefthand.Position.X-midhip.Position.X)<0.2
                     && Math.Abs(righthand.Position.X - midhip.Position.X) < 0.2)
                {
                    sq_point1.Fill = color2;
                    sq_point2.Fill = color2;
                    tra = true;//手維持狀態
                }
                else
                { 
                    if(Math.Abs(lefthand.Position.X - righthand.Position.X) > 0.3)
                    {
                        sq_point1.Fill = color1;
                        sq_point2.Fill = color1;
                        tra = false;
                    }    
                }

                if (Math.Abs(leftfeet.Position.X-rightfeet.Position.X)>0.4 && Math.Abs(leftfeet.Position.Y - rightfeet.Position.Y)<0.1)//左右腳
                {
                    sq_point3.Fill = color2;
                    sq_point4.Fill = color2;
                    trb = true;

                }
                else
                {
                    if(Math.Abs(leftfeet.Position.X - rightfeet.Position.X) < 0.3)
                    {
                        sq_point3.Fill = color1;
                        sq_point4.Fill = color1;
                        trb = false;
                    }
                        
                }
                if (midhip.Position.Y - leftfeet.Position.Y < 0.5 + level_now && midhip.Position.Y - rightfeet.Position.Y < 0.5 + level_now)//髖關節
                {
                    
                   
                    trc = true;

                }
                else
                {
                    if(midhip.Position.Y - leftfeet.Position.Y >0.6 && midhip.Position.Y - rightfeet.Position.Y > 0.6)
                    {
                       
                        trc = false;
                        sq_color1rd = true;
                    }


                }

                if (tra && trb&& trc&&sq_color1rd)//成功次數
                {

                    a++;
                    trc = false;
                    sq_color1rd=false;
                    do_how_many_times = do_how_many_times - 1;
                    if (do_how_many_times == 0)
                    {
                        MessageBox.Show("恭喜恭喜!!");
                        a = 0;
                        MessageBox.Show("請再次要做的選擇次數");
                        

                    }

                }
            }


        }
        
       




        


        KinectSensor sensor1 = (from sensorToCheck in KinectSensor.KinectSensors    //角度用
                                where sensorToCheck.Status == KinectStatus.Connected
                                select sensorToCheck).FirstOrDefault();

     

        private void timer1_Tick(object sender, EventArgs e)//範例影片用計時器
        {
            TimeSpan timehc = new TimeSpan(0, 0, 0, 2, 0);
            TimeSpan timehn= new TimeSpan(0, 0, 0, 1, 0);
            TimeSpan timesq= new TimeSpan(0, 0, 0, 2, 0);
            midiahn.Position = timehn;
            midiahc.Position = timehc;
            midiasq.Position = timesq;

        }
        private void timer2_Tick(object sender, EventArgs e)//計算時間用的計時器 
        {
            second1 = second1 + 1;
            if (second1 == 60)
            {
                minute1 = minute1 + 1;
                if (minute1 == 60)
                {
                    hour1 = hour1 + 1;
                    minute1 = 0;
                }
                second1 = 0;
            }
            
           
        }





        private void changetosq(object sender, RoutedEventArgs e)
        {

            SolidColorBrush color1 = new SolidColorBrush();
            SolidColorBrush color2 = new SolidColorBrush();
            SolidColorBrush color3 = new SolidColorBrush();
            SolidColorBrush color4 = new SolidColorBrush();

            color1.Color = Color.FromArgb(255, 255, 0, 0);
            color2.Color = Color.FromArgb(0, 0, 0, 5);
            color3.Color = Color.FromArgb(255, 106, 210, 214);
            color4.Color = Color.FromArgb(255, 221, 221, 221);

            squat_open = true;
            hc_open = false;
            hn_open = false;
            postrd = false;
            level_open = false;
            do_times_open = false;
            sensoropen.Fill = color1;
            hn_point1.Height = 0;
            hn_point1.Width = 0;
            hn_point2.Height = 0;
            hn_point2.Width = 0;
            hn_point3.Height = 0;
            hn_point3.Width = 0;
            hn_point4.Height = 0;
            hn_point4.Width = 0;
            ////////////////
            hc_point1.Height = 0;
            hc_point1.Width = 0;
            hc_point2.Height = 0;
            hc_point2.Width = 0;
            hc_point3.Height = 0;
            hc_point3.Width = 0;
            hc_point4.Height = 0;
            hc_point4.Width = 0;
            hc_point5.Height = 0;
            hc_point5.Width = 0;

            sq_point1.Height = 22;
            sq_point1.Width = 22;
            sq_point2.Height = 22;
            sq_point2.Width = 22;
            sq_point3.Height = 22;
            sq_point3.Width = 22;
            sq_point4.Height = 22;
            sq_point4.Width = 22;
            


            midiahn.Height = 0;
            midiahn.Width = 0;
            midiahc.Height = 0;
            midiahc.Width = 0;
            midiasq.Height = 341;
            midiasq.Width = 504;
            midiasq.Play();
           
            botton2.Background = color4;
            botton1.Background = color4;
            botton3.Background = color3;
          
       
            botton7.Background = color4;
            botton8.Background = color4;
            botton9.Background = color4;
            MessageBox.Show("請選擇難易度");
            a = 0;


        }
        private void changetohc(object sender, RoutedEventArgs e)//動作交換
        {
            SolidColorBrush color1 = new SolidColorBrush();
            SolidColorBrush color2 = new SolidColorBrush();
            SolidColorBrush color3 = new SolidColorBrush();
            SolidColorBrush color4 = new SolidColorBrush();

            color1.Color = Color.FromArgb(255, 255, 0, 0);
            color2.Color = Color.FromArgb(0, 0, 0, 5);
            color3.Color = Color.FromArgb(255,106, 210, 214);
            color4.Color = Color.FromArgb(255, 221, 221, 221);

            hc_open = true;
            hn_open = false;
            squat_open = false;
            postrd = false;
            level_open = false;
            do_times_open = false;
            sensoropen.Fill = color1;
            hn_point1.Height = 0;
            hn_point1.Width = 0;
            hn_point2.Height = 0;
            hn_point2.Width = 0;
            hn_point3.Height = 0;
            hn_point3.Width = 0;
            hn_point4.Height = 0;
            hn_point4.Width = 0;
            ////////////////
            hc_point1.Height = 22;
            hc_point1.Width = 22;
            hc_point2.Height = 22;
            hc_point2.Width = 22;
            hc_point3.Height = 22;
            hc_point3.Width = 22;
            hc_point4.Height = 22;
            hc_point4.Width = 22;
            hc_point5.Height = 22;
            hc_point5.Width = 22;

            sq_point1.Height = 0;
            sq_point1.Width = 0;
            sq_point2.Height = 0;
            sq_point2.Width = 0;
            sq_point3.Height = 0;
            sq_point3.Width = 0;
            sq_point4.Height = 0;
            sq_point4.Width = 0;
  


            midiahn.Height = 0;
            midiahn.Width = 0;
            midiahc.Height = 341;
            midiahc.Width = 504;
            midiasq.Height = 0;
            midiasq.Width = 0;
            midiahc.Play();
            successtime.Text = "0";
            botton2.Background = color3;
            botton1.Background = color4;
            botton3.Background = color4;
           
           
            botton7.Background = color4;
            botton8.Background = color4;
            botton9.Background = color4;
            MessageBox.Show("請選擇難易度");
            a = 0;





        }

        private void changetohn(object sender, RoutedEventArgs e) //動作交換
        {
            SolidColorBrush color1 = new SolidColorBrush();
            SolidColorBrush color2 = new SolidColorBrush();
            SolidColorBrush color3 = new SolidColorBrush();
            SolidColorBrush color4 = new SolidColorBrush();

            color1.Color = Color.FromArgb(255, 255, 0, 0);
            color2.Color = Color.FromArgb(0, 0, 0, 5);
            color3.Color = Color.FromArgb(255, 106, 210, 214);
            color4.Color = Color.FromArgb(255, 221, 221, 221);

            hn_open = true;
            hc_open = false;
            squat_open = false;
            postrd = false;
            level_open = false;
            do_times_open = false;
            sensoropen.Fill = color1;
            hc_point1.Height = 0;
            hc_point1.Width = 0;
            hc_point2.Height = 0;
            hc_point2.Width = 0;
            hc_point3.Height = 0;
            hc_point3.Width = 0;
            hc_point4.Height = 0;
            hc_point4.Width = 0;
            hc_point5.Height = 0;
            hc_point5.Width = 0;
            ///////////////////////
            hn_point1.Height = 22;
            hn_point1.Width = 22;
            hn_point2.Height = 22;
            hn_point2.Width = 22;
            hn_point3.Height = 22;
            hn_point3.Width = 22;
            hn_point4.Height = 22;
            hn_point4.Width = 22;
            ////////////////
            sq_point1.Height = 0;
            sq_point1.Width = 0;
            sq_point2.Height = 0;
            sq_point2.Width = 0;
            sq_point3.Height = 0;
            sq_point3.Width = 0;
            sq_point4.Height = 0;
            sq_point4.Width = 0;
           

            midiasq.Height = 0;
            midiasq.Width = 0;
            midiahc.Height = 0;
            midiahc.Width = 0;
            midiahn.Height = 341;
            midiahn.Width = 504;
            midiahn.Play();
            successtime.Text = "0";

            botton1.Background = color3;
            botton2.Background = color4;
            botton3.Background = color4;
          
          ;
            botton7.Background = color4;
            botton8.Background = color4;
            botton9.Background = color4;
            MessageBox.Show("請選擇難易度");
            a = 0;





        }



        private void ANGLE_DOWN_click(Object sender, RoutedEventArgs e)//kinect 角度
        {
            if (sensor1.ElevationAngle - 3 < sensor1.MinElevationAngle)
                sensor1.ElevationAngle = sensor1.MinElevationAngle;
            else
                sensor1.ElevationAngle = sensor1.ElevationAngle - 3;
            Thread.Sleep(1000);
        }
        private void ANGLE_UP_click(Object sender, RoutedEventArgs e)
        {
            if (sensor1.ElevationAngle + 3 > sensor1.MaxElevationAngle)
                sensor1.ElevationAngle = sensor1.MaxElevationAngle;
            else
                sensor1.ElevationAngle = sensor1.ElevationAngle + 3;
            Thread.Sleep(1000);
        }

        



        private void change_to_level1(object sender, RoutedEventArgs e)//手碰膝35
        {
            level_open = true;
            level_now = 0.2;
            successtime.Text = ("0");
            a = 0;
            SolidColorBrush color1 = new SolidColorBrush();
            SolidColorBrush color3 = new SolidColorBrush();
            SolidColorBrush color4 = new SolidColorBrush();
            color3.Color = Color.FromArgb(255, 106, 210, 214);
            color4.Color = Color.FromArgb(255, 221, 221, 221);
            color1.Color = Color.FromArgb(255, 255, 0, 0);
            botton7.Background = color3;
            botton8.Background = color4;
            botton9.Background = color4;
        
            MessageBox.Show("請輸入次數(1~500");
        }

        private void change_to_level2(object sender, RoutedEventArgs e)//手碰膝25
        {
            level_open = true;
            level_now = 0.1;
            successtime.Text = ("0");
            a = 0;
            SolidColorBrush color1 = new SolidColorBrush();
            SolidColorBrush color3 = new SolidColorBrush();
            SolidColorBrush color4 = new SolidColorBrush();
            color3.Color = Color.FromArgb(255, 106, 210, 214);
            color4.Color = Color.FromArgb(255, 221, 221, 221);
            color1.Color = Color.FromArgb(255, 255, 0, 0);
            botton8.Background = color3;
            botton9.Background = color4;
            botton7.Background = color4;
           
            MessageBox.Show("請輸入次數(1~500");
        }

        private void change_to_level3(object sender, RoutedEventArgs e)//手碰膝15
        {
            level_open = true;
            level_now = 0;
            successtime.Text = ("0");
            a = 0;
            SolidColorBrush color1 = new SolidColorBrush();
            SolidColorBrush color3 = new SolidColorBrush();
            SolidColorBrush color4 = new SolidColorBrush();
            color3.Color = Color.FromArgb(255, 106, 210, 214);
            color4.Color = Color.FromArgb(255, 221, 221, 221);
            color1.Color = Color.FromArgb(255, 255, 0, 0);
            botton9.Background = color3;
            botton8.Background = color4;
            botton7.Background = color4;
            
            MessageBox.Show("請輸入次數(1~500");
        }

        

        private void do_how_many_time(object sender, RoutedEventArgs e)
        {
            string str1 = how_many_times.Text;
            int check = int.Parse(str1);
            if(check<501&&check > 0)
               {
                do_how_many_times= check;
                successtime.Text = ("0");
                do_times_open = true;
                MessageBox.Show("準備後即可開始動作");
                hour.Content = 0;
                minute.Content = 0;
                second.Content = 0;
                hour1 = 0;
                minute1 = 0;
                second1 = 0;
                current_second = 0;
                current_minute = 0;
                current_hour = 0;
                    
                
               


            }
            else
            {
                MessageBox.Show("請輸入1~500內的數字");
            }
            
        }







        //private void change_to_fifteen_times(object sender, RoutedEventArgs e)
        //{
        //    successtime.Text = ("0");
        //    a = 0;
        //    do_how_many_times = 5;
        //    do_times_open = true;
        //    SolidColorBrush color1 = new SolidColorBrush();
        //    SolidColorBrush color3 = new SolidColorBrush();
        //    SolidColorBrush color4 = new SolidColorBrush();
        //    color3.Color = Color.FromArgb(255, 106, 210, 214);
        //    color4.Color = Color.FromArgb(255, 221, 221, 221);
        //    color1.Color = Color.FromArgb(255, 255, 0, 0);
        //    botton5.Background = color3;
        //    botton4.Background = color4;
        //    botton6.Background = color4;
        //    MessageBox.Show("準備後即可開始動作");
        //}//次數交換

        //private void CheckGesture()//防誤判
        //{
        //    if (isLeftHelloGestureActive)
        //    {
        //        CheckGesture_ready = true;
        //    }
        //    else
        //    {
        //        CheckGesture_ready = false;
        //    }
        //}
        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    chooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
        //}

        //void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{

        //    KinectSensor sensor = (KinectSensor)e.NewValue;

        //    sensor.SkeletonStream.Enable();

        //    sensor.SkeletonStream.Enable();

        //    sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
        //}

    }
}