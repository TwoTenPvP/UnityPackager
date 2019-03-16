using System;
using System.Collections.Generic;
using System.IO;
using UnityPackager;
using Xunit;

namespace Tests
{
    public class PackerTest
    {
       
        [Fact]
        public void PackTest()
        {
            Dictionary<string, string> fileMap = new Dictionary<string, string>
            {
                ["sample/sample1.txt"] = "sample1.txt",
                ["sample/childfolder/sample2.txt"] = "sample2.txt"
            };

            Packer.Pack(fileMap, "sample.unitypackage");

            Assert.True(File.Exists("sample.unitypackage"), "Package should have been created");
            // now let's unpack and check it out
            string fullpath = Path.GetFullPath("sample.unitypackage");
            Console.WriteLine($"Full path is {fullpath}");

            Unpacker.Unpack("sample.unitypackage", "sample_out");

            Assert.True(File.Exists("sample_out/sample1.txt"), "sample1.txt should have been decompressed");
            Assert.True(File.Exists("sample_out/sample2.txt"), "sample2.txt should have been decompressed");
        }
    }
}
