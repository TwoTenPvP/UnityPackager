using System.IO;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Tar;

namespace UnityPackager
{
    public static class Utils
    {
        public static string CreateGuid(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.Unicode.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();

                foreach (byte b in hashBytes)
                {
                    stringBuilder.Append(b.ToString("X2"));
                }

                return stringBuilder.ToString();
            }
        }

        public static void AddFilesInDirRecursive(TarArchive archive, string directory)
        {
            string[] files = Directory.GetFiles(directory);

            for (int i = 0; i < files.Length; i++)
            {
                TarEntry entry = TarEntry.CreateEntryFromFile(files[i]);
                entry.Name = files[i].Remove(0, archive.RootPath.Length + 1).Replace('\\', '/');
                archive.WriteEntry(entry, true);
            }

            string[] subDirs = Directory.GetDirectories(directory);

            for (int i = 0; i < subDirs.Length; i++)
            {
                AddFilesInDirRecursive(archive, subDirs[i]);
            }
        }
    }
}
