using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ASCReader.Import {
    public static class HeightmapImporter {

        public static ASCData ImportHeightmap(string filepath) {
            FileStream stream = File.Open(filepath, FileMode.Open);
            var image = new Bitmap(stream);
            ASCData asc = new ASCData(image.Width,image.Height);
            Program.WriteLine(image.Width+"x"+image.Height);
            asc.cellsize = 1;
            asc.nodata_value = -9999;
            for(int x = 0; x < image.Width; x++) {
                for(int y = 0; y < image.Height; y++) {
                    Color c = image.GetPixel(x,y);
                    asc.data[x,y] = c.GetBrightness();
                }
            }
            asc.RecalculateValues(false);
            asc.lowPoint = 0;
            asc.highPoint = 1;
            image.Dispose();
            stream.Close();
            asc.isValid = true;
            return asc;
        }
    }
}