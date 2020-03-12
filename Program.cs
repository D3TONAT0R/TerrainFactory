using System;

namespace ASCReader
{
    class Program
    {

        static ASCData data;
        static ExportOptions exportOptions;
        static void Main(string[] args)
        {
            Console.WriteLine("---------------------------------");
            Console.WriteLine("ASCII-GRID TO ASCII-XYZ CONVERTER");
            Console.WriteLine("---------------------------------");
            while(data == null || !data.isValid) {
                InputFile();
                if(data != null && data.isValid) {
                    if(!GetExportOptions()) data = null;
                    while (!OutputFile()) {
                        Console.WriteLine("Failure");
                    }
                    Console.WriteLine("---------------------------------");
                    data = null;
                }
            }
        }

        static void InputFile() {
            Console.WriteLine("Enter path to .asc file:");
            string path = GetInput();
            Console.WriteLine("Reading file "+path+" ...");
            data = new ASCData(path);
        }

        static bool OutputFile() {
            Console.WriteLine("Enter path to write .xyz file:");
            string path = GetInput();
            return data.WriteToXYZ(path, exportOptions);
        }

        static bool GetExportOptions() {
            Console.WriteLine("Export options (optional):");
            Console.WriteLine("Available export options:");
            Console.WriteLine("    subsample N       Only export every N-th cell");
            Console.WriteLine("    split N           Split files every NxN cells (minimum 32)");
            Console.WriteLine("Type 'export' when ready to export");
            Console.WriteLine("Type 'abort' to abort the export");
            String input;
            exportOptions = new ExportOptions(); 
            while(true) {
                input = GetInput();
                input = input.ToLower();
                if(input == "export") {
                    return true;
                } else if(input == "abort") {
                    Console.WriteLine("Export aborted");
                    return false;
                } else if(input.StartsWith("subsample")) {
                    string[] split = input.Split(' ');
                    if(split.Length > 1) {
                        int i;
                        if(int.TryParse(split[1], out i)) {
                            exportOptions.subsampling = i;
                            Console.WriteLine("Subsampling set to: "+i);
                        } else {
                            Console.WriteLine("Can't parse to int: "+split[1]);
                        }
                    } else {
                        Console.WriteLine("An integer is required!");
                    }
                } else if(input.StartsWith("split")) {
                    string[] split = input.Split(' ');
                    if(split.Length > 1) {
                        int i;
                        if(int.TryParse(split[1], out i)) {
                            exportOptions.fileSplitDims = i;
                            Console.WriteLine("File splitting set to: "+i+"x"+i);
                        } else {
                            Console.WriteLine("Can't parse to int: "+split[1]);
                        }
                    } else {
                        Console.WriteLine("An integer is required!");
                    }
                } else {
                    Console.WriteLine("Unknown option :"+input);
                }
            }
        }

        public static string GetInput() {
            string s = Console.ReadLine();
            Console.WriteLine("> "+s);
            return s;
        }
    }
}
