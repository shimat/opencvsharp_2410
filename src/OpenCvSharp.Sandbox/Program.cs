using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.CPlusPlus.Gpu;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.CPlusPlus.Point;
using Rect = OpenCvSharp.CPlusPlus.Rect;
using Size = OpenCvSharp.CPlusPlus.Size;
using Window = OpenCvSharp.CPlusPlus.Window;

namespace OpenCvSharp.Sandbox
{
    /// <summary>
    /// Simple codes for debugging
    /// </summary>
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            //VideoCaptureSample();
            //OpenCVError();
            /*
            double ret = Cv2.PointPolygonTest(new[] { new Point2f(0, 0), new Point2f(0, 10), new Point2f(10, 10), new Point2f(10, 0) }, new Point2f(5, 5), false);
            ret.ToString();

            BForceMatcherSample();*/
            //ChamferMatchingSample();

            /*
            var img1 = new IplImage("data/lenna.png", LoadMode.Color);
            var img2 = new IplImage("data/match2.png", LoadMode.Color);
            Surf(img1, img2);
            //*/

            /*
            var img = new IplImage("data/lenna.png", LoadMode.Color);
            var points = new[]
            {
                new CvPoint(100, 100),
                new CvPoint(200, 100),
                new CvPoint(200, 200),
            };
            PolygonTest(img, points);
            //*/

            FileStorageTest();
            //ConvertToBitmapSourceTest();
            //CvLoadTest();
            //CaptureTest();

