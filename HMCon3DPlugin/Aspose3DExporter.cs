using HMCon;
using HMCon.Export;
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
//using Aspose.ThreeD.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace HMCon3D {
	internal class Aspose3DExporter {

		private readonly Scene scene;

		public Aspose3DExporter(ModelData model) {
			try {
				bool makeChildNodes = model.meshes.Count > 1;
				scene = new Scene();
				for(int i = 0; i < model.meshes.Count; i++) {
					var mesh = model.meshes[i];
					Mesh m = new Mesh();
					foreach(Vector3 v in mesh.vertices) m.ControlPoints.Add(new Aspose.ThreeD.Utilities.Vector4(v.X, v.Y, v.Z, 1));
					for(int j = 0; j < mesh.tris.Count; j += 3) {
						m.CreatePolygon(mesh.tris[j], mesh.tris[j + 1], mesh.tris[j + 2]);
					}
					var elem = m.CreateElementUV(TextureMapping.Diffuse, MappingMode.PolygonVertex, ReferenceMode.Direct);
					var uv = new List<Aspose.ThreeD.Utilities.Vector4>();
					for(int k = 0; k < mesh.uvs.Count; k++) {
						uv.Add(new Aspose.ThreeD.Utilities.Vector4(mesh.uvs[k].X, mesh.uvs[k].Y, 0, 1));
					}
					elem.Data.AddRange(uv);
					elem.Indices.AddRange(mesh.tris);
					if(makeChildNodes) {
						Node n = new Node("mesh" + (i + 1), m);
						scene.RootNode.ChildNodes.Add(n);
					} else {
						Node n = new Node("mesh", m);
						scene.RootNode.ChildNodes.Add(n);
					}
				}
			} catch(Exception e) {
				ConsoleOutput.WriteError("ERROR while creating 3D data for Aspose3D:");
				ConsoleOutput.WriteLine(e.ToString());
				throw e;
			}
		}

		public void WriteFile(FileStream stream, HMCon.Formats.FileFormat filetype) {
			if(filetype is Autodesk3DSFormat) {
				scene.Save(stream, Aspose.ThreeD.FileFormat.Discreet3DS);
			} else if(filetype is FBXFormat) {
				scene.Save(stream, Aspose.ThreeD.FileFormat.FBX7300ASCII);
			}
		}
	}
}