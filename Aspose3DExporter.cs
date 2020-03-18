using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;

public class Aspose3DExporter {
    
    public static bool ExportModel3ds(List<Vector3> verts, List<int> tris, string filename) {
        try {
            Mesh m = new Mesh();
            foreach(Vector3 v in verts) m.ControlPoints.Add(new Aspose.ThreeD.Utilities.Vector4(v.X, v.Y, v.Z, 1));
            for(int i = 0; i < tris.Count; i+=3) {
                m.CreatePolygon(tris[i], tris[i+1], tris[i+2]);
            }
		    Scene scene = new Scene();
            Node n = new Node("mesh", m);
            scene.RootNode.ChildNodes.Add(n);
		    FileStream stream = new FileStream(filename, FileMode.CreateNew);
		    scene.Save(stream, Aspose.ThreeD.FileFormat.Discreet3DS);
		    stream.Close();
            return false;
        }
        catch(Exception e) {
            Console.WriteLine("ERROR while exporting 3ds using Aspose3D:");
            Console.WriteLine(e.ToString());
            return false;
        }
    }
}