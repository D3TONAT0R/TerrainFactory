using HMCon;
using HMCon.Export;
using Assimp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using HMCon.Formats;

namespace HMCon3D {
	internal class Assimp3DExporter {

		private Scene scene;

		public Assimp3DExporter(ModelData model) {
			try {
				bool makeChildNodes = model.meshes.Count > 1;
				scene = new Scene();
				for(int i = 0; i < model.meshes.Count; i++) {
					var mesh = model.meshes[i];
					Mesh m = new Mesh();
					foreach(Vector3 v in mesh.vertices) m.Vertices.Add(new Vector3D(v.X, v.Y, v.Z));
					m.SetIndices(mesh.tris.ToArray(), 3);
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
				ConsoleOutput.WriteError("ERROR while creating 3D data for Assimp:");
				ConsoleOutput.WriteLine(e.ToString());
				throw e;
			}
		}

		public void WriteFile(FileStream stream, FileFormat ff) {
			AssimpContext context = new AssimpContext();
			var blob = context.ExportToBlob(scene, ff.Extension);
			stream.Write(blob.Data, 0, blob.Data.Length);
			context.Dispose();
		}
	}
}