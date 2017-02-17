using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenCvSharp.CPlusPlus;

namespace OpenCvSharp.Tests.Contrib
{
    [TestFixture]
    public class FaceRecognizerTest : TestBase
    {
        [Test]
        public void CreateAndDisposeLBPH()
        {
            var recognizer = FaceRecognizer.CreateLBPHFaceRecognizer();
            recognizer.Dispose();
        }

        [Test]
        public void CreateAndDisposeEigen()
        {
            var recognizer = FaceRecognizer.CreateEigenFaceRecognizer();
            recognizer.Dispose();
        }

        [Test]
        public void CreateAndDisposeFisher()
        {
            var recognizer = FaceRecognizer.CreateFisherFaceRecognizer();
            recognizer.Dispose();
        }

        [Test, Ignore("not implemented")]
        public void TrainAndPredict()
        {
            var image = Image("Lenna.png");

            var model = FaceRecognizer.CreateLBPHFaceRecognizer();

            var cascade = new CascadeClassifier("../haarcascade_frontalface_default.xml");
            var rects = cascade.DetectMultiScale(image);

            foreach (Rect rect in rects)
            {
                using (Mat face = image[rect].Clone())
                {
                    Cv2.Resize(face, face, new Size(256, 256));

                    int label;
                    double confidence;
                    model.Predict(face, out label, out confidence);
                    Console.WriteLine($"{label} ({confidence})");
                }
            }
        }
    }
}

