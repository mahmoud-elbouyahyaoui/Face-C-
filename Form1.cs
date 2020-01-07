using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceDetector
{
    public partial class Form1 : Form
    {
        // Variables 
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX,0.6d,0.6d);
        HaarCascade faceDetected;
        Image<Bgr, Byte> Frame;
        Capture camera;
        Image<Gray, Byte> result;
        Image<Gray, Byte> TrainedFace = null;
        Image<Gray, Byte> grayFace = null;
        List<Image<Gray, Byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> Users = new List<string>();
        int Count, NumLables, t;
        string name, names = null;

        private void saveButton_Click(object sender, EventArgs e)
        {
            Count = Count + 1;
            grayFace = camera.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            MCvAvgComp[][] DetectedFaces = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,new Size(20,20));
            foreach (MCvAvgComp f in DetectedFaces[0])
            {
                TrainedFace = Frame.Copy(f.rect).Convert<Gray, Byte>();
                break;
            }
            TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            trainingImages.Add(TrainedFace);
            labels.Add(textName.Text);
            File.WriteAllText(Application.StartupPath + "/Faces/Faces.txt",trainingImages.ToArray().Length.ToString()+",");
            for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
            {
                trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Faces/Faces" + i + ".bmp");
                File.AppendAllText(Application.StartupPath + "/Faces/Faces.txt", trainingImages.ToArray()[i-1] + ",");
            }
            MessageBox.Show(textName.Text + " ADDED wooowww");

        }

        public Form1()
        {
            InitializeComponent();
            //HaaeCascade for face detection
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                string Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
                string[] Labels = Labelsinf.Split(',');
                NumLables = Convert.ToInt16(Labels[0]);
                Count = NumLables;
                string FacesLoad;
                for(int i=1;i<NumLables+1;i++)
                {
                    FacesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/Faces.txt"));
                    labels.Add(Labels[i]);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("faces . txt  "+ex.ToString());
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            camera = new Capture();
            camera.QueryFrame();
            Application.Idle += new EventHandler(FrameProcedure);
        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            Users.Add("");
            Frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,new Size(20,20));
            foreach(MCvAvgComp f in facesDetectedNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Green), 3);
                if(trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termcriteria = new MCvTermCriteria(Count, 0.001);
                    EigenObjectRecognizer reconizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 1500, ref termcriteria);
                    name = reconizer.Recognize(result);
                    Frame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));
                


                }
                //Users[t - 1] = name;
                Users.Add("");

            }

            cameraBox.Image = Frame;
            name = "";
            Users.Clear();

        }
    }
}
