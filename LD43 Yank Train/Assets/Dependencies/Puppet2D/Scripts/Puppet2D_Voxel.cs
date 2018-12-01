using UnityEngine;
using System.Collections;

namespace Puppet2D
{
	public class Puppet2D_Voxel
	{
		public int Bone0 = -1, Bone1 = -1, Bone2 = -1, Bone3 = -1;
		public float Weight0, Weight1, Weight2, Weight3 = 0f;
		public Vector3[] NeighbourIndexes = new Vector3[6];
		public Bounds bounds;
		public bool Inside = true;
		public Vector3 Pos = Vector3.zero;
		public Vector3 Scale = Vector3.one;

	}
}