            //Mat[] mats = StitchingPreprocess(400, 400, 10);
            //Stitching(mats);
            //Track();
            //Run();
        }

        private static void FileStorageTest()
        {
            const string fileName = "foo.yml";
            using (var fs = new FileStorage(fileName, FileStorageMode.Write | FileStorageMode.FormatYaml))
            {
                fs.Write("int", 123);
                fs.Write("double", Math.PI);
                using (var tempMat = new Mat("data/lenna.png"))
                {
                    fs.Write("mat", tempMat);
                }
            }

            using (var fs = new FileStorage(fileName, FileStorageMode.Read))
            {
                Console.WriteLine("int: {0}", fs["int"].ReadInt());
                Console.WriteLine("double: {0}", (double)fs["double"]);
                using (var window = new Window("mat"))
                {
                    window.ShowImage(fs["mat"].ReadMat());
                    Cv2.WaitKey();
                }
            }
        }

        private static void ConvertToBitmapSourceTest()
        {
            BitmapSource bs;

            // OpenCVによる画像処理 (Threshold)
            using (var src = new IplImage("data/lenna.png", LoadMode.GrayScale))
            using (var dst = new IplImage(src.Size, BitDepth.U8, 1))
            {
                src.Smooth(src, SmoothType.Gaussian, 5);
                src.Threshold(dst, 0, 255, ThresholdType.Otsu);
                bs = BitmapSourceConverter.ToBitmapSource(dst.ToBitmap());
            }

            // WPFのWindowに表示してみる
            var image = new System.Windows.Controls.Image { Source = bs };
            var window = new System.Windows.Window
            {
                Title = "from IplImage to BitmapSource",
                Width = bs.PixelWidth,
                Height = bs.PixelHeight,
                Content = image
            };

            var app = new Application();
            app.Run(window);
        }

        private static void CvLoadTest()
        {
            for (var i = 0; i < 1000000; i++)
            {
                //A potential b.ug in OpenCVSharp marks CvHaarClassifierCascade.IsEnabledDispose = false, preventing the IDispoable pattern from 
                //cleaning up the memory allocated when loading the face detection features
                using (var res = Cv.Load<CvHaarClassifierCascade>("data/haarcascade_frontalface_default.xml"))
                {
                    if (i%100 == 0)
                        Console.WriteLine("{0}MB", MyProcess.WorkingSet64 / 1024.0 / 1024.0);

                    res.ToString();
                    //Calling this method cleans up the region of allocated memory correctly. 
                    //Comment out these two lines to reproduce the memory leak.
                    //var ptr = res.CvPtr;
                    //NativeMethods.cvReleaseHaarClassifierCascade(ref ptr);
                }
            }
        }

        private static void CaptureTest()
        {
            var capture = new VideoCapture(@"C:\a.wmv");

            if (!capture.IsOpened()) throw new Exception("Video can`t be played");

            int sleepTime = (int) Math.Round(1000/capture.Fps);
            using (var window = new Window("capture"))
            {
                var trackbar = window.CreateTrackbar2("Position", 0, capture.FrameCount,
                    (int pos, object userdata) =>
                    {
                        capture.PosFrames = pos;
                    }, null);
                // Frame image buffer
                Mat image = new Mat();

                // When the movie playback reaches end, Mat.data becomes NULL.
                while (capture.Read(image))
                {
                    trackbar.Pos = capture.PosFrames;
                    window.ShowImage(image);
                    Cv2.WaitKey(sleepTime);
                }
            }
        }

        private static void PolygonTest(IplImage img, IEnumerable<CvPoint> points)
        {
            Cv.PolyLine(img, new [] { points.ToArray(), points.Select(p => new CvPoint(p.X + 100, p.Y + 100)).ToArray(), }, true, CvColor.Red, 1);

            using (new CvWindow(img))
            {
                Cv.WaitKey();
            }
        }

        private static void VideoCaptureSample()
        {
            var cap = new VideoCapture(0);

            if (!cap.IsOpened())
            {
                Console.WriteLine("Can't use camera.");
                return;
            }

            var frame = new Mat();
            cap.Read(frame);
            Window.ShowImages(frame);
            frame.ToString();
        }

        private static void OpenCVError()
        {
            var mat = new Mat(5, 1, MatType.CV_32FC1);
            var mat2 = new MatOfFloat(mat);
            var indexer = mat2.GetIndexer();
            indexer[0, 0] = 2f;
            indexer[1, 0] = 1f;
            indexer[2, 0] = 3f;
            indexer[3, 0] = 4f;
            indexer[4, 0] = 5f;
            double maxValue, minValue;
            var test = 10;
            int maxIdx, minIdx;
            Console.WriteLine(test);
            Cv2.MinMaxIdx(mat, out minValue, out maxValue, out minIdx, out maxIdx);
            Console.WriteLine(test);
            Console.Read();
        }

        private static void BForceMatcherSample()
        {
            var src1 = new Mat("data/match1.png");
            var src2 = new Mat("data/match2.png");

            var gray1 = new Mat();
            var gray2 = new Mat();
            Cv2.CvtColor(src1, gray1, ColorConversion.BgrToGray);
            Cv2.CvtColor(src2, gray2, ColorConversion.BgrToGray);

            var fast = new FastFeatureDetector(10);
            var descriptorExtractor = new BriefDescriptorExtractor(32);

            var descriptors1 = new Mat();
            var descriptors2 = new Mat();

            KeyPoint[] keypoints1 = fast.Run(gray1, null);
            descriptorExtractor.Compute(gray1, ref keypoints1, descriptors1);

            KeyPoint[] keypoints2 = fast.Run(gray2, null);
            descriptorExtractor.Compute(gray2, ref keypoints2, descriptors2);

            // Match descriptor vectors
            var bfMatcher = new BFMatcher(NormType.L2, false);
            
            DMatch[][] bfMatches = bfMatcher.KnnMatch(descriptors1, descriptors2, 3, null, false);
            bfMatches.ToString();

            var view = new Mat();
            Cv2.DrawMatches(src1, keypoints1, src2, keypoints2, bfMatches, view);
            Window.ShowImages(view);
        }

        private static void ChamferMatchingSample()
        {
            using (var img = new Mat("data/lenna.png", LoadMode.GrayScale))
            using (var templ = new Mat("data/lennas_eye.png", LoadMode.GrayScale))
            {
                Point[][] points;
                float[] cost;
                
                using (var imgEdge = img.Canny(50, 200))
                using (var templEdge = templ.Canny(50, 200))
                {
                    imgEdge.SaveImage("e1.png");
                    templEdge.SaveImage("e2.png");

                    var ret = Cv2.ChamferMatching(imgEdge, templEdge, out points, out cost);

                    int i = 0;

                    Console.WriteLine(ret);
                    Console.WriteLine(points.Count());

                    using (var img3 = img.CvtColor(ColorConversion.GrayToRgb))
                    {
                        foreach (var point in points)
                        {
                            foreach (var point1 in point)
                            {
                                Vec3b c = new Vec3b(0, 255, 0);
                                img3.Set<Vec3b>(point1.Y, point1.X, c);
                            }

                            Console.WriteLine(cost[i]);
                            i++;
                        }
                        foreach (var point1 in points[0])
                        {
                            Vec3b c = new Vec3b(255, 0, 255);
                            img3.Set<Vec3b>(point1.Y, point1.X, c);
                        }

                        Window.ShowImages(img3);
                        img3.SaveImage("final.png");
                    }
                }
            }
        }

        private static void Clahe()
        {
            Mat src = new Mat("data/tsukuba_left.png", LoadMode.GrayScale);
            Mat dst20 = new Mat();
            Mat dst40 = new Mat();
            Mat dst44 = new Mat();

            using (CLAHE clahe = Cv2.CreateCLAHE())
            {
                clahe.ClipLimit = 20;
                clahe.Apply(src, dst20);
                clahe.ClipLimit = 40;
                clahe.Apply(src, dst40);
                clahe.TilesGridSize = new Size(4, 4);
                clahe.Apply(src, dst44);
            }

            Window.ShowImages(src, dst20, dst40, dst44);
        }

        private static void Surf(IplImage img1, IplImage img2)
        {
            Mat src = new Mat(img1, true);
            Mat src2 = new Mat(img2, true);
            //Detect the keypoints and generate their descriptors using SURF
            SURF surf = new SURF(500, 4, 2, true);
            KeyPoint[] keypoints1, keypoints2;
            MatOfFloat descriptors1 = new MatOfFloat();
            MatOfFloat descriptors2 = new MatOfFloat();
            surf.Run(src, null, out keypoints1, descriptors1);
            surf.Run(src2, null, out keypoints2, descriptors2);
            // Matching descriptor vectors with a brute force matcher
            BFMatcher matcher = new BFMatcher(NormType.L2, false);
            DMatch[] matches = matcher.Match(descriptors1, descriptors2);//例外が発生する箇所
            Mat view = new Mat();
            Cv2.DrawMatches(src, keypoints1, src2, keypoints2, matches, view);

            Window.ShowImages(view);
        }

        private static Mat[] StitchingPreprocess(int width, int height, int count)
        {
            Mat source = new Mat(@"C:\Penguins.jpg", LoadMode.Color);
            Mat result = source.Clone();

            var rand = new Random();
            var mats = new List<Mat>();
            for (int i = 0; i < count; i++)
            {
                int x1 = rand.Next(source.Cols - width);
                int y1 = rand.Next(source.Rows - height);
                int x2 = x1 + width;
                int y2 = y1 + height;

                result.Line(new Point(x1, y1), new Point(x1, y2), new Scalar(0, 0, 255));
                result.Line(new Point(x1, y2), new Point(x2, y2), new Scalar(0, 0, 255));
                result.Line(new Point(x2, y2), new Point(x2, y1), new Scalar(0, 0, 255));
                result.Line(new Point(x2, y1), new Point(x1, y1), new Scalar(0, 0, 255));

                Mat m = source[new Rect(x1, y1, width, height)];
                mats.Add(m.Clone());
                //string outFile = String.Format(@"C:\temp\stitching\{0:D3}.png", i);
                //m.SaveImage(outFile);
            }

            result.SaveImage(@"C:\temp\parts.png");
            using (new Window(result))
            {
                Cv.WaitKey();
            }

            return mats.ToArray();
        }

        private static void Stitching(Mat[] images)
        {
            var stitcher = Stitcher.CreateDefault(false);

            Mat pano = new Mat();

            Console.Write("Stitching 処理開始...");
            var status = stitcher.Stitch(images, pano);
            Console.WriteLine(" 完了 {0}", status);

            pano.SaveImage(@"C:\temp\pano.png");
            Window.ShowImages(pano);

            foreach (Mat image in images)
            {
                image.Dispose();
            }
        }

        private static void Track()
        {
            using (var video = new CvCapture("data/bach.mp4"))
            {
                IplImage frame = null;
                IplImage gray = null;
                IplImage binary = null;
                IplImage render = null;
                IplImage renderTracks = null;
                CvTracks tracks = new CvTracks();
                CvWindow window = new CvWindow("render");
                CvWindow windowTracks = new CvWindow("tracks");

                for (int i = 0; ; i++)
                {
                    frame = video.QueryFrame();
                    //if (frame == null)
                    //    frame = new IplImage("data/shapes.png");
                    if (gray == null)
                    {
                        gray = new IplImage(frame.Size, BitDepth.U8, 1);
                        binary = new IplImage(frame.Size, BitDepth.U8, 1);
                        render = new IplImage(frame.Size, BitDepth.U8, 3);
                        renderTracks = new IplImage(frame.Size, BitDepth.U8, 3);
                    }

                    render.Zero();
                    renderTracks.Zero();

                    Cv.CvtColor(frame, gray, ColorConversion.BgrToGray);
                    Cv.Threshold(gray, binary, 0, 255, ThresholdType.Otsu);

                    CvBlobs blobs = new CvBlobs(binary);
                    CvBlobs newBlobs = new CvBlobs(blobs
                        .OrderByDescending(pair => pair.Value.Area)
                        .Take(200)
                        .ToDictionary(pair => pair.Key, pair => pair.Value), blobs.Labels);
                    newBlobs.RenderBlobs(binary, render);
                    window.ShowImage(render);

                    newBlobs.UpdateTracks(tracks, 10.0, Int32.MaxValue);
                    tracks.Render(binary, renderTracks);
                    windowTracks.ShowImage(renderTracks);

                    Cv.WaitKey(200);
                    Console.WriteLine(i);
                }
            }
        }

        static string AspectRatioAsString(float f)
        {
            bool carryon = true;
            int index = 0;
            double roundedUpValue = 0;
            while (carryon)
            {
                index++;
                float upper = index * f;

                roundedUpValue = Math.Ceiling(upper);

                if (roundedUpValue - upper <= (double)0.1 || index > 20)
                {
                    carryon = false;
                }
            }

            return roundedUpValue + ":" + index;
        }

        private static void Run()
        {
            var dm = DescriptorMatcher.Create("BruteForce");
            dm.Clear();

            string[] algoNames = Algorithm.GetList();
            Console.WriteLine(String.Join("\n", algoNames));

            SIFT al1 = new SIFT();
            string[] ppp = al1.GetParams();
            Console.WriteLine(ppp);
            var t = al1.ParamType("contrastThreshold");
            double d = al1.GetDouble("contrastThreshold");
            t.ToString();
            d.ToString();

            var src = new Mat("data/lenna.png");
            var rand = new Random();
            var memory = new List<long>(100);

            var a1 = new Mat(src, Rect.FromLTRB(0, 0, 30, 40));
            var a2 = new Mat(src, Rect.FromLTRB(0, 0, 30, 40));
            var a3 = new Mat(src, Rect.FromLTRB(0, 0, 30, 40));
            a3.ToString();

            for (long i = 0; ; i++)
            {
                for (int j = 0; j < 200; j++)
                {
                    int c1 = rand.Next(100, 400);
                    int c2 = rand.Next(100, 400);
                    Mat temp = src.Row[c1];
                    src.Row[c1] = src.Row[c2];
                    src.Row[c2] = temp;
                }

                memory.Add(MyProcess.WorkingSet64);
                if (memory.Count >= 100)
                {
                    double average = memory.Average();
                    Console.WriteLine("{0:F3}MB", average / 1024.0 / 1024.0);
                    memory.Clear();
                    GC.Collect();
                }
            }
        }
    }
}
