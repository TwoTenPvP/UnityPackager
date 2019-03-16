using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;
using UnityPackager;
using Xunit;

namespace Tests
{
    public class FoldersTest
    {
        // check that the tar contains the expected files
        private static void VerifyTar(HashSet<String> expected, MemoryStream outstream)
        {
            var instream = new MemoryStream(outstream.ToArray(), false);

            using (TarArchive archive = TarArchive.CreateInputTarArchive(instream))
            {
                var entries = new HashSet<string>();

                archive.ProgressMessageEvent += (ar, entry, message) =>
                {
                    entries.Add(entry.Name);
                };

                archive.ListContents();

                Assert.Equal(expected, entries);
            }
        }


        [Fact]
        public void TestRecursiveAdd()
        {
            var outstream = new MemoryStream();

            using (TarArchive archive = TarArchive.CreateOutputTarArchive(outstream))
            {
                archive.AddFilesRecursive("sample");
            }

            var expected = new HashSet<string> { "sample/sample1.txt", "sample/childfolder/sample2.txt" };

            VerifyTar(expected, outstream);

        }


        [Fact]
        public void TestRecursiveAddStripping()
        {
            var outstream = new MemoryStream();

            using (TarArchive archive = TarArchive.CreateOutputTarArchive(outstream))
            {
                archive.RootPath = "sample";
                archive.AddFilesRecursive("sample");
            }

            var expected = new HashSet<string> { "sample1.txt", "childfolder/sample2.txt" };

            VerifyTar(expected, outstream);

        }


    }
}
