using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reWZ;
using reWZ.WZProperties;
using System.IO;

namespace YNAMSExtractor
{
    class Program
    {
        const string wzLocationPath = @"C:\Nexon\Library\maplestory\appdata"; // Replace this with the location of your maplestory install, I guess.
        private static List<FileInfo> availableFiles = new List<FileInfo>();
        private static Dictionary<string, WZFile> loadedFiles = new Dictionary<string, WZFile>();

        static string GetWZFilePath(out SpecialChoices choice)
        {
            string path = null;
            choice = SpecialChoices.None;
            Console.WriteLine("Select a file: ");
            DirectoryInfo wzDir = new DirectoryInfo(wzLocationPath);
            // Display options here 'cause whynot

            // If we have no list, make one!
            if (availableFiles.Count == 0)
            {
                availableFiles.AddRange(wzDir.EnumerateFiles("*.wz", SearchOption.TopDirectoryOnly));
            }

            int numericChoice = printOptions<FileInfo>(availableFiles, fileInfo => fileInfo.Name, out choice);


            if (choice == SpecialChoices.ValueSelected)
            {
                FileInfo fileChoice = availableFiles[numericChoice];
                Console.WriteLine("Loading {0}...", fileChoice.Name);
                path = fileChoice.FullName;
            }

            return path;
        }


        static void Main(string[] args)
        {
            // To start, let's make a simple file explorer so I can view which files I want.
            SpecialChoices shouldQuit;
            // So it turns out the WzFile stuff is either broken or I'm dumb. If you try to call dispose, 
            // it doesn't appropriately close the memory mapped file so when you try to make a new one
            // it doesn't turn out so well.

            string path = GetWZFilePath(out shouldQuit);

            if (shouldQuit == SpecialChoices.Quit || shouldQuit == SpecialChoices.Back)
            {
                Console.WriteLine("Quitting...");
                return;
            }

            if (!loadedFiles.ContainsKey(path))
            {
                loadedFiles[path] = new WZFile(path, WZVariant.GMS, false); // We have to store the files we've already loaded because they don't get properly disposed ;-;
            }

            WZFile zFile = loadedFiles[path];
            DoCrawl(zFile);

            //Console.ReadKey();
        }

        private static void DoCrawl(WZFile zFile)
        {
            SpecialChoices choice;
            WZObject currentObject = zFile.MainDirectory;
            List<WZObject> objList = null;
            while (true)
            {
                Console.WriteLine("Current directory: {0}", currentObject == zFile.MainDirectory ? "Top" : currentObject.Path);
                objList = currentObject.ToList<WZObject>();
                int select = printOptions<WZObject>(objList, wzObj => wzObj.Name, out choice);

                if (choice == SpecialChoices.Quit || (choice == SpecialChoices.Back && currentObject == zFile.MainDirectory))
                {
                    break;
                }
                else if (choice == SpecialChoices.GetValue)
                {
                    printValue(currentObject);
                }
                else if (choice == SpecialChoices.Back)
                {
                    currentObject = currentObject.Parent;
                }
                else
                {
                    currentObject = objList[select];
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static int printOptions<T>(List<T> options, printValueFunc<T> valueFunc, out SpecialChoices choice)
        {
            choice = SpecialChoices.None;

            int numericChoice = -1;
            for (int index = 0; index < options.Count; index++)
            {
                Console.WriteLine(" - {0}. {1}", index + 1, valueFunc(options[index]));
            }

            if (options.Count == 0)
            {
                Console.WriteLine("No available options.");
            }

            Console.WriteLine();
            

            // TODO: Add bounds checking and re-trying till they select a real option
            while (choice == SpecialChoices.None)
            {
                Console.WriteLine("Special Options: Q - Quit, B - Back, V - Get Value");
                Console.Write("Option: ");
                string input = Console.ReadLine();
                Console.WriteLine();

                switch (input.ToLower())
                {
                    // Back will return -1 which signals we should go up
                    case "b":
                    case ".":
                    case "back":
                    case "":
                        {
                            choice = SpecialChoices.Back;
                            break;
                        }
                    case "q":
                    case "quit":
                        {
                            choice = SpecialChoices.Quit;
                            break;
                        }
                    case "v":
                        {
                            choice = SpecialChoices.GetValue;
                            break;
                        }
                    default:
                        {
                            bool success = int.TryParse(input, out numericChoice);
                            if (success && numericChoice-1 >= 0 && numericChoice-1 < options.Count)
                            {
                                choice = SpecialChoices.ValueSelected;
                            }
                            else
                            {
                                Console.WriteLine("Invalid choice!\n");
                            }
                            break;
                        }
                }
            }


            return numericChoice - 1;
        }
        private static void printValue(WZObject obj)
        {
            Console.WriteLine("{0} is a {1}.", obj.Name, obj.Type.ToString());
            // I'm sure there's a better way but I'm tired and this is straight forward
            switch (obj.Type)
            {
                case WZObjectType.String:
                    {
                        Console.WriteLine("Value = {0}", obj.ValueOrDie<String>());
                        break;
                    }
                case WZObjectType.Double:
                    {
                        Console.WriteLine("Value = {0}", obj.ValueOrDie<Double>());
                        break;
                    }
                case WZObjectType.Single:
                    {
                        Console.WriteLine("Value = {0}", obj.ValueOrDie<Single>());
                        break;
                    }
                case WZObjectType.Int64:
                    {
                        Console.WriteLine("Value = {0}", obj.ValueOrDie<Int64>());
                        break;
                    }
                case WZObjectType.UInt16:
                    {
                        Console.WriteLine("Value = {0}", obj.ValueOrDie<Int64>());
                        break;
                    }
                case WZObjectType.Int32:
                    {
                        Console.WriteLine("Value = {0}", obj.ValueOrDie<Int32>());
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Cannot display value");
                        break;
                    }
            }
        }

        private delegate string printValueFunc<T>(T obj);

        private enum SpecialChoices
        {
            None,
            ValueSelected,
            GetValue,
            Back,
            Quit
        }
    }
}
