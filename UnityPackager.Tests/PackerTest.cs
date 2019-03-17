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
            // now let's unpack and check it out
            string fullpath = Path.GetFullPath("sample.unitypackage");
            Console.WriteLine($"Full path is {fullpath}");

            Unpacker.Unpack("sample.unitypackage", "sample_out");

            Assert.True(File.Exists("sample_out/sample1.txt"), "sample1.txt should have been decompressed");
            Assert.True(File.Exists("sample_out/sample2.txt"), "sample2.txt should have been decompressed");
            Assert.True(File.Exists("sample_out/sample1.meta"), "sample1.meta should have been generated");
            Assert.True(File.Exists("sample_out/sample2.meta"), "sample2.meta should have been generated");
            Assert.True(File.Exists("sample_out/images/box.png"), "box.png should have been decompressed");
            Assert.True(File.Exists("sample_out/images/box.meta"), "box.meta should have been decompressed");

            // let's make sure the file was not modified
            byte[] md5 = GetMD5();

            Assert.Equal("A6-04-78-87-FC-41-65-97-76-D5-CB-4A-18-2F-33-7A", BitConverter.ToString(md5));
        }

        private static byte[] GetMD5()
        {
            using (MD5 md5 = MD5.Create())
            {
                using (var stream = File.OpenRead("sample_out/images/box.png"))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}
