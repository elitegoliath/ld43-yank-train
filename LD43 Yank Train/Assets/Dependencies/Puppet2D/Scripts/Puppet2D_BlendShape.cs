#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace Puppet2D
{
	[ExecuteInEditMode]
	public class Puppet2D_BlendShape : MonoBehaviour
	{
		public SkinnedMeshRenderer SourceSkin;
		public MeshFilter TargetMeshFilter;
		public string BlendShapeName = "newBlendShape";

		[SerializeField]
		public Mesh mesh;
		[SerializeField]
		public Vector3[] verts;
		[SerializeField]
		public float Falloff = 1f;
		//[SerializeField]
		private Vector3 vertPos;
		[SerializeField]
		public Puppet2D_VertexDeform[] Deforms;

		[SerializeField]
		private bool _intialised = false;

		public void Init()
		{
			mesh = GetComponent<MeshFilter>().sharedMesh;
			verts = mesh.vertices;
			Deforms = new Puppet2D_VertexDeform[verts.Length];
			List<Vector3> uniquePos = new List<Vector3>();

			int index = 0;
			foreach (Vector3 vert in verts)
			{
				

				vertPos = transform.TransformPoint(vert);
				GameObject Deform = new GameObject("vert" + index);
				Deform.transform.position = vertPos;
				Deform.transform.parent = transform;
				Puppet2D_VertexDeform giz = Deform.AddComponent<Puppet2D_VertexDeform>();
				giz.ConnectedIndexes = new List<int>();
				if (uniquePos.Contains(vert))
				{
					giz.Active = false;
					int dupIndex = uniquePos.IndexOf(vert);
					Deforms[dupIndex].ConnectedIndexes.Add(index);
					giz.ConnectedIndexes.Add(dupIndex);
				}

				uniquePos.Add(vert);


				giz.CurrentIndex = index;
				Deforms[index] = giz;
				giz._parent = this;
				giz.IntialPos = vertPos;

				index++;
			}
			int id1, id2, id0;
			for (int triIndex = 0; triIndex < mesh.triangles.Length ; triIndex+=3)
			{
				id0 = mesh.triangles[triIndex];
				id1 = mesh.triangles[triIndex+1];
				id2 = mesh.triangles[triIndex+2];

				if (!Deforms[id0].ConnectedIndexes.Contains(id1))
					Deforms[id0].ConnectedIndexes.Add(id1);
				if (!Deforms[id0].ConnectedIndexes.Contains(id2))
					Deforms[id0].ConnectedIndexes.Add(id2);

				if (!Deforms[id1].ConnectedIndexes.Contains(id0))
					Deforms[id1].ConnectedIndexes.Add(id0);
				if (!Deforms[id1].ConnectedIndexes.Contains(id2))
					Deforms[id1].ConnectedIndexes.Add(id2);

				if (!Deforms[id2].ConnectedIndexes.Contains(id0))
					Deforms[id2].ConnectedIndexes.Add(id0);
				if (!Deforms[id2].ConnectedIndexes.Contains(id1))
					Deforms[id2].ConnectedIndexes.Add(id1);

			}


				_intialised = true;

		}
		private void OnEnable()
		{
			Undo.undoRedoPerformed += OnUndoRedo;

		}
		void OnUndoRedo()
		{
			verts = mesh.vertices;
			for (int i = 0; i < Deforms.Length; i++)
			{
				if(Deforms[i]!=null)
					verts[i] = Deforms[i].transform.position;
			}
		}
		void LateUpdate()
		{
			if (_intialised)
				UpdateMesh();

		}
		void UpdateMesh()
		{


			mesh.vertices = verts;
			mesh.RecalculateBounds();

			//mesh.RecalculateNormals();


		}


		public void SetBlendShape()
		{

			Mesh mesh = TargetMeshFilter.sharedMesh;

			Mesh meshCurrent = SourceSkin.sharedMesh;

			Vector3[] deltas = new Vector3[mesh.vertexCount];
			Vector3[] deltaNormals = new Vector3[mesh.vertexCount];
			Vector3[] deltaTangents = new Vector3[mesh.vertexCount];


			for (int i = 0; i < mesh.vertexCount; i++)
			{
				deltas[i] = mesh.vertices[i] - meshCurrent.vertices[i];
				deltaNormals[i] = mesh.normals[i] - meshCurrent.normals[i];
				deltaTangents[i] = mesh.tangents[i] - meshCurrent.tangents[i];

			}


			ReplaceBlendShape(BlendShapeName, deltas, deltaNormals, deltaTangents);

		}

		public void ReplaceBlendShape(string blendShapeName, Vector3[] deltasNew, Vector3[] deltaNormalsNew, Vector3[] deltaTangentsNew)
		{
			//int shapeIndexToChange = SourceSkin.sharedMesh.GetBlendShapeIndex (blendShapeName);

			Mesh myMesh = SourceSkin.sharedMesh;
			
			Mesh tmpMesh = new Mesh();
			tmpMesh.vertices = myMesh.vertices;
			Vector3[] dVertices = new Vector3[myMesh.vertexCount];
			Vector3[] dNormals = new Vector3[myMesh.vertexCount];
			Vector3[] dTangents = new Vector3[myMesh.vertexCount];
			bool added = false;
			for (int shape = 0; shape < myMesh.blendShapeCount; shape++)
			{
				for (int frame = 0; frame < myMesh.GetBlendShapeFrameCount(shape); frame++)
				{
					string shapeName = myMesh.GetBlendShapeName(shape);
					float frameWeight = myMesh.GetBlendShapeFrameWeight(shape, frame);

					myMesh.GetBlendShapeFrameVertices(shape, frame, dVertices, dNormals, dTangents);

					if (shapeName == blendShapeName)
					{
						Debug.Log("Changing");
						dVertices = deltasNew;
						dNormals = deltaNormalsNew;
						dTangents = deltaTangentsNew;
						added = true;
					}



					tmpMesh.AddBlendShapeFrame(shapeName, frameWeight, dVertices, dNormals, dTangents);
					//BlendShapesAll newBlendShape = new BlendShapesAll (shape,frame, dVertices, dNormals, dTangents);
					//allBlendShapes.Add(newBlendShape);
				}
			}

			//SourceSkin.sharedMesh = tmpMesh;

			SourceSkin.sharedMesh.ClearBlendShapes();

			for (int shape = 0; shape < tmpMesh.blendShapeCount; shape++)
			{
				for (int frame = 0; frame < tmpMesh.GetBlendShapeFrameCount(shape); frame++)
				{
					string shapeName = tmpMesh.GetBlendShapeName(shape);
					float frameWeight = tmpMesh.GetBlendShapeFrameWeight(shape, frame);

					tmpMesh.GetBlendShapeFrameVertices(shape, frame, dVertices, dNormals, dTangents);

					myMesh.AddBlendShapeFrame(shapeName, frameWeight, dVertices, dNormals, dTangents);
					//BlendShapesAll newBlendShape = new BlendShapesAll (shape,frame, dVertices, dNormals, dTangents);
					//allBlendShapes.Add(newBlendShape);
				}
			}
			if (!added)
			{
				SourceSkin.sharedMesh.AddBlendShapeFrame(BlendShapeName, 1f, deltasNew, deltaNormalsNew, deltaTangentsNew);
			}


		}
	}
	
}
#endif