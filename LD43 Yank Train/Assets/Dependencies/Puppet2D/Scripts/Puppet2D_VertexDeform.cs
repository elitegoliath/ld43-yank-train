#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Puppet2D
{
	[ExecuteInEditMode]
	public class Puppet2D_VertexDeform : MonoBehaviour
	{

		private static float CURRENT_SIZE = 0.02f;
		private static float CURRENT_FALLOFF = 1f;

		public float _size = CURRENT_SIZE;
		public float _falloff = CURRENT_FALLOFF;

		public Puppet2D_BlendShape _parent;

		private float _lastKnownSize = CURRENT_SIZE;
		private float _lastKnownFalloff = CURRENT_FALLOFF;


		public Vector3 IntialPos;
		[SerializeField]
		public bool Active = true;
		[SerializeField]
		public bool Editted = false;
		[SerializeField]
		public bool Selected = false;
		public Vector3 CurrentPos;

		public List<int> ConnectedIndexes = new List<int>();
		[SerializeField]
		public Color ColorVal;
		[SerializeField]
		public float CurrentBlend;
		[SerializeField]
		public int CurrentIndex;


		private bool UndoRedo = false;
		//private GameObject _currentSel = null;
		private void OnEnable()
		{
			CurrentPos = transform.position;
			ColorVal = Color.white;
			Undo.undoRedoPerformed += undoRedoPerformed;
			Undo.willFlushUndoRecord += flush;
			Selection.selectionChanged += SelectionChange;

		}
		void OnDestroy()
		{
			Undo.undoRedoPerformed -= undoRedoPerformed;
			Undo.willFlushUndoRecord -= flush;
			Selection.selectionChanged -= SelectionChange;
		}
		void SelectionChange()
		{
			ColorVal = Color.white;
			_parent.Deforms[CurrentIndex].CurrentPos = _parent.Deforms[CurrentIndex].gameObject.transform.position;
			_parent.Deforms[CurrentIndex].Editted = false;
			_parent.Deforms[CurrentIndex].Selected = false;
			UndoRedo = false;
		}
		void flush()
		{
			_parent.Deforms[CurrentIndex].CurrentPos = _parent.Deforms[CurrentIndex].gameObject.transform.position;
			_parent.Deforms[CurrentIndex].Editted = false;
			_parent.Deforms[CurrentIndex].Selected = false;
			UndoRedo = false;
		}
		void undoRedoPerformed()
		{
			UndoRedo = true;
		}
		void Update()
		{
			Run();
		}
		void Run()
		{
			
			
			//ColorVal = Color.white;
			// Change the size if the user requests it
			if (_lastKnownSize != _size)
			{
				_lastKnownSize = _size;
				CURRENT_SIZE = _size;
			}

			// Ensure the rest of the gizmos know the size has changed...
			if (CURRENT_SIZE != _lastKnownSize)
			{
				_lastKnownSize = CURRENT_SIZE;
				_size = _lastKnownSize;
			}

			

			// FALL OFF WIP
			
			
			if (_lastKnownFalloff != _falloff)
			{
				_lastKnownFalloff = _falloff;
				CURRENT_FALLOFF = _falloff;
			}

			
			if (CURRENT_FALLOFF != _lastKnownFalloff)
			{
				_lastKnownFalloff = CURRENT_FALLOFF;
				_falloff = _lastKnownFalloff;
			}

			if (UndoRedo)
				return;

			float dist = 0;

			List<int> WorkingList = new List<int>();
			Queue<int> indexQueue = new Queue<int>();

			List<GameObject> testGo = new List<GameObject>();
			foreach (GameObject go in Selection.gameObjects)
			{
				Puppet2D_VertexDeform vd = go.GetComponent<Puppet2D_VertexDeform>();
				if (vd)
				{
					vd.Selected = true;
					//if(vd.Active)
					testGo.Add(go);
				}
			}

			if (testGo.Contains(this.gameObject))
			{
				
				//if (Active)
				{
					
					WorkingList.Add(CurrentIndex);
					Editted = true;
					ColorVal = Color.green;

					_parent.verts[CurrentIndex] = _parent.Deforms[CurrentIndex].transform.position;

					for (int c = 0; c < ConnectedIndexes.Count; c++)
					{
						indexQueue.Enqueue(ConnectedIndexes[c]);
					}
					Color blendColor = Color.white;
					while (indexQueue.Count > 0)
					{
						int workingIndex = indexQueue.Dequeue();
						if (!_parent.Deforms[workingIndex].Editted)
						{
							_parent.Deforms[workingIndex].ColorVal = blendColor;
							WorkingList.Add(workingIndex);
							_parent.Deforms[workingIndex].Editted = true;

							dist = Vector3.Distance(IntialPos, _parent.Deforms[workingIndex].IntialPos);
							if (dist < _falloff)
							{
								float blend = (1 - (dist / _falloff));
								Undo.RecordObject(_parent.Deforms[workingIndex].transform, "bs record");
								Undo.RecordObject(_parent, "bs record");
								if (!_parent.Deforms[workingIndex].Selected)
								{									
									_parent.Deforms[workingIndex].transform.position += (transform.position - CurrentPos) * blend;
								}
								_parent.verts[workingIndex] = _parent.Deforms[workingIndex].transform.position;

								blendColor = Color.Lerp(Color.white, Color.green, blend);
								_parent.Deforms[workingIndex].ColorVal = blendColor;

								for (int c = 0; c < _parent.Deforms[workingIndex].ConnectedIndexes.Count; c++)
								{
									indexQueue.Enqueue(_parent.Deforms[workingIndex].ConnectedIndexes[c]);
								}
							}
						}

					}


				}

			}

			





		}
		
		
		void OnDrawGizmos()
		{
			//if (Active)
			{
			
				if (Selected)
				{
					Gizmos.color = Color.green * 2f;
					Gizmos.DrawSphere(transform.position, CURRENT_SIZE*1.5f);

				}
				else
				{
					Gizmos.color = 2f*ColorVal;
					Gizmos.DrawSphere(transform.position, CURRENT_SIZE);

				}
			}
			
		}
	}
}
#endif