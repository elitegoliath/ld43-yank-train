using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
namespace Puppet2D
{
	public class Puppet2D_Skinning : Editor
	{
		private static GameObject _workingMesh;
		private static GameObject[] _workingBones;

		[MenuItem("GameObject/Puppet2D/Skin/ConvertSpriteToMesh")]
		public static void ConvertSpriteToMeshMenu()
		{
			ConvertSpriteToMesh(0);
		}
		public static void ConvertSpriteToMesh(int triIndex)
		{
			GameObject[] selection = Selection.gameObjects;
			foreach (GameObject spriteGO in selection)
			{
				if (spriteGO.GetComponent<SpriteRenderer>())
				{
					string spriteName = spriteGO.GetComponent<SpriteRenderer>().sprite.name;
					Sprite spriteInfo = spriteGO.GetComponent<SpriteRenderer>().sprite;
					//                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteInfo)) as TextureImporter;
					//                SpriteMetaData[] smdArray = textureImporter.spritesheet;

					//                Texture2D tex = spriteGO.GetComponent<SpriteRenderer>().sprite.texture;

					Bounds bounds = spriteGO.GetComponent<SpriteRenderer>().bounds;// Puppet2D_AutoRig.GetBounds(spriteGO);

					if (spriteName.Contains("Bone"))
					{
						Debug.LogWarning("You can't convert Bones to Mesh");
						return;
					}
					if ((spriteName == "orientControl") || (spriteName == "parentControl") || (spriteName == "VertexHandleControl") || (spriteName == "IKControl"))
					{
						Debug.LogWarning("You can't convert Controls to Mesh");
						return;
					}
					PolygonCollider2D polyCol;
					GameObject MeshedSprite;
					Quaternion rot = spriteGO.transform.rotation;
					spriteGO.transform.eulerAngles = Vector3.zero;
					int layer = spriteGO.layer;
					string sortingLayer = spriteGO.GetComponent<Renderer>().sortingLayerName;
					int sortingOrder = spriteGO.GetComponent<Renderer>().sortingOrder;


					if (spriteGO.GetComponent<PolygonCollider2D>() == null)
					{
						polyCol = Undo.AddComponent<PolygonCollider2D>(spriteGO);
						Puppet2D_CreatePolygonFromSprite polyFromSprite = ScriptableObject.CreateInstance("Puppet2D_CreatePolygonFromSprite") as Puppet2D_CreatePolygonFromSprite;
						MeshedSprite = polyFromSprite.Run(spriteGO.transform, true, triIndex);

						MeshedSprite.name = (spriteGO.name + "_GEO");
						DestroyImmediate(polyFromSprite);
						Undo.DestroyObjectImmediate(polyCol);



					}
					else
					{
						polyCol = spriteGO.GetComponent<PolygonCollider2D>();

						Puppet2D_CreatePolygonFromSprite polyFromSprite = ScriptableObject.CreateInstance("Puppet2D_CreatePolygonFromSprite") as Puppet2D_CreatePolygonFromSprite;
						MeshedSprite = polyFromSprite.Run(spriteGO.transform, true, triIndex);

						MeshedSprite.name = (spriteGO.name + "_GEO");

						DestroyImmediate(polyFromSprite);
						Undo.DestroyObjectImmediate(polyCol);

					}
					MeshedSprite.layer = layer;
					MeshedSprite.GetComponent<Renderer>().sortingLayerName = sortingLayer;
					MeshedSprite.GetComponent<Renderer>().sortingOrder = sortingOrder;
					Puppet2D_SortingLayer sortingLayerComp = MeshedSprite.AddComponent<Puppet2D_SortingLayer>();
					sortingLayerComp.bounds = bounds;

					MeshedSprite.transform.position = spriteGO.transform.position;
					MeshedSprite.transform.rotation = rot;

					//Sprite spriteInfo = spriteGO.GetComponent<SpriteRenderer>().sprite;

					//TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteInfo)) as TextureImporter;

					MeshedSprite.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Unlit/Transparent");

					MeshedSprite.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", spriteInfo.texture);

					//textureImporter.textureType = TextureImporterType.Sprite;

					DestroyImmediate(spriteGO);

					GameObject globalCtrl = Puppet2D_CreateControls.CreateGlobalControl();

					if (globalCtrl != null)
					{

						MeshedSprite.transform.parent = globalCtrl.transform;

					}

					Selection.activeGameObject = MeshedSprite;

				}
				else
				{
					Debug.LogWarning("Object is not a sprite");
					return;
				}
			}
		}
		[MenuItem("GameObject/Puppet2D/Skin/Parent Mesh To Bones")]
		public static void BindRigidSkinMenu()
		{
			BindRigidSkin();
		}

