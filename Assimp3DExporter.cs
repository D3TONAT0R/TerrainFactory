using System;
using System.Collections.Generic;
using System.Numerics;
using Assimp;

public class Assimp3DExporter {

    public static bool ExportModel3ds(List<Vector3> verts, List<int> tris, string filename) {
        try {
            Mesh m = new Mesh();
            foreach(Vector3 v in verts) m.Vertices.Add(new Vector3D(v.X, v.Y, v.Z));
            m.SetIndices(tris.ToArray(), 3);
            Scene scene = new Scene();
            scene.Meshes.Add(m);
            Node n = new Node("mesh");
            scene.RootNode = new Node("root");
            scene.RootNode.Children.Add(n);
            n.MeshIndices.Add(0);
            AssimpContext context = new AssimpContext();
            context.ExportFile(scene, filename, "3ds");
            context.Dispose();
            return true;
        }
        catch(Exception e) {
            Console.WriteLine("ERROR while exporting 3ds using AssImp:");
            Console.WriteLine(e.ToString());
            return false;
        }
    }
}