using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace RTSGame
{
	public class SortGameobjectsInParentByName : MonoBehaviour
	{

		[ContextMenu("SortGameobjects")]
		public void SortGO()
        {
			foreach (var obj in transform.GetComponentsInChildren<Transform>())
			{
				if (obj.parent != null)
                {
					if (obj.parent != transform)
						continue;
				}

				List<Transform> children = new List<Transform>();
				for (int i = obj.transform.childCount - 1; i >= 0; i--)
				{
					Transform child = obj.transform.GetChild(i);
					children.Add(child);
					child.parent = null;
				}
				children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
				foreach (Transform child in children)
				{
					child.parent = obj.transform;
				}
			}
		}
	}
}