		public static void BindRigidSkin(List<GameObject> selectedBones = null, List<GameObject> selectedMeshes = null)
		{
			if (selectedMeshes == null)
			{
				GameObject[] selection = Selection.gameObjects;
				selectedBones = new List<GameObject>();
				selectedMeshes = new List<GameObject>();

				foreach (GameObject Obj in selection)
				{
					if (Obj.GetComponent<SpriteRenderer>())
					{
						if (Obj.GetComponent<SpriteRenderer>().sprite && Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("ffd"))
							selectedMeshes.Add(Obj.transform.parent.gameObject);
						else
						{
							if (Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("Bone"))
							{

								if (Obj.transform.childCount > 0)
								{
									if (!Obj.GetComponent<Puppet2D_HiddenBone>())
										selectedBones.Add(Obj);

								}
								else
									selectedBones.Add(Obj);

							}
							else
							{
								selectedMeshes.Add(Obj);
							}
						}
					}
					else
					{
						selectedMeshes.Add(Obj);
					}
				}
			}
			if ((selectedBones.Count == 0) || (selectedMeshes.Count == 0))
			{
				Debug.LogWarning("You need to select at least one bone and one other object");
				return;
			}
			foreach (GameObject mesh in selectedMeshes)
			{
				float testdist = 1000000;
				GameObject closestBone = null;
				for (int i = 0; i < selectedBones.Count; i++)
				{
					GameObject bone = selectedBones[i];
					//float dist = Vector2.Distance(new Vector2(bone.GetComponent<SpriteRenderer>().bounds.center.x, bone.GetComponent<Renderer>().bounds.center.y), new Vector2(mesh.transform.position.x, mesh.transform.position.y));
					Vector3 centre;
					if (bone.transform.parent != null)
						centre = (bone.transform.position + bone.transform.parent.position) / 2f;
					else
						centre = bone.transform.position;
					float dist = Vector2.Distance(new Vector2(centre.x, centre.y), new Vector2(mesh.transform.position.x, mesh.transform.position.y));

					if (dist < testdist)
					{

						testdist = dist;

						closestBone = bone.transform.parent.gameObject;
					}

				}

				Undo.SetTransformParent(mesh.transform, closestBone.transform, "parent bone");

			}

		}
		static bool ContainsPoint(Vector3[] polyPoints, Vector3 p)
		{
			bool inside = false;
			float a1 = Vector3.Angle(polyPoints[0] - p, polyPoints[1] - p);
			float a2 = Vector3.Angle(polyPoints[1] - p, polyPoints[2] - p);
			float a3 = Vector3.Angle(polyPoints[2] - p, polyPoints[0] - p);

			if (Mathf.Abs((a1 + a2 + a3) - 360) < 0.1f)
			{
				inside = true;
				//Debug.Log((a1 + a2 + a3));
			}
			//        for (int index = 0; index < polyPoints.Length; j = index++) 
			//        { 
			//            if ( ((polyPoints[index].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[index].y)) && 
			//                (p.x < (polyPoints[j].x - polyPoints[index].x) * (p.y - polyPoints[index].y) / (polyPoints[j].y - polyPoints[index].y) + polyPoints[index].x)) 
			//                inside = !inside; 
			//        } 
			return inside;
		}
		static Vector3 Barycentric(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
		{
			Vector3 v0 = b - a;
			Vector3 v1 = c - a;
			Vector3 v2 = p - a;
			float d00 = Vector3.Dot(v0, v0);
			float d01 = Vector3.Dot(v0, v1);
			float d11 = Vector3.Dot(v1, v1);
			float d20 = Vector3.Dot(v2, v0);
			float d21 = Vector3.Dot(v2, v1);
			float denom = d00 * d11 - d01 * d01;


			float v = (d11 * d20 - d01 * d21) / denom;
			float w = (d00 * d21 - d01 * d20) / denom;
			float u = 1.0f - v - w;
			return new Vector3(v, w, u);
		}

		//	public static void UndoDeleteMeshRenderer ()
		//	{
		//		Undo.undoRedoPerformed -= UndoDeleteMeshRenderer;
		//		if(_workingMesh)
		//		{
		//			Material m = _workingMesh.GetComponent<SkinnedMeshRenderer> ().sharedMaterial;
		//			DestroyImmediate (_workingMesh.GetComponent<SkinnedMeshRenderer> ());
		//			MeshRenderer mr = _workingMesh.AddComponent<MeshRenderer> ();
		//			mr.sharedMaterial = m;
		//		}
		//
		//	}
		/*public static void UndoAddSkinnedMeshRenderer ()
		{


			//Selection 
			BindSmoothSkin ();

			Undo.undoRedoPerformed += UndoDeleteMeshRenderer;
			Undo.undoRedoPerformed -= UndoAddSkinnedMeshRenderer;

		}*/
		[MenuItem("GameObject/Puppet2D/Skin/Bind Smooth Skin")]
		public static void BindSmoothSkinMenu()
		{
			BindSmoothSkin();
		}
		public static GameObject BindSmoothSkin(int isGeosedic = 0)
		{
			GameObject[] selection = Selection.gameObjects;
			List<Transform> selectedBones = new List<Transform>();
			List<GameObject> selectedMeshes = new List<GameObject>();
			List<GameObject> ffdControls = new List<GameObject>();

			foreach (GameObject Obj in selection)
			{
				if (Obj.GetComponent<SpriteRenderer>() == null)
				{
					if ((Obj.GetComponent<MeshRenderer>()) || (Obj.GetComponent<SkinnedMeshRenderer>()))
					{
						selectedMeshes.Add(Obj);
					}
					else
					{
						Debug.LogWarning("Please select a mesh with a MeshRenderer, and some bones");
						//return null;
					}

				}
				else if (Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("Bone"))
				{
					if (Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("ffdBone"))
						ffdControls.Add(Obj);

					selectedBones.Add(Obj.transform);
					if (Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("BoneScaled"))
						Obj.GetComponent<SpriteRenderer>().sprite = Puppet2D_Editor.boneOriginal;


				}
				else
				{
					Debug.LogWarning("Please select a mesh with a MeshRenderer, not a sprite");
					//return null;
				}
			}

			if (selectedBones.Count == 0)
			{
				if (selectedMeshes.Count > 0)
				{
					if (EditorUtility.DisplayDialog("Detatch Skin?", "Do you want to detatch the Skin From the bones?", "Detach", "Do Not Detach"))
					{
						foreach (GameObject mesh in selectedMeshes)
						{
							SkinnedMeshRenderer smr = mesh.GetComponent<SkinnedMeshRenderer>();
							if (smr)
							{
								Material mat = smr.sharedMaterial;
								Undo.DestroyObjectImmediate(smr);
								MeshRenderer mr = mesh.AddComponent<MeshRenderer>();
								mr.sharedMaterial = mat;
							}
						}
						return null;
					}



				}
				return null;
			}
			for (int i = selectedMeshes.Count - 1; i >= 0; i--)
			{
				// check to make sure its not a FFD mesh
				GameObject mesh = selectedMeshes[i];
				Puppet2D_FFDLineDisplay[] allFFDPointsInScene = Transform.FindObjectsOfType<Puppet2D_FFDLineDisplay>();
				bool isFFDMesh = false;
				foreach (Puppet2D_FFDLineDisplay ffdPoint in allFFDPointsInScene)
				{
					if (ffdPoint.outputSkinnedMesh && ffdPoint.outputSkinnedMesh.gameObject == mesh)
					{
						ffdControls.Add(ffdPoint.gameObject);
						selectedBones.Add(ffdPoint.transform);
						isFFDMesh = true;
					}
				}
				if (isFFDMesh)
					selectedMeshes.Remove(mesh);
			}
			if ((ffdControls.Count > 0) && selectedMeshes.Count == 0 && ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh)
			{
				GameObject preSkinnedMesh = new GameObject();
				MeshFilter mf = preSkinnedMesh.AddComponent<MeshFilter>();
				preSkinnedMesh.AddComponent<MeshRenderer>();
				Mesh mesh = new Mesh();
				ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh.BakeMesh(mesh);
				mf.sharedMesh = mesh;

				List<Object> newObjs = new List<Object>();
				foreach (Transform tr in selectedBones)
				{
					if (tr.GetComponent<SpriteRenderer>() && tr.GetComponent<SpriteRenderer>().sprite && !tr.GetComponent<SpriteRenderer>().sprite.name.Contains("ffd") && !tr.GetComponent<Puppet2D_HiddenBone>())
						newObjs.Add(tr.gameObject);
				}
				newObjs.Add(preSkinnedMesh);
				Selection.objects = newObjs.ToArray();
				GameObject newGO = BindSmoothSkin(1);
				foreach (GameObject go in ffdControls)
				{
					go.GetComponent<Puppet2D_FFDLineDisplay>().skinnedMesh = newGO.GetComponent<SkinnedMeshRenderer>();
					go.GetComponent<Puppet2D_FFDLineDisplay>().Init();
				}
				DestroyImmediate(newGO);

				return preSkinnedMesh;
			}
			for (int ind = 0; ind < selectedMeshes.Count; ind++)
			{
				if (isGeosedic == 0 && selectedMeshes[ind].transform.rotation == Quaternion.identity)
					GeosedicSkinWeights(selectedMeshes[ind], selectedBones);
				else
					EuclidianSkinWeights(selectedMeshes[ind], selectedBones, ffdControls);

			}
			foreach (Transform bone in selectedBones)
			{
				if (bone.GetComponent<SpriteRenderer>().sprite.name == "Bone")
					bone.GetComponent<SpriteRenderer>().sprite = Puppet2D_Editor.boneSprite;
			}
			if (selectedMeshes.Count > 0)
			{
				//SortVertices(selectedMeshes[0]);
				return selectedMeshes[0];
			}
			else
				return null;


		}
		static private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
		{
			Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
			Color[] rpixels = result.GetPixels(0);
			float incX = (1.0f / (float)targetWidth);
			float incY = (1.0f / (float)targetHeight);
			for (int px = 0; px < rpixels.Length; px++)
			{
				rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
			}
			result.SetPixels(rpixels, 0);
			result.Apply();
			return result;
		}
		/*
		static void GeosedicSkinWeights(GameObject mesh, List<Transform> selectedBones)
		{
			EditorUtility.DisplayProgressBar("Binding Skin", "Generating Heat Map", 0);

			Bounds bounds;
			string sortingLayer;
			int sortingOrder;
			SkinnedMeshRenderer renderer;
			Mesh sharedMesh;
			Vector3[] verts;
			Matrix4x4[] bindPoses;
			BoneWeight[] weights;
			Material mat;

			SetupSkinnedMesh(mesh, selectedBones, out bounds, out sortingLayer, out sortingOrder, out renderer, out sharedMesh, out verts, out bindPoses, out weights, out mat);




			Texture2D tex = mat.mainTexture as Texture2D;
			int Width = tex.width / 1;
			int Height = tex.height / 1;
			// Debug.Log(Width);
			// Debug.Log(Height);
			SetTextureImporterFormat(tex, true);

			//Texture2D tex2 = ScaleTexture(tex,Width,Height)  ;


			tex.filterMode = FilterMode.Point;

			Color[] pix = tex.GetPixels(0, 0, Width, Height);

			//        Color[] pix = new Color[originalPix.Length];
			//        for (int p=0; p<pix.Length; p++)
			//        {
			//            pix[p] = originalPix[p];
			//        }


			float[,] boneWeights = new float[selectedBones.Count, pix.Length];
			for (int boneIndex = 0; boneIndex < selectedBones.Count; boneIndex++)
			{
				for (int i = 0; i < pix.Length; i++)
				{
					boneWeights[boneIndex, i] = -1f;
				}
			}


			//        for (int p=0; p<pix.Length; p++)
			//        {
			//            pix[p] = new Color(0,0,0, pix[p].a);
			//
			//        }

			bool[] filledPixels = new bool[pix.Length];
			for (int b = 0; b < filledPixels.Length; b++)
				filledPixels[b] = false;

			int counter = 0;

			for (int boneIndex = 0; boneIndex < selectedBones.Count; boneIndex++)
			{
				Vector2 startPos = WorldPosToXY(selectedBones[boneIndex].position, bounds, Width, Height);
				Vector2 endPos = startPos;


				Queue<int> _pixelQueue = new Queue<int>();
				Queue<int> _weightsQueue = new Queue<int>();

				int childrenNumber = selectedBones[boneIndex].childCount;
				if (childrenNumber > 0)
				{

					for (int j = 0; j < childrenNumber; j++)
					{
						if (selectedBones[boneIndex].GetChild(j) && !selectedBones[boneIndex].GetChild(j).GetComponent<Puppet2D_HiddenBone>())
						{
							endPos = WorldPosToXY(selectedBones[boneIndex].GetChild(j).position, bounds, Width, Height);

							List<Vector2> linePixels = DrawLine(startPos, endPos);
							foreach (Vector2 pixel in linePixels)
							{
								int indx = (int)pixel.y * Width + (int)pixel.x;
								if (indx >= 0 && indx < pix.Length && pix[indx].a > 0)
								{
									_pixelQueue.Enqueue(indx);
									_weightsQueue.Enqueue(0);
								}
							}
						}
					}
				}
				else
				{

					Transform parentBone = selectedBones[boneIndex].parent;
					Vector2 directionNormalised = (startPos - new Vector2(parentBone.position.x, parentBone.position.y)).normalized;
					endPos = startPos + directionNormalised * 10f;
					List<Vector2> linePixels = DrawLine(startPos, endPos);
					foreach (Vector2 pixel in linePixels)
					{
						int indx = (int)pixel.y * Width + (int)pixel.x;
						if (indx >= 0 && indx < pix.Length && pix[indx].a > 0)
						{
							_pixelQueue.Enqueue(indx);
							_weightsQueue.Enqueue(0);
						}
					}
				}


				bool[] completedPixels = new bool[pix.Length];

				for (int b = 0; b < completedPixels.Length; b++)
					completedPixels[b] = false;
				while (_pixelQueue.Count > 0)
				{
					if (counter % 5000 == 1)
						EditorUtility.DisplayProgressBar("Binding Skin", "Generating Heat Map", (float)counter / (pix.Length * selectedBones.Count));

					counter++;

					int i = _pixelQueue.Dequeue();
					int weight = _weightsQueue.Dequeue();
					if (i > 0 && i < completedPixels.Length && completedPixels[i] == false && pix[i].a > 0f)
					{
						//                    if(selectedBones[ boneIndex].name=="legR_1")
						//                       pix[i] = new Color(1-weight/200f,1-weight/200f,1-weight/200f,1);
						boneWeights[boneIndex, i] = weight;

						weight++;
						completedPixels[i] = true;
						filledPixels[i] = true;

						int x = i % Width;
						int y = Mathf.FloorToInt(i / Width);

						int topIndex = (y - 1) * Width + x;
						int bottomIndex = (y + 1) * Width + x;
						int leftIndex = (y) * Width + (x - 1);
						int rightIndex = (y) * Width + (x + 1);

						_pixelQueue.Enqueue(topIndex);
						_weightsQueue.Enqueue(weight);

						_pixelQueue.Enqueue(bottomIndex);
						_weightsQueue.Enqueue(weight);

						_pixelQueue.Enqueue(leftIndex);
						_weightsQueue.Enqueue(weight);

						_pixelQueue.Enqueue(rightIndex);
						_weightsQueue.Enqueue(weight);



					}

				}

			}
			for (int p = 0; p < pix.Length; p++)
			{
				if (pix[p].a >= 0.1f && filledPixels[p] == false)
				{
					pix[p] = new Color(0, 0, 0, 0);
				}

			}

			for (int p = 0; p < verts.Length; p++)
			{
				if (counter % 5000 == 1)
					EditorUtility.DisplayProgressBar("Binding Skin", "Generating Heat Map", (float)counter / (pix.Length * selectedBones.Count));

				counter += 1;

				bool[] completedPixels = new bool[pix.Length];

				for (int b = 0; b < completedPixels.Length; b++)
					completedPixels[b] = false;

				//            Debug.Log("test");

				int idx = WorldPosToIndex(mesh.transform.TransformPoint(verts[p]), bounds, Width, Height);
				int prevIdx = idx;
				if (idx >= 0 && idx < pix.Length)
				{
					Queue<int> _indexQueue = new Queue<int>();

					// get first non transparent pixel
					int x = idx % Width;
					int y = Mathf.FloorToInt(idx / Width);

					if (pix[idx].a < .1f)
						_indexQueue.Enqueue(idx);

					int foundIndex = -1;


					while (_indexQueue.Count > 0)
					{


						idx = _indexQueue.Dequeue();
						//pix[idx] = new Color(1,1,1,pix[idx].a);
						if (idx >= 0 && idx < pix.Length)
						{
							if (pix[idx].a >= .1f)
							{
								foundIndex = idx;

							}
							if (pix[idx].a < .1f && completedPixels[idx] == false)
							{
								completedPixels[idx] = true;

								x = idx % Width;
								y = Mathf.FloorToInt(idx / Width);
								//Debug.Log("x " + x + " y " + y);

								int topIndex = (y - 1) * Width + x;
								int bottomIndex = (y + 1) * Width + x;
								int leftIndex = (y) * Width + (x - 1);
								int rightIndex = (y) * Width + (x + 1);

								if (topIndex <= pix.Length && topIndex >= 0)
									_indexQueue.Enqueue(topIndex);
								if (bottomIndex <= pix.Length && bottomIndex >= 0)
									_indexQueue.Enqueue(bottomIndex);
								if (leftIndex <= pix.Length && leftIndex >= 0)
									_indexQueue.Enqueue(leftIndex);
								if (rightIndex <= pix.Length && rightIndex >= 0)
									_indexQueue.Enqueue(rightIndex);


							}
							if (foundIndex >= 0)
							{
								//pix[idx] = new Color(1,0,0,pix[idx].a);
								idx = foundIndex;
								_indexQueue.Clear();
								if (prevIdx == idx)
								{
									//                                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
									//                                cube.transform.position = mesh.transform.TransformPoint(verts[p]);
								}
							}
						}

					}

					if (foundIndex >= 0)
						idx = foundIndex;

					int[] boneIndex = GetFirstSecondBone(idx, boneWeights);
					weights[p].boneIndex0 = boneIndex[0];
					weights[p].boneIndex1 = boneIndex[1];

					float dif = boneWeights[boneIndex[1], idx] - boneWeights[boneIndex[0], idx];
					float falloff = 15f;
					float w1 = Mathf.Clamp01(.5f - (dif) / (Height / falloff));
					float w0 = 1f - w1;
					//Debug.Log(selectedBones[boneIndex[0]] + " " + selectedBones[boneIndex[0]].childCount);
					if (selectedBones[boneIndex[0]].childCount == 0)
					{
						w1 = 0f;
						w0 = 1f;
					}


					weights[p].weight0 = w0;
					weights[p].weight1 = w1;

				}
				else
				{
					weights[p].boneIndex0 = 0;
					weights[p].weight0 = 1;
				}



			}
			renderer.quality = SkinQuality.Bone2;

			FinishSkinnedMesh(mesh, selectedBones, bounds, sortingLayer, sortingOrder, renderer, sharedMesh, bindPoses, weights, mat);

			EditorUtility.ClearProgressBar();
			//        Texture2D destTex = new Texture2D(Width, Height);
			//        destTex.SetPixels(pix);
			//        destTex.name = "test";
			//        destTex.Apply();
			//        mat.mainTexture = destTex;

			renderer.sharedMaterial = mat;
		}
		*/
		static Puppet2D_Voxel GetVoxel(Puppet2D_Voxel[,,] voxels, Bounds bounds, Vector3 pos, int resolution)
		{
			//float height = ((bounds.size.x+ bounds.size.y + bounds.size.z)/3f);
			//float voxelSize = height/(float)resolution;
			float voxelSize = GetVoxelSize(new Vector3(bounds.size.x, bounds.size.y, bounds.size.z), resolution);


			int x = Mathf.RoundToInt((pos.x - bounds.min.x - (voxelSize * .5f)) / voxelSize) + 1;
			int y = Mathf.RoundToInt((pos.y - bounds.min.y - (voxelSize * .5f)) / voxelSize) + 1;
			int z = Mathf.RoundToInt((pos.z - bounds.min.z - (voxelSize * .5f)) / voxelSize) + 1;



			if (x > -1 && y > -1 && z > -1 && x < voxels.GetLength(0) && y < voxels.GetLength(1) && z < voxels.GetLength(2))
			{
				return voxels[x, y, z];
			}
			else
			{
				return null;
			}
		}
		static Puppet2D_Voxel GetVoxelInRadius(Puppet2D_Voxel[,,] voxels, Bounds bounds, Vector3 pos, float radius, int resolution)
		{
			//float height = ((bounds.size.x+ bounds.size.y + bounds.size.z)/3f);
			//float voxelSize = height/resolution;
			float voxelSize = GetVoxelSize(new Vector3(bounds.size.x, bounds.size.y, bounds.size.z), resolution);


			int x = (int)((pos.x - bounds.min.x) / voxelSize);
			int y = (int)((pos.y - bounds.min.y) / voxelSize);
			int z = (int)((pos.z - bounds.min.z) / voxelSize);


			return voxels[x, y, z];
		}

		static bool IsVoxelInside(Bounds bounds, Collider col, Vector3[] verts, float x, float y, float z, int i, GameObject cube)
		{
			Vector3 Start = new Vector3(x, y, z);
			Vector3 End = new Vector3(x, y, bounds.max.z);
			// check forward
			int hitNumber = 0;
			RaycastHit hit = new RaycastHit();
			while (Physics.Linecast(Start, End, out hit) && hitNumber < 100)
			{
				hitNumber++;
				Vector3 dir = (End - Start).normalized;
				Start = hit.point + dir * .01f;
			}
			if (hitNumber == 100)
				Debug.LogError("hit 100!");
			// check back
			Start = new Vector3(x, y, bounds.max.z);
			End = new Vector3(x, y, z);
			// check forward
			hit = new RaycastHit();
			while (Physics.Linecast(Start, End, out hit) && hitNumber < 100)
			{
				hitNumber++;
				Vector3 dir = (End - Start).normalized;
				Start = hit.point + dir * .01f;
			}
			//Debug.Log(hitNumber);
			bool destroyCube = true;
			if (hitNumber % 2 == 0)
			{
				destroyCube = true;
			}
			else
				destroyCube = false;
			return destroyCube;
		}
		static void SetInteriorVoxels(Puppet2D_Voxel[,,] voxels, Bounds bounds, Collider col, Vector3[] verts, float x, float y, int resolution, List<Puppet2D_Voxel> voxelList)
		{
			//float increment = bounds.size.z / 1000f;

			//float frac = GetVoxelSize(new Vector3(bounds.size.x, bounds.size.y, bounds.size.z), resolution);
			//Debug.Log("x " + x + " Y " + y);

			Vector3 Start = new Vector3(x, y, bounds.min.z - 10f);
			Vector3 End = new Vector3(x, y, bounds.max.z + 10f);
			// check forward
			Ray newRay = new Ray(Start, End - Start);
			RaycastHit[] hit = Physics.RaycastAll(newRay, 999999f);
			if (hit.Length>0)
			{
				Puppet2D_Voxel voxel = GetVoxel(voxels, bounds, hit[0].point, resolution);
				if (voxel != null)
				{
					voxel.Inside = true;
					voxelList.Add(voxel);
				}


			}
			/*

			// check back
			Start = new Vector3(x, y, bounds.max.z + 10f);
			End = new Vector3(x, y, bounds.min.z - 10f);
			// check forward

			hit = new RaycastHit();
			while (Physics.Linecast(Start, End, out hit))
			{
				Vector3 dir = (End - Start).normalized;
				Start = hit.point + dir * 0.001f;
				backHitIndexes.Add(hit.point);
			}
			//Debug.Log("forwardHitIndexes " + forwardHitIndexes.Count + " backHitIndexes " + backHitIndexes.Count);
			int j = forwardHitIndexes.Count - 1;

			for (int i = 0; i < forwardHitIndexes.Count; i++)
			{
				if (j < backHitIndexes.Count)
					FillLine3D(voxels, bounds, forwardHitIndexes[i] + Vector3.back * frac, backHitIndexes[j] + Vector3.forward * frac, resolution, voxelList);

				j--;
			}
			*/

		}
		public static List<Puppet2D_Voxel> FillLine3D(Puppet2D_Voxel[,,] voxels, Bounds bounds, Vector3 p1, Vector3 p2, int resolution, List<Puppet2D_Voxel> voxelList)
		{
			List<Puppet2D_Voxel> retList = new List<Puppet2D_Voxel>();

			//float frac = Vector3.Distance(p1,p2)/1000f;

			Puppet2D_Voxel voxel = GetVoxel(voxels, bounds, p1, resolution);
			if (voxel != null)
			{
				voxel.Inside = true;
				voxelList.Add(voxel);
			}

			Puppet2D_Voxel voxelEnd = GetVoxel(voxels, bounds, p2, resolution);





			while (voxel != null && voxel != voxelEnd)
			{
				voxel = voxels[(int)voxel.NeighbourIndexes[5].x, (int)voxel.NeighbourIndexes[5].y, (int)voxel.NeighbourIndexes[5].z];
				if (voxel != null)
				{
					voxel.Inside = true;
					voxelList.Add(voxel);
				}


			}

			/*Vector3 pos = p1;
			//float frac = ((bounds.size.x+ bounds.size.y + bounds.size.z)/3f)/resolution;
			float frac = GetVoxelSize(new Vector3(bounds.size.x, bounds.size.y, bounds.size.z), resolution);

			float ctr = 0;
			//Color col = new Color(Random.Range(0f,1f), Random.Range(0f,1f),Random.Range(0f,1f));
			while (Vector3.Distance(pos, p2) > 0.001f)
			{
				pos = Vector3.Lerp(p1, p2, ctr);
				ctr += frac;
				Voxel voxel = GetVoxel(voxels, bounds, pos, resolution);
				if (voxel != null)
				{
					voxel.Inside = true;
					voxelList.Add(voxel);
				}
			}*/
			return retList;

		}
		public static List<Puppet2D_Voxel> DrawLine3D(Puppet2D_Voxel[,,] voxels, Bounds bounds, Vector3 p1, Vector3 p2, int BoneIndex, int resolution)
		{
			//Debug.Log("here");
			List<Puppet2D_Voxel> retList = new List<Puppet2D_Voxel>();
			Vector3 pos = p1;
			//float frac = ((bounds.size.x+ bounds.size.y + bounds.size.z)/3f)/resolution;
			float frac = GetVoxelSize(new Vector3(bounds.size.x, bounds.size.y, bounds.size.z), resolution);

			float ctr = 0;
			//Color col = new Color(Random.Range(0f,1f), Random.Range(0f,1f),Random.Range(0f,1f));
			while (Vector3.Distance(pos, p2) > 0.001f)
			{
				pos = Vector3.Lerp(p1, p2, ctr);
				ctr += frac/100;
				Puppet2D_Voxel voxel = GetVoxel(voxels, bounds, pos, resolution);
				if (voxel != null)
				{
					voxel.Weight0 = 1f;
					voxel.Bone0 = BoneIndex;
					//voxel.GetComponent<Renderer>().material.color = col;
					retList.Add(voxel);
				}
			}
			return retList;

		}
		static float GetVoxelSize(Vector3 size, int resolution)
		{
			return Mathf.Pow(((size.x * size.y ) / resolution), 1f / 2f);
		}
		static Puppet2D_Voxel[,,] GenerateVoxels(Bounds bounds, Collider col, Vector3[] verts, int resolution, out List<Puppet2D_Voxel> voxelList)
		{

			voxelList = new List<Puppet2D_Voxel>();

			/*float sizeY = (bounds.size.x+ bounds.size.y + bounds.size.z)/3f;
			float voxelSize = sizeY/resolution;*/
			float voxelSize = GetVoxelSize(new Vector3(bounds.size.x, bounds.size.y, bounds.size.z), resolution);


			int width = (int)((bounds.max.x - bounds.min.x) / voxelSize) + 2;
			int height = (int)((bounds.max.y - bounds.min.y) / voxelSize) + 2;
			int depth = (int)((bounds.max.z - bounds.min.z) / voxelSize) + 2;
			//Debug.Log(width + " " + height + " " + depth);

			Puppet2D_Voxel[,,] voxels = new Puppet2D_Voxel[width + 1, height + 1, depth + 1];

			float x, y, z;
			EditorUtility.DisplayProgressBar("Generating Voxels for " + col.gameObject.name, "", 0);
			float total = (float)width * height * depth;
			int count = 0;
			int stepToShow = (int)(total / 100f);
			float halfVoxel = voxelSize * .5f;

			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{

					for (int k = 0; k < depth; k++)
					{
						if (count % stepToShow == 0)
							EditorUtility.DisplayProgressBar("Generating Voxels for " + col.gameObject.name, (count + " out of " + total), (float)count / total);
						count++;
						x = bounds.min.x + voxelSize * i;
						y = bounds.min.y + voxelSize * j;
						z = bounds.min.z + voxelSize * k;


						voxels[i, j, k] = new Puppet2D_Voxel();
						voxels[i, j, k].NeighbourIndexes = new Vector3[6];
						voxels[i, j, k].NeighbourIndexes[0] = i > 0 ? new Vector3(i - 1, j, k) : new Vector3(0, j, k);
						voxels[i, j, k].NeighbourIndexes[1] = i < width ? new Vector3(i + 1, j, k) : new Vector3(width - 1, j, k);
						voxels[i, j, k].NeighbourIndexes[2] = j > 0 ? new Vector3(i, j - 1, k) : new Vector3(i, 0, k);
						voxels[i, j, k].NeighbourIndexes[3] = j < height ? new Vector3(i, j + 1, k) : new Vector3(i, height - 1, k);
						voxels[i, j, k].NeighbourIndexes[4] = k > 0 ? new Vector3(i, j, k - 1) : new Vector3(i, j, 0);
						voxels[i, j, k].NeighbourIndexes[5] = k < depth ? new Vector3(i, j, k + 1) : new Vector3(i, j, depth - 1);
						voxels[i, j, k].Inside = false;


						voxels[i, j, k].Pos = new Vector3(x, y, z);
						voxels[i, j, k].Scale = new Vector3(voxelSize, voxelSize, voxelSize);


					}

				}

			}
			count = 0;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					if (count % stepToShow == 0)
						EditorUtility.DisplayProgressBar("Generating Voxels for " + col.gameObject.name, (count + " out of " + total), (float)count / total);
					count++;
					x = bounds.min.x + voxelSize * i;
					y = bounds.min.y + voxelSize * j;

					SetInteriorVoxels(voxels, bounds, col, verts, x + halfVoxel, y + halfVoxel, resolution, voxelList);

				}

			}
			EditorUtility.ClearProgressBar();
			return voxels;
		}

		/*static private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
		{
			Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
			Color[] rpixels = result.GetPixels(0);
			float incX = (1.0f / (float)targetWidth);
			float incY = (1.0f / (float)targetHeight);
			for (int px = 0; px < rpixels.Length; px++)
			{
				rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
			}
			result.SetPixels(rpixels, 0);
			result.Apply();
			return result;
		}*/
		static void GeosedicSkinWeights(GameObject mesh, List<Transform> selectedBones)
		{
			//Debug.Log(mesh.name);
			int resolution = (int)Puppet2D_Editor.VoxelScale * 50;
			Bounds bounds;
			string sortingLayer;
			int sortingOrder;
			SkinnedMeshRenderer renderer;
			Mesh sharedMesh;
			Vector3[] verts;
			Matrix4x4[] bindPoses;
			BoneWeight[] weights;
			Material mat;

			List<Transform> reducedBones = new List<Transform>();
			Bounds meshBounds = mesh.GetComponent<Renderer>().bounds;
			meshBounds.SetMinMax(meshBounds.min - Vector3.forward*9999f, meshBounds.max + Vector3.forward *9999f);
			foreach (Transform selectedBone in selectedBones)
			{
				if (meshBounds.Contains(selectedBone.position))
				{
					//Debug.Log(selectedBone);
					reducedBones.Add(selectedBone);
				}
			}
			selectedBones = reducedBones;
			
			SetupSkinnedMesh(mesh, selectedBones, out bounds, out sortingLayer, out sortingOrder, out renderer, out sharedMesh, out verts, out bindPoses, out weights, out mat);
			//Debug.Log(bounds.center + " CENTRE " + mesh.name);



			MeshCollider col = mesh.GetComponent<MeshCollider>();
			if (col == null)
			{
				col = mesh.AddComponent<MeshCollider>();
				col.sharedMesh = sharedMesh;
			}


			List<Puppet2D_Voxel> voxelList = new List<Puppet2D_Voxel>();
			Puppet2D_Voxel[,,] voxels = GenerateVoxels(bounds, col, verts, resolution, out voxelList);

			Queue<Puppet2D_Voxel> voxelsQueue = new Queue<Puppet2D_Voxel>();

			EditorUtility.DisplayProgressBar("Queuing Voxels", "", 0);
			for (int boneIndex = 0; boneIndex < selectedBones.Count; boneIndex++)
			{
				Vector3 startPos3D = selectedBones[boneIndex].transform.position;
				//voxelsQueue.Enqueue(GetVoxel(voxels, bounds, startPos3D, resolution));
				if (selectedBones[boneIndex].childCount > 0)
				{
					for (int childIndex = 0; childIndex < selectedBones[boneIndex].childCount; childIndex++)
					{
						if (!selectedBones[boneIndex].GetChild(childIndex).GetComponent<Puppet2D_HiddenBone>())
						{

							List<Puppet2D_Voxel> voxelLine = DrawLine3D(voxels, bounds, startPos3D, selectedBones[boneIndex].GetChild(childIndex).position, boneIndex, resolution);
							foreach (Puppet2D_Voxel v in voxelLine)
							{
								voxelsQueue.Enqueue(v);
							}
						}
					}
				}
				else
				{
					// Draw Bone Point
					Puppet2D_Voxel voxel = GetVoxel(voxels, bounds, selectedBones[boneIndex].position, resolution);
					if (voxel != null)
					{
						voxel.Weight0 = 1f;
						voxel.Bone0 = boneIndex;
						voxelsQueue.Enqueue(voxel);
					}
				}
			}
			EditorUtility.ClearProgressBar();

			EditorUtility.DisplayProgressBar("Setting Voxel weights", "", 0);

			Queue<Puppet2D_Voxel> outsideVoxelQueue = new Queue<Puppet2D_Voxel>();

			bool atLeastOneBoneInsideMesh = false;
			// ITERATE THROUGH VOXELS SETTING ALL BONE WEIGHTS
			while (voxelsQueue.Count > 0)
			{
				Puppet2D_Voxel voxel = voxelsQueue.Dequeue();

				foreach (Vector3 neighbourIndex in voxel.NeighbourIndexes)
				{
					Puppet2D_Voxel neighbour = voxels[(int)neighbourIndex.x, (int)neighbourIndex.y, (int)neighbourIndex.z];
					if (neighbour != null && neighbour.Inside && neighbour.Bone0 == -1)
					{
						neighbour.Bone0 = voxel.Bone0;
						neighbour.Weight0 = 1f;
						atLeastOneBoneInsideMesh = true;
						//neighbour.GetComponent<Renderer>().material = voxel.GetComponent<Renderer>().sharedMaterial;
						voxelsQueue.Enqueue(neighbour);
					}
					/*else if (neighbour != null && !neighbour.Inside)
					{
						neighbour.Bone0 = voxel.Bone0;
						neighbour.Weight0 = 1f;
						//outsideVoxelQueue.Enqueue(neighbour);
					}*/
				}
			}
			EditorUtility.ClearProgressBar();

			// BLUR WEIGHTS

			EditorUtility.DisplayProgressBar("Blur Voxel Weights", "", 0);
			//int blurIterations =6;
			float blurMult = Puppet2D_Editor.VoxelScale / 1000f;
			if (blurMult < 1f)
				blurMult = 1f;
			//Debug.Log(blurMult);
			for (int iter = 0; iter < Puppet2D_Editor.BlurIter * blurMult; iter++)
			{
				foreach (Puppet2D_Voxel voxel in voxelList)
				{
					float total = 1f;
					float totalWeight1 = 0;
					if (voxel.Bone1 != -1)
						totalWeight1 += voxel.Weight1;
					foreach (Vector3 neighbourIndex in voxel.NeighbourIndexes)
					{
						Puppet2D_Voxel neighbour = voxels[(int)neighbourIndex.x, (int)neighbourIndex.y, (int)neighbourIndex.z];
						if (neighbour != null && neighbour.Inside && neighbour.Bone0 != -1)
						{
							if (voxel.Bone1 == -1)
							{
								if (voxel.Bone0 != neighbour.Bone0)
								{
									voxel.Bone1 = neighbour.Bone0;
									totalWeight1 += neighbour.Weight0;
									total++;
								}
								else
								{
									voxel.Bone1 = neighbour.Bone1;
									totalWeight1 += neighbour.Weight1;
									total++;
								}
							}
							else if (voxel.Bone1 == neighbour.Bone0)
							{
								total++;
								totalWeight1 += neighbour.Weight0;
							}
							else if (voxel.Bone1 == neighbour.Bone1)
							{
								total++;
								totalWeight1 += neighbour.Weight1;
							}


						}
					}

					voxel.Weight1 = (totalWeight1 / total);
					voxel.Weight0 = 1f - voxel.Weight1;

				}
			}
			if (atLeastOneBoneInsideMesh)
			{
				//Debug.Log("atLeastOneBoneInsideMesh");return;
				foreach (Puppet2D_Voxel voxel in voxelList)
				{
					foreach (Vector3 neighbourIndex in voxel.NeighbourIndexes)
					{
						Puppet2D_Voxel neighbour = voxels[(int)neighbourIndex.x, (int)neighbourIndex.y, (int)neighbourIndex.z];
						if (neighbour != null && !neighbour.Inside)
						{
							neighbour.Bone0 = voxel.Bone0;
							neighbour.Weight0 = voxel.Weight0;
							neighbour.Bone1 = voxel.Bone1;
							neighbour.Weight1 = voxel.Weight1;

							outsideVoxelQueue.Enqueue(neighbour);
						}
					}
				}
			}

			EditorUtility.ClearProgressBar();

			// ITERATE THROUGH ALL OUTSIDE VOXELS

			EditorUtility.DisplayProgressBar("Setting Outside Voxel Weights", "", 0);

			while (outsideVoxelQueue.Count > 0)
			{
				Puppet2D_Voxel voxel = outsideVoxelQueue.Dequeue();
				// If theres a weight next to it get it
				/*foreach (Vector3 neighbourIndex in voxel.NeighbourIndexes)
				{
					Voxel neighbour = voxels[(int)neighbourIndex.x, (int)neighbourIndex.y, (int)neighbourIndex.z];

					if (neighbour != null && neighbour.Inside )
					{
						voxel.Bone0 = neighbour.Bone0;
						voxel.Weight0 = neighbour.Weight0;
						voxel.Bone1 = neighbour.Bone1;
						voxel.Weight1 = neighbour.Weight1;
						break;
					}

				}*/

				foreach (Vector3 neighbourIndex in voxel.NeighbourIndexes)
				{
					Puppet2D_Voxel neighbour = voxels[(int)neighbourIndex.x, (int)neighbourIndex.y, (int)neighbourIndex.z];

					if (neighbour != null && !neighbour.Inside && neighbour.Bone0 == -1)
					{
						neighbour.Bone0 = voxel.Bone0;
						neighbour.Weight0 = voxel.Weight0;
						neighbour.Bone1 = voxel.Bone1;
						neighbour.Weight1 = voxel.Weight1;
						outsideVoxelQueue.Enqueue(neighbour);
					}

				}
			}
			EditorUtility.ClearProgressBar();


			EditorUtility.DisplayProgressBar("copying Voxels to mesh", "", 0);
			//List<Object> gos = new List<Object>();
			voxelsQueue.Clear();
			for (int i = 0; i < verts.Length; i++)
			{
				weights[i].boneIndex0 = 0;
				weights[i].boneIndex1 = 0;
				weights[i].boneIndex2 = 0;
				weights[i].boneIndex3 = 0;
				weights[i].weight0 = 1;
				weights[i].weight1 = 0;
				weights[i].weight2 = 0;
				weights[i].weight3 = 0;

				Vector3 vert = mesh.transform.TransformPoint(verts[i]);
				Puppet2D_Voxel v = GetVoxel(voxels, bounds, vert, resolution);
				List<Puppet2D_Voxel> UsedVoxels = new List<Puppet2D_Voxel>();
				if (v != null)
				{

					if (v.Bone0 == -1)
					{


						voxelsQueue.Enqueue(v);
						int searchDepth = 0;
						while (voxelsQueue.Count > 0 && searchDepth < 200)
						{
							Puppet2D_Voxel voxel = voxelsQueue.Dequeue();

							foreach (Vector3 neighbourIndex in voxel.NeighbourIndexes)
							{
								Puppet2D_Voxel neighbour = voxels[(int)neighbourIndex.x, (int)neighbourIndex.y, (int)neighbourIndex.z];
								if (neighbour != null && !UsedVoxels.Contains(neighbour))
								{
									if (neighbour.Inside && neighbour.Bone0 != -1)
									{
										v = neighbour;
										voxelsQueue.Clear();
										break;
									}
									else
									{
										UsedVoxels.Add(neighbour);
										voxelsQueue.Enqueue(neighbour);
									}

								}

							}
							searchDepth++;
						}

					}


				}


				if (v != null)
				{
					if (v.Bone0 > -1)
					{
						weights[i].boneIndex0 = v.Bone0;
						weights[i].weight0 = v.Weight0;
					}
					if (v.Bone1 > -1)
					{
						weights[i].boneIndex1 = v.Bone1;
						weights[i].weight1 = v.Weight1;

					}
				}

			}

			EditorUtility.ClearProgressBar();
			GameObject VoxelsGO = null;

			GameObject VoxelsGOParent = null;
			if (Puppet2D_Editor.KeepVoxels)
			{
				VoxelsGOParent = new GameObject();
				VoxelsGOParent.name = "Voxels";
				Undo.RegisterCreatedObjectUndo(VoxelsGOParent, "voxel");
			}

			MeshFilter voxMF = null;
			Mesh voxMesh = null;
			List<Vector3> vertList = new List<Vector3>();
			List<int> triList = new List<int>();
			List<Color> colorList = new List<Color>();
			int indexVoxel = 0;
			CreateNewVoxelModel(ref VoxelsGOParent, ref VoxelsGO, ref voxMF, ref voxMesh, ref indexVoxel);

			// Make Proxy Cube
			GameObject cubeTemp = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Mesh cubeMesh = cubeTemp.GetComponent<MeshFilter>().sharedMesh;

			DestroyImmediate(cubeTemp);

			EditorUtility.DisplayProgressBar("Creating Voxel models", "", 0);

			int index = 0;
			foreach (Puppet2D_Voxel vox in voxels)
			{
				if (vertList.Count + cubeMesh.vertexCount > 65000)
				{
					CompleteVoxelModel(voxMF, voxMesh, vertList, triList, colorList);
					indexVoxel++;
					index = 0;
					CreateNewVoxelModel(ref VoxelsGOParent, ref VoxelsGO, ref voxMF, ref voxMesh, ref indexVoxel);

				}
				if (Puppet2D_Editor.KeepVoxels)
				{
					if (vox != null && vox.Inside)
					{
						float hue = ((float)vox.Bone0 / selectedBones.Count);
						float hue2 = ((float)vox.Bone1 / selectedBones.Count);
						Color Color1 = (Color.HSVToRGB(hue * 1f, 1f, 1f));//+  HsvToRgb(hue2*255f, 255, 255)*boneWeights[i].weight1 );
						Color Color2 = (Color.HSVToRGB(hue2 * 1f, 1f, 1f));//+  HsvToRgb(hue2*255f, 255, 255)*boneWeights[i].weight1 );	

						/*Mesh newMesh = new Mesh();
						newMesh.vertices = cubeMesh.vertices;
						newMesh.triangles = cubeMesh.triangles;
						Color[] cols = new Color[newMesh.vertices.Length];
						for (int c = 0; c < cols.Length; c++)
						{

							cols[c] = Color.Lerp(Color2, Color1, vox.Weight0) ;
						}

						newMesh.colors = cols;

						vox.gameObject.AddComponent<MeshFilter>().sharedMesh = newMesh;
						MeshRenderer cubeMR = vox.gameObject.AddComponent<MeshRenderer>();
						cubeMR.sharedMaterial = new Material(Shader.Find("Puppet3D/vertColor"));
						*/
						for (int i = 0; i < cubeMesh.vertexCount; i++)
						{
							Vector3 newPos = vox.Pos + vox.Scale.x * cubeMesh.vertices[i];
							vertList.Add(newPos);
							colorList.Add(Color.Lerp(Color2, Color1, vox.Weight0));
						}
						for (int i = 0; i < cubeMesh.triangles.Length; i++)
						{
							triList.Add(index + cubeMesh.triangles[i]);
						}

						index += 24;
					}
				}


			}
			CompleteVoxelModel(voxMF, voxMesh, vertList, triList, colorList);
			EditorUtility.ClearProgressBar();

			renderer.quality = SkinQuality.Bone2;

			FinishSkinnedMesh(mesh, selectedBones, bounds, sortingLayer, sortingOrder, renderer, sharedMesh, bindPoses, weights, mat);


			renderer.sharedMaterial = mat;


			return;

		}

		private static void CompleteVoxelModel(MeshFilter voxMF, Mesh voxMesh, List<Vector3> vertList, List<int> triList, List<Color> colorList)
		{
			if (Puppet2D_Editor.KeepVoxels)
			{
				voxMesh.vertices = vertList.ToArray();
				voxMesh.colors = colorList.ToArray();
				voxMesh.triangles = triList.ToArray();
				voxMF.sharedMesh = voxMesh;
				vertList.Clear();
				colorList.Clear();
				triList.Clear();
			}
		}

		private static void CreateNewVoxelModel(ref GameObject VoxelsGOParent, ref GameObject VoxelsGO, ref MeshFilter voxMF, ref Mesh voxMesh, ref int index)
		{
			if (Puppet2D_Editor.KeepVoxels)
			{
				VoxelsGO = new GameObject();
				VoxelsGO.transform.parent = VoxelsGOParent.transform;
				Undo.RegisterCreatedObjectUndo(VoxelsGO, "voxel");
				VoxelsGO.name = "Voxels";
				voxMF = VoxelsGO.AddComponent<MeshFilter>();
				voxMesh = new Mesh();
				voxMesh.name = index.ToString();
				MeshRenderer cubeMR = VoxelsGO.AddComponent<MeshRenderer>();
				cubeMR.sharedMaterial = new Material(Shader.Find("Puppet2D/vertColor"));

			}
		}

		static int[] GetFirstSecondBone(int vertIndex, float[,] weights)
		{
			float weightCheck = 9999999f;
			int[] boneReturn = new int[2];
			boneReturn[0] = -1;
			boneReturn[1] = -1;

			for (int i = 0; i < weights.GetLength(0); i++)
			{

				//if (weights[i, vertIndex]>0 && weightCheck > weights[i, vertIndex] )
				if (weights[i, vertIndex] >= 0 && weightCheck >= weights[i, vertIndex])
				{
					weightCheck = weights[i, vertIndex];
					boneReturn[0] = i;

				}

			}
			float weightCheck2 = 999999f;
			for (int i = 0; i < weights.GetLength(0); i++)
			{
				if (boneReturn[0] != i)
				{
					// if (weights[i, vertIndex]>0 &&  weights[i, vertIndex] < weightCheck2)
					if (weights[i, vertIndex] >= 0 && weightCheck2 >= weights[i, vertIndex])
					{
						weightCheck2 = weights[i, vertIndex];
						boneReturn[1] = i;

					}
				}

			}
			//        Debug.Log(boneReturn[0] + " : " + boneReturn[1]);
			if (boneReturn[0] == -1)
			{
				boneReturn[0] = 0;

				Debug.LogError("Your image might have some rough edges - this has caused a few errors in the default skinning");

			}
			if (boneReturn[1] == -1)
				boneReturn[1] = 0;

			return boneReturn;
		}

		public static List<Vector2> DrawLine(Vector2 p1, Vector2 p2)
		{
			List<Vector2> retList = new List<Vector2>();
			Vector2 t = p1;
			float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
			float ctr = 0;

			while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
			{
				t = Vector2.Lerp(p1, p2, ctr);
				ctr += frac;
				retList.Add(new Vector2(t.x, t.y));
				//Debug.Log(t.x + " " + t.y);
			}
			return retList;
		}

		static Vector2 WorldPosToXY(Vector3 worldPos, Bounds bounds, int arrayWidth, int arrayHeight)
		{

			Vector3 localPos = worldPos;
			localPos.x = (localPos.x - bounds.min.x) / bounds.size.x;
			localPos.y = (localPos.y - bounds.min.y) / bounds.size.y;

			float x = localPos.x * arrayWidth;
			float y = localPos.y * arrayHeight; ;

			return new Vector2((int)x, (int)y);
		}
		static int WorldPosToIndex(Vector3 worldPos, Bounds bounds, int arrayWidth, int arrayHeight)
		{

			Vector3 localPos = worldPos;
			localPos.x = (localPos.x - bounds.min.x) / bounds.size.x;
			localPos.y = (localPos.y - bounds.min.y) / bounds.size.y;

			float x = localPos.x * arrayWidth;
			float y = localPos.y * arrayHeight; ;


			int index = (int)(y) * arrayWidth + (int)x;

			//Debug.Log("world pos = " + worldPos);
			//Debug.Log("localPos pos = " + localPos);
			//Debug.Log("x  = " + x + " y  = " + y);
			//Debug.Log("index  = " + index);


			return index;
		}
		public static void SetTextureImporterFormat(Texture2D texture, bool isReadable)
		{
			if (null == texture) return;

			string assetPath = AssetDatabase.GetAssetPath(texture);
			var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
			if (tImporter != null)
			{
				//tImporter.textureType = TextureImporterType.Advanced;

				tImporter.isReadable = isReadable;

				AssetDatabase.ImportAsset(assetPath);
				AssetDatabase.Refresh();
			}
		}
		public static void SortVertices(GameObject gameObject = null)
		{
			if (gameObject == null)
				gameObject = Selection.activeGameObject;
			if (Selection.activeGameObject == null)
				return;
			SkinnedMeshRenderer smr = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();
			if (smr == null)
				return;
			Mesh mesh = smr.sharedMesh;
			SpriteRenderer[] bones = new SpriteRenderer[smr.bones.Length];
			for (int c = 0; c < bones.Length; c++)
				bones[c] = smr.bones[c].GetComponent<SpriteRenderer>();

			int[] tris = mesh.triangles;

			tris = BubbleSort(tris, bones, mesh);


			mesh.triangles = tris;


			Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
		}
		public static int[] BubbleSort(int[] tris, SpriteRenderer[] bones, Mesh mesh)
		{

			int i, j;
			int N = tris.Length / 3;
			for (j = N - 1; j > 0; j--)
			{
				for (i = 0; i < j; i++)
				{
					if (bones[mesh.boneWeights[tris[i * 3]].boneIndex0].sortingOrder > bones[mesh.boneWeights[tris[((i * 3) + 3)]].boneIndex0].sortingOrder)
					{
						tris = exchange(tris, i * 3, ((i * 3) + 3));
					}
				}
			}

			return tris;
		}
		public static int[] exchange(int[] data, int m, int n)
		{
			// Debug.Log("swapping " + m + " for " + n);

			int temporary1 = data[m];
			int temporary2 = data[m + 1];
			int temporary3 = data[m + 2];

			data[m] = data[n];
			data[m + 1] = data[n + 1];
			data[m + 2] = data[n + 2];

			data[n] = temporary1;
			data[n + 1] = temporary2;
			data[n + 2] = temporary3;

			return data;
		}
		static void EuclidianSkinWeights(GameObject mesh, List<Transform> selectedBones, List<GameObject> ffdControls)
		{
			Bounds SortingLayerBounds;
			string sortingLayer;
			int sortingOrder;
			SkinnedMeshRenderer renderer;
			Mesh sharedMesh;
			Vector3[] verts;
			Matrix4x4[] bindPoses;
			BoneWeight[] weights;
			Material mat;

			SetupSkinnedMesh(mesh, selectedBones, out SortingLayerBounds, out sortingLayer, out sortingOrder, out renderer, out sharedMesh, out verts, out bindPoses, out weights, out mat);

			int index = 0;
			int index2 = 0;
			int index3 = 0;
			for (int j = 0; j < weights.Length; j++)
			{
				float testdist = 1000000;
				float testdist2 = 1000000;
				for (int i = 0; i < selectedBones.Count; i++)
				{
					Vector3 worldPt = mesh.transform.TransformPoint(verts[j]);
					float dist = Vector2.Distance(new Vector2(selectedBones[i].GetComponent<Renderer>().bounds.center.x, selectedBones[i].GetComponent<Renderer>().bounds.center.y), new Vector2(worldPt.x, worldPt.y));
					if (dist < testdist)
					{
						testdist = dist;
						index = selectedBones.IndexOf(selectedBones[i]);
					}
					//Transform bone = selectedBones [i];
					//bindPoses [i] = bone.worldToLocalMatrix * mesh.transform.localToWorldMatrix;
				}
				for (int i = 0; i < selectedBones.Count; i++)
				{
					if (!(index == (selectedBones.IndexOf(selectedBones[i]))))
					{
						Vector3 worldPt = mesh.transform.TransformPoint(verts[j]);
						float dist = Vector2.Distance(new Vector2(selectedBones[i].GetComponent<Renderer>().bounds.center.x, selectedBones[i].GetComponent<Renderer>().bounds.center.y), new Vector2(worldPt.x, worldPt.y));
						if (dist < testdist2)
						{
							testdist2 = dist;
							index2 = selectedBones.IndexOf(selectedBones[i]);
						}
					}
				}
				float combinedDistance = testdist + testdist2;
				float weight1 = (testdist / combinedDistance);
				float weight2 = (testdist2 / combinedDistance);
				weight1 = Mathf.Lerp(1, 0, weight1);
				weight2 = Mathf.Lerp(1, 0, weight2);
				weight1 = Mathf.Clamp01((weight1 + 0.5f) * (weight1 + 0.5f) * (weight1 + 0.5f) - 0.5f);
				weight2 = Mathf.Clamp01((weight2 + 0.5f) * (weight2 + 0.5f) * (weight2 + 0.5f) - 0.5f);
				if (Puppet2D_Editor._numberBonesToSkinToIndex == 1)
				{
					renderer.quality = SkinQuality.Bone2;
					weights[j].boneIndex0 = index;
					weights[j].weight0 = weight1;
					weights[j].boneIndex1 = index2;
					weights[j].weight1 = weight2;
				}
				else if (Puppet2D_Editor._numberBonesToSkinToIndex == 2)
				{
					Vector3 worldPt = mesh.transform.TransformPoint(verts[j]);
					//Vector3 worldPt = verts[j];
					renderer.quality = SkinQuality.Bone4;
					if (ffdControls.Count == 0)
					{
						Debug.LogWarning("You must select some FFD controls to bind to");
						return;
					}
					if (ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh == null || ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh.sharedMesh == null)
					{
						Debug.LogWarning("You need the original FFD output mesh to copy skin weights. Make sure the outputSkinnedMesh is assigned to the ffdControl");
						return;
					}
					if (!ffdControls[0].transform.parent || !ffdControls[0].transform.parent.parent)
					{
						Debug.LogWarning("Your FFD Controls need a parent Group for offset");
						return;
					}
					int[] tris = ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh.sharedMesh.triangles;
					Vector3[] ffdMeshVerts = ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh.sharedMesh.vertices;
					bool insideTriangle = false;
					for (int t = 0; t < tris.Length - 2; t += 3)
					{
						Vector3[] polygon = new Vector3[3];
						polygon[0] = ffdControls[0].transform.parent.parent.TransformPoint(ffdMeshVerts[tris[t]]);
						polygon[1] = ffdControls[0].transform.parent.parent.TransformPoint(ffdMeshVerts[tris[t + 1]]);
						polygon[2] = ffdControls[0].transform.parent.parent.TransformPoint(ffdMeshVerts[tris[t + 2]]);
						//Debug.Log(worldPt+" "+polygon[0]+" "+polygon[1]+" "+polygon[2]);
						if (ContainsPoint(polygon, worldPt))
						{
							index = Puppet2D_FFD.GetIndexOfVector3(ffdControls, polygon[0]);
							index2 = Puppet2D_FFD.GetIndexOfVector3(ffdControls, polygon[1]);
							index3 = Puppet2D_FFD.GetIndexOfVector3(ffdControls, polygon[2]);
							insideTriangle = true;
						}
					}
					if (insideTriangle)
					{
						Vector3 weightBary = Barycentric(ffdControls[index].transform.position, ffdControls[index2].transform.position, ffdControls[index3].transform.position, worldPt);
						//Debug.Log(ffdControls[index] + " " + ffdControls[index2] + " " + ffdControls[index3]);
						//if (index != -1 && weights[j].weight0 > 0)
						{
							weights[j].boneIndex0 = index;
							weights[j].weight0 = weightBary.z;
							//if (index2 != -1 && weights[j].weight1 > 0)
							//if (index3 != -1 && weights[j].weight2 > 0)
						}
						{
							weights[j].boneIndex1 = index2;
							weights[j].weight1 = weightBary.x;
						}
						{
							weights[j].boneIndex2 = index3;
							weights[j].weight2 = weightBary.y;
						}
					}
					else
					{
						weights[j].boneIndex0 = 0;
						weights[j].weight0 = 1;
					}
				}
				else
				{
					renderer.quality = SkinQuality.Bone1;
					weights[j].boneIndex0 = index;
					weights[j].weight0 = 1;
				}
			}
			FinishSkinnedMesh(mesh, selectedBones, SortingLayerBounds, sortingLayer, sortingOrder, renderer, sharedMesh, bindPoses, weights, mat);

		}

		static void SetupSkinnedMesh(GameObject mesh, List<Transform> selectedBones, out Bounds SortingLayerBounds, out string sortingLayer, out int sortingOrder, out SkinnedMeshRenderer renderer, out Mesh sharedMesh, out Vector3[] verts, out Matrix4x4[] bindPoses, out BoneWeight[] weights, out Material mat)
		{
			mat = null;
			sortingLayer = "";
			sortingOrder = 0;
			if (mesh.GetComponent<MeshRenderer>() != null)
			{
				mat = mesh.GetComponent<MeshRenderer>().sharedMaterial;
				sortingLayer = mesh.GetComponent<Renderer>().sortingLayerName;
				sortingOrder = mesh.GetComponent<Renderer>().sortingOrder;
				DestroyImmediate(mesh.GetComponent<MeshRenderer>());
			}
			renderer = mesh.GetComponent<SkinnedMeshRenderer>();
			if (renderer == null)
			{
				renderer = mesh.AddComponent<SkinnedMeshRenderer>();
			}
			else
			{
				mat = renderer.sharedMaterial;

				DestroyImmediate(renderer);
				renderer = mesh.AddComponent<SkinnedMeshRenderer>();

			}
			renderer.updateWhenOffscreen = true;
			Puppet2D_SortingLayer puppet2D_SortingLayer = mesh.GetComponent<Puppet2D_SortingLayer>();
			if (puppet2D_SortingLayer != null)
			{
				SortingLayerBounds = puppet2D_SortingLayer.bounds;
				if (puppet2D_SortingLayer != null)
					DestroyImmediate(puppet2D_SortingLayer);
			}
			else
				SortingLayerBounds = new Bounds();

			

			sharedMesh = mesh.transform.GetComponent<MeshFilter>().sharedMesh;
			verts = sharedMesh.vertices;
			bindPoses = new Matrix4x4[selectedBones.Count];
			List<Transform> closestBones = new List<Transform>();
			closestBones.Clear();
			weights = new BoneWeight[verts.Length];
			for (int i = 0; i < selectedBones.Count; i++)
			{
				Transform bone = selectedBones[i];
				bindPoses[i] = bone.worldToLocalMatrix * mesh.transform.localToWorldMatrix;
			}

		}

		static void FinishSkinnedMesh(GameObject mesh, List<Transform> selectedBones, Bounds SortingLayerBounds, string sortingLayer, int sortingOrder, SkinnedMeshRenderer renderer, Mesh sharedMesh, Matrix4x4[] bindPoses, BoneWeight[] weights, Material mat)
		{
			sharedMesh.boneWeights = weights;
			sharedMesh.bindposes = bindPoses;
			renderer.bones = selectedBones.ToArray();
			if (sharedMesh.colors.Length == 0)
			{
				Color[] newColors = new Color[sharedMesh.vertices.Length];
				for (int i = 0; i < sharedMesh.vertices.Length; i++)
				{
					newColors[i] = new Color(1f, 1f, 1f, 1f);
				}
				sharedMesh.colors = newColors;
			}
			renderer.sharedMesh = sharedMesh;
			if (mat)
				renderer.sharedMaterial = mat;
			renderer.sortingLayerName = sortingLayer;
			renderer.sortingOrder = sortingOrder;
			Puppet2D_SortingLayer Puppet2D_SortingLayerNew = mesh.AddComponent<Puppet2D_SortingLayer>();
			Puppet2D_SortingLayerNew.bounds = SortingLayerBounds;
			EditorUtility.SetDirty(mesh);
			EditorUtility.SetDirty(sharedMesh);
			AssetDatabase.SaveAssets();
			AssetDatabase.SaveAssets();
		}

		[MenuItem("GameObject/Puppet2D/Skin/Edit Skin Weights")]
		public static bool EditWeights()
		{
			GameObject[] selection = Selection.gameObjects;

			foreach (GameObject sel in selection)
			{
				if ((sel.GetComponent<Puppet2D_Bakedmesh>() != null))
				{
					Debug.LogWarning("Already in edit mode");
					return false;
				}
				if ((sel.GetComponent<SkinnedMeshRenderer>()))
				{
					SkinnedMeshRenderer renderer = sel.GetComponent<SkinnedMeshRenderer>();
					Undo.RecordObject(sel, "add mesh to meshes being editted");
					Undo.AddComponent<Puppet2D_Bakedmesh>(sel);
					Mesh mesh = sel.GetComponent<MeshFilter>().sharedMesh;


					Vector3[] verts = mesh.vertices;
					BoneWeight[] boneWeights = mesh.boneWeights;

					for (int i = 0; i < verts.Length; i++)
					{
						Vector3 vert = verts[i];
						Vector3 vertPos = sel.transform.TransformPoint(vert);
						GameObject handle = new GameObject("vertex" + i);
						Undo.RegisterCreatedObjectUndo(handle, "vertex created");
						handle.transform.position = vertPos;
						Undo.SetTransformParent(handle.transform, sel.transform, "parent handle");

						SpriteRenderer spriteRenderer = Undo.AddComponent<SpriteRenderer>(handle);
						string path = (Puppet2D_Editor._puppet2DPath + "/Textures/GUI/VertexHandle.psd");
						Sprite sprite = AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
						spriteRenderer.sprite = sprite;
						spriteRenderer.sortingLayerName = Puppet2D_Editor._controlSortingLayer;
						Puppet2D_EditSkinWeights editSkinWeights = Undo.AddComponent<Puppet2D_EditSkinWeights>(handle);

						editSkinWeights.verts = mesh.vertices;

						editSkinWeights.Weight0 = boneWeights[i].weight0;
						editSkinWeights.Weight1 = boneWeights[i].weight1;
						editSkinWeights.Weight2 = boneWeights[i].weight2;
						editSkinWeights.Weight3 = boneWeights[i].weight3;

						if (boneWeights[i].weight0 > 0)
						{
							editSkinWeights.Bone0 = renderer.bones[boneWeights[i].boneIndex0].gameObject;
							editSkinWeights.boneIndex0 = boneWeights[i].boneIndex0;
						}
						else
							editSkinWeights.Bone0 = null;

						if (boneWeights[i].weight1 > 0)
						{
							editSkinWeights.Bone1 = renderer.bones[boneWeights[i].boneIndex1].gameObject;
							editSkinWeights.boneIndex1 = boneWeights[i].boneIndex1;
						}
						else
						{
							editSkinWeights.Bone1 = null;
							editSkinWeights.boneIndex1 = renderer.bones.Length;
						}

						if (boneWeights[i].weight2 > 0)
						{
							editSkinWeights.Bone2 = renderer.bones[boneWeights[i].boneIndex2].gameObject;
							editSkinWeights.boneIndex2 = boneWeights[i].boneIndex2;
						}
						else
						{
							editSkinWeights.Bone2 = null;
							editSkinWeights.boneIndex2 = renderer.bones.Length;
						}

						if (boneWeights[i].weight3 > 0)
						{
							editSkinWeights.Bone3 = renderer.bones[boneWeights[i].boneIndex3].gameObject;
							editSkinWeights.boneIndex3 = boneWeights[i].boneIndex3;
						}
						else
						{
							editSkinWeights.Bone3 = null;
							editSkinWeights.boneIndex3 = renderer.bones.Length;
						}

						editSkinWeights.mesh = mesh;
						editSkinWeights.meshRenderer = renderer;
						editSkinWeights.vertNumber = i;
					}

				}
				else
				{
					Debug.LogWarning("Selection does not have a meshRenderer");
					return false;
				}


			}
			return true;
		}

		[MenuItem("GameObject/Puppet2D/Skin/Finish Editting Skin Weights")]
		public static Object[] FinishEditingWeights()
		{
			SpriteRenderer[] sprs = FindObjectsOfType<SpriteRenderer>();
			Puppet2D_Bakedmesh[] skinnedMeshesBeingEditted = FindObjectsOfType<Puppet2D_Bakedmesh>();
			List<Object> returnObjects = new List<Object>();
			foreach (SpriteRenderer spr in sprs)
			{
				if (spr.sprite)
					if (spr.sprite.name.Contains("Bone"))
						spr.gameObject.GetComponent<SpriteRenderer>().color = Color.white;

			}
			foreach (Puppet2D_Bakedmesh bakedMesh in skinnedMeshesBeingEditted)
			{
				GameObject sel = bakedMesh.gameObject;
				returnObjects.Add(sel);

				DestroyImmediate(bakedMesh);

				int numberChildren = sel.transform.childCount;
				List<GameObject> vertsToDestroy = new List<GameObject>();
				for (int i = 0; i < numberChildren; i++)
				{
					vertsToDestroy.Add(sel.transform.GetChild(i).gameObject);


				}
				foreach (GameObject vert in vertsToDestroy)
					DestroyImmediate(vert);
			}
			return returnObjects.ToArray();
		}

		static Mesh SmoothSkinWeights(Mesh sharedMesh)
		{
			//        Debug.Log("smoothing skin weights");
			int[] triangles = sharedMesh.GetTriangles(0);
			BoneWeight[] boneWeights = sharedMesh.boneWeights;

			for (int i = 0; i < triangles.Length; i += 3)
			{
				BoneWeight v1 = boneWeights[triangles[i]];
				BoneWeight v2 = boneWeights[triangles[i + 1]];
				BoneWeight v3 = boneWeights[triangles[i + 2]];

				List<int> v1Bones = new List<int>(new int[] { v1.boneIndex0, v1.boneIndex1, v1.boneIndex2, v1.boneIndex3 });
				List<int> v2Bones = new List<int>(new int[] { v2.boneIndex0, v2.boneIndex1, v2.boneIndex2, v2.boneIndex3 });
				List<int> v3Bones = new List<int>(new int[] { v3.boneIndex0, v3.boneIndex1, v3.boneIndex2, v3.boneIndex3 });

				List<float> v1Weights = new List<float>(new float[] { v1.weight0, v1.weight1, v1.weight2, v1.weight3 });
				List<float> v2Weights = new List<float>(new float[] { v2.weight0, v2.weight1, v2.weight2, v2.weight3 });
				List<float> v3Weights = new List<float>(new float[] { v3.weight0, v3.weight1, v3.weight2, v3.weight3 });


				for (int j = 0; j < 2; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						if (v1Bones[j] == v2Bones[k])
						{
							for (int l = 0; l < 2; l++)
							{
								if (v1Bones[j] == v3Bones[l])
								{

									v1Weights[j] = (v1Weights[j] + v2Weights[k] + v3Weights[l]) / 3;
									v2Weights[k] = (v1Weights[j] + v2Weights[k] + v3Weights[l]) / 3;
									v3Weights[l] = (v1Weights[j] + v2Weights[k] + v3Weights[l]) / 3;


								}
							}
						}
					}

				}
				boneWeights[triangles[i]].weight0 = v1Weights[0];
				boneWeights[triangles[i]].weight1 = v1Weights[1];


				boneWeights[triangles[i + 1]].weight0 = v2Weights[0];
				boneWeights[triangles[i + 1]].weight1 = v2Weights[1];


				boneWeights[triangles[i + 2]].weight0 = v3Weights[0];
				boneWeights[triangles[i + 2]].weight1 = v3Weights[1];


			}
			sharedMesh.boneWeights = boneWeights;
			return sharedMesh;
		}

		public static void DrawHandle(Vector3 mousepos)
		{

			Handles.DrawWireDisc(mousepos + Vector3.forward * 10, Vector3.back, Puppet2D_Editor.EditSkinWeightRadius);

			Handles.color = Puppet2D_Editor.paintControlColor;
			Handles.DrawSolidDisc(mousepos + Vector3.forward * 11, Vector3.back, Puppet2D_Editor.EditSkinWeightRadius * Puppet2D_Editor.paintWeightsStrength);
			SceneView.RepaintAll();

		}

		public static void PaintWeights(Vector3 mousepos, float weightStrength)
		{

			//		Debug.Log("Paint Weights");



			Vector3[] vertices = Puppet2D_Editor.currentSelectionMesh.vertices;
			Color[] colrs = Puppet2D_Editor.currentSelectionMesh.colors;
			BoneWeight[] boneWeights = Puppet2D_Editor.currentSelectionMesh.boneWeights;

			Vector3 pos = Puppet2D_Editor.currentSelection.transform.InverseTransformPoint(mousepos);
			Undo.RecordObject(Puppet2D_Editor.currentSelectionMesh, "Weight paint");
			pos = new Vector3(pos.x, pos.y, 0);

			SkinnedMeshRenderer smr = Puppet2D_Editor.currentSelection.GetComponent<SkinnedMeshRenderer>();
			int boneIndex = smr.bones.ToList().IndexOf(Puppet2D_Editor.paintWeightsBone.transform);

			if (boneIndex < 0)
			{
				Debug.LogWarning(Puppet2D_Editor.paintWeightsBone.name + " is not connected to skin");
				return;
			}
			for (int i = 0; i < vertices.Length; i++)
			{
				if (boneWeights[i].boneIndex0 < 0)
					boneWeights[i].boneIndex0 = 0;
				if (boneWeights[i].boneIndex1 < 0)
					boneWeights[i].boneIndex1 = 0;

				float sqrMagnitude = (vertices[i] - pos).magnitude;
				if (sqrMagnitude > Puppet2D_Editor.EditSkinWeightRadius)
					continue;
				float weightFloat = Puppet2D_Editor.paintWeightsStrength * Puppet2D_Editor.paintWeightsStrength * Puppet2D_Editor.paintWeightsStrength;

				//			Color weightColor = new Color (weightFloat, weightFloat, weightFloat, 1);
				//			if(weightStrength>0)
				//				if(colrs[i].r <=1)
				//					colrs[i] = colrs[i]+weightColor;//colrs[i] = Color.Lerp( colrs[i],Color.white, Puppet2D_Editor.paintWeightsStrength*Puppet2D_Editor.paintWeightsStrength);
				//			else
				//				if(colrs[i].r >=0)
				//					colrs[i] -= weightColor;//colrs[i] = Color.Lerp( colrs[i],Color.black,Puppet2D_Editor.paintWeightsStrength* Puppet2D_Editor.paintWeightsStrength);
				//			



				if (boneWeights[i].boneIndex0 == boneIndex)
				{
					if (weightStrength > 0)
						boneWeights[i].weight0 += weightFloat;
					else
						boneWeights[i].weight0 -= weightFloat;

					boneWeights[i].weight0 = Mathf.Clamp01(boneWeights[i].weight0);
					boneWeights[i].weight1 = 1 - boneWeights[i].weight0;
					colrs[i] = new Color(boneWeights[i].weight0, boneWeights[i].weight0, boneWeights[i].weight0, 1);
					//boneWeights[i].weight0 = colrs[i].r;
					//boneWeights[i].weight1 = 1-colrs[i].r;

				}
				else if (boneWeights[i].boneIndex1 == boneIndex)
				{
					if (weightStrength > 0)
						boneWeights[i].weight1 += weightFloat;
					else
						boneWeights[i].weight1 -= weightFloat;

					boneWeights[i].weight1 = Mathf.Clamp01(boneWeights[i].weight1);
					boneWeights[i].weight0 = 1 - boneWeights[i].weight1;
					colrs[i] = new Color(boneWeights[i].weight1, boneWeights[i].weight1, boneWeights[i].weight1, 1);

					//boneWeights[i].weight1 = colrs[i].r;
					//boneWeights[i].weight0 = 1-colrs[i].r;


				}
				else if (weightFloat != 0 || boneWeights[i].weight1 + boneWeights[i].weight2 + boneWeights[i].weight3 > 0)
				{
					if (boneWeights[i].weight0 < boneWeights[i].weight1)
					{

						if (weightStrength > 0)
							boneWeights[i].weight0 += weightFloat;
						else
							boneWeights[i].weight0 -= weightFloat;

						boneWeights[i].weight0 = Mathf.Clamp01(boneWeights[i].weight0);
						boneWeights[i].weight1 = 1 - boneWeights[i].weight0;
						colrs[i] = new Color(boneWeights[i].weight0, boneWeights[i].weight0, boneWeights[i].weight0, 1);

						boneWeights[i].boneIndex0 = boneIndex;


					}
					else
					{
						if (weightStrength > 0)
							boneWeights[i].weight1 += weightFloat;
						else
							boneWeights[i].weight1 -= weightFloat;

						boneWeights[i].weight1 = Mathf.Clamp01(boneWeights[i].weight1);
						boneWeights[i].weight0 = 1 - boneWeights[i].weight1;
						colrs[i] = new Color(boneWeights[i].weight1, boneWeights[i].weight1, boneWeights[i].weight1, 1);

						boneWeights[i].boneIndex1 = boneIndex;


					}
				}


			}

			//		 
			Puppet2D_Editor.currentSelectionMesh.boneWeights = boneWeights;
			if (Puppet2D_Editor.BlackAndWhiteWeights)
				Puppet2D_Editor.currentSelectionMesh.colors = colrs;
			else
				Puppet2D_Editor.currentSelectionMesh.colors = SetColors(boneWeights);
		}

		public static Color[] SetColors(BoneWeight[] boneWeights)
		{
			SkinnedMeshRenderer smr = Puppet2D_Editor.currentSelection.GetComponent<SkinnedMeshRenderer>();



			Color[] colrs = Puppet2D_Editor.currentSelectionMesh.colors;



			//Shader.SetGlobalInt("_BonesCount", smr.bones);

			for (int i = 0; i < boneWeights.Length; i++)
			{
				float hue = ((float)boneWeights[i].boneIndex0 / smr.bones.Length);
				float hue2 = ((float)boneWeights[i].boneIndex1 / smr.bones.Length);
				//hue /= 2f;

				Color Color1 = (Color.HSVToRGB(hue * 1f, 1f, 1f));//+  HsvToRgb(hue2*255f, 255, 255)*boneWeights[i].weight1 );
				Color Color2 = (Color.HSVToRGB(hue2 * 1f, 1f, 1f));//+  HsvToRgb(hue2*255f, 255, 255)*boneWeights[i].weight1 );

				colrs[i] = Color.Lerp(Color2, Color1, boneWeights[i].weight0 / (boneWeights[i].weight0 + boneWeights[i].weight1));


				//colrs[i] += new Color ((float)boneWeights[i].boneIndex1/smr.bones.Length, (float)boneWeights[i].boneIndex1/smr.bones.Length, boneWeights[i].boneIndex1/smr.bones.Length, 1);
				//   colrs[i] /= 2f;

				//Shader.SetGlobalColor("_Bones" + i.ToString(), new Color(i/ smr.bones.Length,i/ smr.bones.Length,i/ smr.bones.Length,1));


			}

			return colrs;
		}
		static public List<int> GetNeighbors(int[] triangles, int index)
		{
			List<int> verts = new List<int>();

			for (int i = 0; i < triangles.Length / 3; i++)
			{
				// see if the triangle contains the index

				bool found = false;
				for (int j = 0; j < 3; j++)
				{
					int cur = triangles[i * 3 + j];
					if (cur == index) found = true;
				}
				// if we found the index in the triangle, append the others.
				if (found)
				{
					for (int j = 0; j < 3; j++)
					{
						int cur = triangles[i * 3 + j];
						if (verts.IndexOf(cur) == -1 && cur != index)
						{
							verts.Add(cur);
						}
					}
				}
			}
			return verts;
		}
		public static void PaintSmoothWeights(Vector3 mousepos)
		{
			//		Debug.Log("Paint Smooth Weights");

			Vector3[] vertices = Puppet2D_Editor.currentSelectionMesh.vertices;
			Color[] colrs = Puppet2D_Editor.currentSelectionMesh.colors;
			BoneWeight[] boneWeights = Puppet2D_Editor.currentSelectionMesh.boneWeights;

			Vector3 pos = Puppet2D_Editor.currentSelection.transform.InverseTransformPoint(mousepos);
			Undo.RecordObject(Puppet2D_Editor.currentSelectionMesh, "Weight paint");
			pos = new Vector3(pos.x, pos.y, 0);

			SkinnedMeshRenderer smr = Puppet2D_Editor.currentSelection.GetComponent<SkinnedMeshRenderer>();
			int boneIndex = smr.bones.ToList().IndexOf(Puppet2D_Editor.paintWeightsBone.transform);

			if (boneIndex < 0)
			{
				Debug.LogWarning(Puppet2D_Editor.paintWeightsBone.name + " is not connected to skin");
				return;
			}
			List<int> vertsInCircle = new List<int>();

			List<float> averageWeights = new List<float>();

			List<int> trianglesInRange = new List<int>();
			for (int i = 0; i < smr.sharedMesh.triangles.Count(); i++)
			{
				float sqrMagnitude = (vertices[smr.sharedMesh.triangles[i]] - pos).magnitude;
				if (sqrMagnitude > Puppet2D_Editor.EditSkinWeightRadius)
					continue;

				trianglesInRange.Add(smr.sharedMesh.triangles[i]);

			}
			for (int i = 0; i < vertices.Length; i++)
			{


				float sqrMagnitude = (vertices[i] - pos).magnitude;
				if (sqrMagnitude > Puppet2D_Editor.EditSkinWeightRadius)
					continue;



				List<int> connectedVert = GetNeighbors(trianglesInRange.ToArray(), i);

				float combinedWeights = 0f;
				int numberConnectedVerts = 1;

				if (boneWeights[i].boneIndex0 == boneIndex)
				{
					combinedWeights += boneWeights[i].weight0;


				}
				else if (boneWeights[i].boneIndex1 == boneIndex)
				{

					combinedWeights += boneWeights[i].weight1;

				}

				foreach (int vert in connectedVert)
				{
					sqrMagnitude = (vertices[vert] - pos).magnitude;
					if (sqrMagnitude > Puppet2D_Editor.EditSkinWeightRadius)
						continue;

					numberConnectedVerts++;
					if (boneWeights[vert].boneIndex0 == boneIndex)
					{
						combinedWeights += boneWeights[vert].weight0;


					}
					else if (boneWeights[vert].boneIndex1 == boneIndex)
					{

						combinedWeights += boneWeights[vert].weight1;

					}
				}
				if (numberConnectedVerts != 0)
				{

					vertsInCircle.Add(i);
					combinedWeights /= numberConnectedVerts;
					averageWeights.Add(combinedWeights);

					//				Debug.Log ("vert " + i + " has " + numberConnectedVerts + " connected verts, with new combined weight of  " + combinedWeights );
				}

			}



			//Debug.Log ("number verts " + vertsInCircle.Count + " combined weights " + combinedWeights);
			for (int j = 0; j < vertsInCircle.Count; j++)
			{
				int i = vertsInCircle[j];


				float newWeight = Mathf.Lerp(colrs[i].r, averageWeights[j], Puppet2D_Editor.paintWeightsStrength);


				if (boneWeights[i].boneIndex0 < 0)
					boneWeights[i].boneIndex0 = 0;
				if (boneWeights[i].boneIndex1 < 0)
					boneWeights[i].boneIndex1 = 0;

				//boneWeights[i].weight0 = newWeight;
				//boneWeights[i].weight1 = 1-newWeight;
				colrs[i] = new Color(newWeight, newWeight, newWeight);


				if (boneWeights[i].boneIndex0 == boneIndex)
				{

					boneWeights[i].weight0 = colrs[i].r;
					boneWeights[i].weight1 = 1 - colrs[i].r;

				}
				else if (boneWeights[i].boneIndex1 == boneIndex)
				{

					boneWeights[i].weight1 = colrs[i].r;
					boneWeights[i].weight0 = 1 - colrs[i].r;

				}
				else if (colrs[i].r != 0)
				{
					if (boneWeights[i].weight0 == 0)
					{
						boneWeights[i].weight0 = colrs[i].r;
						boneWeights[i].boneIndex0 = boneIndex;
						boneWeights[i].weight1 = 1 - colrs[i].r;
					}
					else if (boneWeights[i].weight1 == 0)
					{
						boneWeights[i].weight1 = colrs[i].r;
						boneWeights[i].boneIndex1 = boneIndex;
						boneWeights[i].weight0 = 1 - colrs[i].r;
					}
				}
				/*else if (colrs[i].r != 0 || boneWeights[i].weight1 + boneWeights[i].weight2 + boneWeights[i].weight3 > 0)
				{
					if(boneWeights[i].weight0<boneWeights[i].weight1)
					{
						boneWeights[i].weight0 = colrs[i].r ;
						boneWeights[i].boneIndex0 = boneIndex;
						boneWeights[i].weight1 = 1-colrs[i].r ;
					}
					else
					{
						boneWeights[i].weight1 = colrs[i].r ;
						boneWeights[i].boneIndex1 = boneIndex;
						boneWeights[i].weight0 = 1-colrs[i].r ;
					}
				}*/



			}

			//		Puppet2D_Editor.currentSelectionMesh.colors = colrs;
			Puppet2D_Editor.currentSelectionMesh.boneWeights = boneWeights;
			if (Puppet2D_Editor.BlackAndWhiteWeights)
				Puppet2D_Editor.currentSelectionMesh.colors = colrs;
			else
				Puppet2D_Editor.currentSelectionMesh.colors = SetColors(boneWeights);
		}

		public static void PaintSmoothWeightsOld(Vector3 mousepos)
		{

			//		Debug.Log("Paint Smooth Weights Old");

			Vector3[] vertices = Puppet2D_Editor.currentSelectionMesh.vertices;
			Color[] colrs = Puppet2D_Editor.currentSelectionMesh.colors;
			BoneWeight[] boneWeights = Puppet2D_Editor.currentSelectionMesh.boneWeights;
			int[] tris = Puppet2D_Editor.currentSelectionMesh.triangles;

			Vector3 pos = Puppet2D_Editor.currentSelection.transform.InverseTransformPoint(mousepos);
			Undo.RecordObject(Puppet2D_Editor.currentSelectionMesh, "Weight paint");
			pos = new Vector3(pos.x, pos.y, 0);

			SkinnedMeshRenderer smr = Puppet2D_Editor.currentSelection.GetComponent<SkinnedMeshRenderer>();
			int boneIndex = smr.bones.ToList().IndexOf(Puppet2D_Editor.paintWeightsBone.transform);

			if (boneIndex < 0)
			{
				Debug.LogWarning(Puppet2D_Editor.paintWeightsBone.name + " is not connected to skin");
				return;
			}


			for (int i = 0; i < tris.Length; i++)
			{

				if (boneWeights[tris[i]].boneIndex0 < 0)
					boneWeights[tris[i]].boneIndex0 = 0;
				if (boneWeights[tris[i]].boneIndex1 < 0)
					boneWeights[tris[i]].boneIndex1 = 0;

				int indexB = 0;
				int indexC = 0;

				if (i % 3 == 2)
				{
					indexB = tris[i - 1];
					indexC = tris[i - 2];

				}
				else if ((i) % 3 == 1)
				{
					indexB = tris[i - 1];
					indexC = tris[i + 1];
				}
				else if ((i) % 3 == 0)
				{

					indexB = tris[i + 1];
					indexC = tris[i + 2];

				}
				float sqrMagnitude = (vertices[tris[i]] - pos).magnitude;
				if (sqrMagnitude < Puppet2D_Editor.EditSkinWeightRadius)
				{

					//Debug.Log("h");
					colrs[tris[i]] = Color.black;
					int blend = 1;
					if (boneWeights[tris[i]].boneIndex0 == boneIndex)
					{
						colrs[tris[i]] += new Color(boneWeights[tris[i]].weight0, boneWeights[tris[i]].weight0, boneWeights[tris[i]].weight0);
					}
					else if (boneWeights[tris[i]].boneIndex1 == boneIndex)
					{
						colrs[tris[i]] += new Color(boneWeights[tris[i]].weight1, boneWeights[tris[i]].weight1, boneWeights[tris[i]].weight1);
					}
					else
					{
						if (boneWeights[tris[i]].weight0 < boneWeights[tris[i]].weight1)
						{
							boneWeights[tris[i]].boneIndex0 = boneIndex;
						}
						else
						{
							boneWeights[tris[i]].boneIndex1 = boneIndex;
						}
					}


					if (boneWeights[indexB].boneIndex0 == boneIndex)
					{
						colrs[tris[i]] += new Color(boneWeights[indexB].weight0, boneWeights[indexB].weight0, boneWeights[indexB].weight0);
						blend++;
					}
					else if (boneWeights[indexB].boneIndex1 == boneIndex)
					{
						colrs[tris[i]] += new Color(boneWeights[indexB].weight1, boneWeights[indexB].weight1, boneWeights[indexB].weight1);
						blend++;

					}
					if (boneWeights[indexC].boneIndex0 == boneIndex)
					{
						blend++;
						colrs[tris[i]] += new Color(boneWeights[indexC].weight0, boneWeights[indexC].weight0, boneWeights[indexC].weight0);
					}
					else if (boneWeights[indexC].boneIndex1 == boneIndex)
					{
						blend++;
						colrs[tris[i]] += new Color(boneWeights[indexC].weight1, boneWeights[indexC].weight1, boneWeights[indexC].weight1);
					}

					colrs[tris[i]] /= blend;
					if (boneWeights[tris[i]].boneIndex0 == boneIndex)
					{
						boneWeights[tris[i]].weight0 = Mathf.Lerp(boneWeights[tris[i]].weight0, colrs[tris[i]].r, Puppet2D_Editor.paintWeightsStrength * Puppet2D_Editor.paintWeightsStrength);
						boneWeights[tris[i]].weight1 = 1 - boneWeights[tris[i]].weight0;
						colrs[tris[i]] = new Color(boneWeights[tris[i]].weight0, boneWeights[tris[i]].weight0, boneWeights[tris[i]].weight0);
					}
					else if (boneWeights[tris[i]].boneIndex1 == boneIndex)
					{
						boneWeights[tris[i]].weight1 = Mathf.Lerp(boneWeights[tris[i]].weight1, colrs[tris[i]].r, Puppet2D_Editor.paintWeightsStrength * Puppet2D_Editor.paintWeightsStrength);
						boneWeights[tris[i]].weight0 = 1 - boneWeights[tris[i]].weight1;
						colrs[tris[i]] = new Color(boneWeights[tris[i]].weight1, boneWeights[tris[i]].weight1, boneWeights[tris[i]].weight1);
					}

				}


			}
			//        Puppet2D_Editor.currentSelectionMesh.colors = colrs;
			Puppet2D_Editor.currentSelectionMesh.boneWeights = boneWeights;
			if (Puppet2D_Editor.BlackAndWhiteWeights)
				Puppet2D_Editor.currentSelectionMesh.colors = colrs;
			else
				Puppet2D_Editor.currentSelectionMesh.colors = SetColors(boneWeights);

		}

		public static void ChangePaintRadius(Vector3 pos)
		{
			Puppet2D_Editor.EditSkinWeightRadius = (pos - Puppet2D_Editor.ChangeRadiusStartPosition).x + Puppet2D_Editor.ChangeRadiusStartValue;

		}
		public static void ChangePaintStrength(Vector3 pos)
		{

			Puppet2D_Editor.paintWeightsStrength = (pos - Puppet2D_Editor.ChangeRadiusStartPosition).x * 0.1f + Puppet2D_Editor.ChangeRadiusStartValue;
			Puppet2D_Editor.paintWeightsStrength = Mathf.Clamp01(Puppet2D_Editor.paintWeightsStrength);
		}
		public static float GetNeighbourWeight(Vector3[] vertices, BoneWeight[] boneWeights, List<int> indexes, int index, int boneIndex)
		{
			float distance = 1000000f;
			int closestIndex = indexes[0];
			for (int i = 0; i < indexes.Count; i++)
			{
				float checkDistance = (vertices[indexes[i]] - vertices[index]).magnitude;
				if (checkDistance < distance)
				{
					closestIndex = indexes[i];
					distance = checkDistance;
				}

			}
			if (boneWeights[closestIndex].boneIndex0 == boneIndex)
				return boneWeights[closestIndex].weight0;
			if (boneWeights[closestIndex].boneIndex1 == boneIndex)
				return boneWeights[closestIndex].weight1;
			if (boneWeights[closestIndex].boneIndex2 == boneIndex)
				return boneWeights[closestIndex].weight2;
			if (boneWeights[closestIndex].boneIndex3 == boneIndex)
				return boneWeights[closestIndex].weight3;
			return 0;

		}
		public static Mesh SaveFBXMesh(Mesh mesh, bool Duplicate = false)
		{
			string path = AssetDatabase.GetAssetPath(mesh);
			string extension = Path.GetExtension(path);
			//Debug.Log("extension is " + extension);

			if (extension == ".asset" && !Duplicate)
			{
				return mesh;
			}
			else
			{
				string[] pathSplit = path.Split('/');
				string meshPath = "";
				for (int i = 0; i < pathSplit.Length - 1; i++)
				{
					meshPath += pathSplit[i] + "/";
				}
				if (meshPath == "")
					meshPath = "Assets/";


				string outMeshPath = meshPath + mesh.name + "P3D.asset";

				outMeshPath = AssetDatabase.GenerateUniqueAssetPath(outMeshPath);


				//Debug.Log("path is " + outMeshPath);
				Mesh newMesh = new Mesh();
				newMesh.vertices = mesh.vertices;
				newMesh.colors = mesh.colors;
				newMesh.normals = mesh.normals;
				newMesh.uv = mesh.uv;
				newMesh.bindposes = mesh.bindposes;
				newMesh.boneWeights = mesh.boneWeights;
				newMesh.tangents = mesh.tangents;
				newMesh.subMeshCount = mesh.subMeshCount;
				for (int index = 0; index < mesh.subMeshCount; index++)
				{
					newMesh.SetTriangles(mesh.GetTriangles(index), index);
				}

				AssetDatabase.CreateAsset(newMesh, outMeshPath);
				Debug.Log("Saving mesh into " + outMeshPath);
				return AssetDatabase.LoadAssetAtPath(outMeshPath, typeof(Mesh)) as Mesh;
			}
		}
	}
	
}
