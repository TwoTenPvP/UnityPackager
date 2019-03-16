using System;
using System.Collections.Generic;
using System.IO;

namespace UnityPackager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                Environment.Exit(1);
                return;
            }
            else if (args[0].ToLower() == "pack")
            {
                if (args.Length <= 3)
                {
                    PrintUsage();
                    Environment.Exit(1);
                    return;
                }
                else if ((args.Length - 2) % 2 != 0)
                {
                    PrintUsage();
                    Environment.Exit(1);
                    return;
                }
            
                string outputFile = args[1];

                if (!Path.IsPathRooted(outputFile))
                    outputFile = Path.GetFullPath(outputFile);
            
                List<FileEntry> entries = new List<FileEntry>();
                
                for (int i = 2; i < args.Length; i += 2)
                {
                    FileEntry entry = new FileEntry();

                    if (!Path.IsPathRooted(args[i]))
                        entry.FullPath = Path.GetFullPath(args[i]);
                    else
                        entry.FullPath = args[i];

                    entry.OutputExportPath = args[i + 1];
                
                    entries.Add(entry);
                }

                Packer.Pack(entries.ToArray(), outputFile);
            } 
            else if (args[0].ToLower() == "unpack")
            {
                if (args.Length != 3)
                {
                    PrintUsage();
                    Environment.Exit(1);
                    return;
                }
                
                string inputFile = args[1];

                if (!Path.IsPathRooted(inputFile))
                    inputFile = Path.GetFullPath(inputFile);
                
                string outputFolder = args[2];

                if (!Path.IsPathRooted(outputFolder))
                    outputFolder = Path.GetFullPath(outputFolder);
                
                Unpacker.Unpack(inputFile, outputFolder);
            }
            else
            {
                PrintUsage();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\t" + "UnityPackager pack <output> [(<input-file> <target-path>)]...");
            Console.WriteLine("\t" + "UnityPackager unpack <input-file> <output-folder>");
            Console.WriteLine("Example:");
            Console.WriteLine("\t" + "UnityPackager pack MyPackage.unitypackage MyFile.cs Assets/MyFile.cs");
            Console.WriteLine("\t" + "UnityPackager unpack MyPackage.unitypackage MyProjectFolder");
        }

    }
}