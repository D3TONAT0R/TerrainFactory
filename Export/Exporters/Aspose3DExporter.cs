using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ASCReader.Export;
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;

namespace ASCReader.Export.Exporters {
	public class Aspose3DExporter : IExporter {

		private Scene scene;

		public Aspose3DExporter(List<(List<Vector3> verts, List<int> tris)> meshInfo) {
			try {
				bool makeChildNodes = meshInfo.Count > 1;
				scene = new Scene();
				for(int i = 0; i < meshInfo.Count; i++) {
					var tuple = meshInfo[i];
					Mesh m = new Mesh();
					foreach(Vector3 v in meshInfo[i].verts) m.ControlPoints.Add(new Aspose.ThreeD.Utilities.Vector4(v.X, v.Y, v.Z, 1));
					for(int j = 0; j < tuple.tris.Count; j += 3) {
						m.CreatePolygon(tuple.tris[j], tuple.tris[j + 1], tuple.tris[j + 2]);
					}
					if(makeChildNodes) {
						Node n = new Node("mesh" + (i + 1), m);
						scene.RootNode.ChildNodes.Add(n);
					} else {
						scene.RootNode.AddEntity(m);
					}
				}
			} catch(Exception e) {
				Program.WriteError("ERROR while creating 3D data for Aspose3D:");
				Program.WriteLine(e.ToString());
				throw e;
			}
		}

		public void WriteFile(FileStream stream, FileFormat filetype) {
			if(filetype == FileFormat.MDL_3ds) {
				scene.Save(stream, Aspose.ThreeD.FileFormat.Discreet3DS);
			} else {
				scene.Save(stream, Aspose.ThreeD.FileFormat.FBX7300ASCII);
			}
		}
	}
}