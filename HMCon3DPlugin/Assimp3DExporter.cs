using HMCon;
using HMCon.Export;
using Assimp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace HMCon3D {
	public class Assimp3DExporter : IExporter {

		private Scene scene;

		public Assimp3DExporter(List<(List<Vector3> verts, List<int> tris, List<Vector2> uvs)> meshInfo) {
			try {
				bool makeChildNodes = meshInfo.Count > 1;
				scene = new Scene();
				for(int i = 0; i < meshInfo.Count; i++) {
					var tuple = meshInfo[i];
					Mesh m = new Mesh();
					foreach(Vector3 v in tuple.verts) m.Vertices.Add(new Vector3D(v.X, v.Y, v.Z));
					m.SetIndices(tuple.tris.ToArray(), 3);
					int index = scene.Meshes.Count;
					scene.Meshes.Add(m);
					if(makeChildNodes) {
						Node n = new Node("mesh" + (i + 1));
						n.MeshIndices.Add(index);
						scene.RootNode.Children.Add(n);
					} else {
						scene.RootNode.MeshIndices.Add(0);
					}
				}
			} catch(Exception e) {
				Program.WriteError("ERROR while creating 3D data for Assimp:");
				Program.WriteLine(e.ToString());
				throw e;
			}
		}

		public bool NeedsFileStream(FileFormat format) {
			return true;
		}

		public void WriteFile(FileStream stream, string path, FileFormat ff) {
			AssimpContext context = new AssimpContext();
			var blob = context.ExportToBlob(scene, ff.extension);
			stream.Write(blob.Data, 0, blob.Data.Length);
			context.Dispose();
		}
	}
}