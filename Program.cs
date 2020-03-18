using System;

namespace ASCReader
{
    class Program
    {

        public static int exported3dFiles = 0;
        static ASCData data;
        static ExportOptions exportOptions;
        static void Main(string[] args)
        {
            Console.WriteLine("---------------------------------");
            Console.WriteLine("ASCII-GRID FILE CONVERTER");
            Console.WriteLine("---------------------------------");
            while(data == null || !data.isValid) {
                InputFile();
                if(data != null && data.isValid) {
                    if(!GetValidExportOptions()) {
                        data = null;
                        continue;
                    }
                    while (!OutputFiles()) {
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

        static bool OutputFiles() {
            Console.WriteLine("Enter path & name to write the file(s):");
            string path = GetInput();
            return data.WriteAllFiles(path, exportOptions);
        }


        static bool GetValidExportOptions() {
            if(!GetExportOptions()) return false;
            while(!ValidateExportOptions()) {
                Console.WriteLine("Cannot export with the current settings / format!");
                if(!GetExportOptions()) return false;
            }
            return true;
        }
        static bool GetExportOptions() {
            Console.WriteLine("Export options (optional):");
            Console.WriteLine("Available export options:");
            Console.WriteLine("    format N..        Export to the specified format(s)");
            Console.WriteLine("        xyz           ASCII-XYZ points");
            Console.WriteLine("        3ds           3d Mesh");
            Console.WriteLine("        png           Heightmap");
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
                } else if(input.StartsWith("format")) {
                    string[] split = input.Split(' ');
                    if(split.Length > 1) {
                        split[0] = null;
                        exportOptions.SetOutputFormats(split, false);
                        string str = "";
                        foreach(FileFormat ff in exportOptions.outputFormats) {
                            str += " "+ff;
                        }
                        if(str == "") str = " <NONE>";
                        Console.WriteLine("Exporting to the following format(s):"+str);
                    } else {
                        Console.WriteLine("A list of formats is required!");
                    }
                } else {
                    Console.WriteLine("Unknown option :"+input);
                }
            }
        }

        static bool ValidateExportOptions() {
            bool valid = true;
            int cellsPerFile = GetTotalExportCellsPerFile();
            if(exportOptions.outputFormats.Count == 0) {
                Console.WriteLine("ERROR: No export format is defined! choose at least one format for export: "+cellsPerFile);
                return false;
            }
            if(exportOptions.outputFormats.Contains(FileFormat.MDL_3ds)) {
                if(cellsPerFile >= 65535) {
                    Console.WriteLine("ERROR: Cannot export more than 65535 cells in a single 3ds file! Current amount: "+cellsPerFile);
                    Console.WriteLine("       Reduce splitting interval or increase subsampling to allow for exporting 3ds Files");
                    valid = false;
                }
            }
            return valid;
        }

        public static string GetInput() {
            string s = Console.ReadLine();
            Console.WriteLine("> "+s);
            return s;
        }

        private static int GetTotalExportCellsPerFile() {
            int cells = exportOptions.fileSplitDims >= 32 ? (int)Math.Pow(exportOptions.fileSplitDims, 2) : data.ncols*data.nrows;
            if(exportOptions.subsampling > 1) cells /= (int)Math.Pow(exportOptions.subsampling,2);
            return cells;
        }
    }
}
