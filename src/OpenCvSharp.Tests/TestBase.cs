using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenCvSharp.CPlusPlus;

namespace OpenCvSharp.Tests
{
    public abstract class TestBase 
    {
        protected void SetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.WorkDirectory);
        }

        protected Mat Image(string fileName, LoadMode modes = LoadMode.Color)
        {
            return new Mat(Path.Combine("_data", "image", fileName), modes);
        }
    }
}
