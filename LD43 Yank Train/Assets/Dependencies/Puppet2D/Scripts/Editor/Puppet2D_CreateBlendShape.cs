using UnityEngine;
using System.Collections;
using UnityEditor;
namespace Puppet2D
{
	public class CreateBlendShape : MonoBehaviour
	{
		static public void MakeBlendShape()
		{
			GameObject sel = Selection.activeGameObject;
			if (sel != null && sel.GetComponent<SkinnedMeshRenderer>()!= null)
			{
				
				GameObject newBlendShape = new GameObject("newBlendShape");
				Puppet2D_BlendShape bs = newBlendShape.AddComponent<Puppet2D_BlendShape>();
				bs.SourceSkin = sel.GetComponent<SkinnedMeshRenderer>();

				MeshFilter meshFilter = newBlendShape.AddComponent<MeshFilter>();
				Mesh newMesh = Puppet2D_Skinning.SaveFBXMesh(bs.SourceSkin.sharedMesh, true);
				meshFilter.sharedMesh = newMesh;
				MeshRenderer mr = newBlendShape.AddComponent<MeshRenderer>();
				bs.TargetMeshFilter = meshFilter;
				mr.sharedMaterials = bs.SourceSkin.sharedMaterials;
				bs.Init();
				Selection.activeGameObject = newBlendShape;
			}

			
		}
		static public void ClearBlendShapes()
		{
			GameObject sel = Selection.activeGameObject;
			if (sel != null && sel.GetComponent<SkinnedMeshRenderer>() != null)
			{
				sel.GetComponent<SkinnedMeshRenderer>().sharedMesh.ClearBlendShapes();
			}
		}

	}
}
