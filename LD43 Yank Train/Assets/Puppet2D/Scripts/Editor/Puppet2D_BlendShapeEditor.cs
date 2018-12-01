using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Puppet2D
{
	[CustomEditor(typeof(Puppet2D_BlendShape))]
	public class Puppet2D_BlendShapeEditor : Editor
	{

		public override void OnInspectorGUI()
		{
			
			if (GUILayout.Button("Set Blend Shape"))
			{
				(target as Puppet2D_BlendShape).SetBlendShape();
			}
			DrawDefaultInspector();

		}
		
	}
}