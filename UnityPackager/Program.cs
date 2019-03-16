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

                Dictionary<string, string> fileMap = new Dictionary<string, string>();

                for (int i = 2; i < args.Length; i += 2)
                {
                    string fromPath = args[i];

                    if (!Path.IsPathRooted(args[i]))
                        fromPath = Path.GetFullPath(fromPath);

                    string toPath = args[i + 1];
                
                    fileMap.Add(fromPath, toPath);
                }

                Packer.Pack(fileMap, outputFile);
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