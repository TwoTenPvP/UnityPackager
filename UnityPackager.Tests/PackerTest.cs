using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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
                ["sample/childfolder/sample2.txt"] = "sample2.txt",
                ["sample/box.png"] = "images/box.png"
            };

            Packer.Pack(fileMap, "sample.unitypackage");

            Assert.True(File.Exists("sample.unitypackage"), "Package should have been created");

            Unpacker.Unpack("sample.unitypackage", "sample_out");

            Assert.True(File.Exists("sample_out/sample1.txt"), "sample1.txt should have been decompressed");
            Assert.True(File.Exists("sample_out/sample2.txt"), "sample2.txt should have been decompressed");
            Assert.True(File.Exists("sample_out/sample1.meta"), "sample1.meta should have been generated");
            Assert.True(File.Exists("sample_out/sample2.meta"), "sample2.meta should have been generated");
            Assert.True(File.Exists("sample_out/images/box.png"), "box.png should have been decompressed");
            Assert.True(File.Exists("sample_out/images/box.meta"), "box.meta should have been decompressed");

            // let's make sure the file was not modified
            byte[] md5 = GetMD5("sample_out/images/box.png");
            Assert.Equal("A6-04-78-87-FC-41-65-97-76-D5-CB-4A-18-2F-33-7A", BitConverter.ToString(md5));

            string meta2 = File.ReadAllText("sample_out/sample2.meta");
            Assert.True(meta2.Contains("somethingelse"), "Packer should preserve our custom yaml files");
        }

        [Fact]
        public void RecursivePackTest()
        {
            Dictionary<string, string> fileMap = new Dictionary<string, string>
            {
                ["sample"] = "Assets/UnityPacker",
            };

            Packer.Pack(fileMap, "recursivesample.unitypackage");

            Assert.True(File.Exists("recursivesample.unitypackage"), "Package should have been created");

            Unpacker.Unpack("recursivesample.unitypackage", "rsample_out");

            Assert.True(File.Exists("rsample_out/Assets/UnityPacker/sample1.txt"), "sample1.txt should have been decompressed");
            Assert.True(File.Exists("rsample_out/Assets/UnityPacker/sample1.meta"), "sample1.meta should have been generated");

            Assert.True(File.Exists("rsample_out/Assets/UnityPacker/childfolder/sample2.txt"), "sample2.txt should have been decompressed");
            Assert.True(File.Exists("rsample_out/Assets/UnityPacker/childfolder/sample2.meta"), "sample2.meta should have been generated");
            Assert.True(File.Exists("rsample_out/Assets/UnityPacker/box.png"), "box.png should have been decompressed");
            Assert.True(File.Exists("rsample_out/Assets/UnityPacker/box.meta"), "box.meta should have been decompressed");

            string meta2 = File.ReadAllText("rsample_out/Assets/UnityPacker/childfolder/sample2.meta");
            Assert.True(meta2.Contains("somethingelse"), "Packer should preserve our custom yaml files");
        }

        private static byte[] GetMD5(string file)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}
