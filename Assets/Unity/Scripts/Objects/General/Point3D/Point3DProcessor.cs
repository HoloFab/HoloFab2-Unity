using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Prepare and take care of the points.
	public class Point3DProcessor : MonoBehaviour {
		// An example of 3D point.
		public GameObject goPoint3DExample;
        
		// History of created objects.
		private List<GameObject> goHistory;
        
		void OnEnable(){
			this.goHistory = new List<GameObject>();
		}
		// Add a point.
		public void AddPoint(){
			if (this.goPoint3DExample != null) {
				GameObject goItem = Instantiate(this.goPoint3DExample,
				                                Camera.main.transform.position + Camera.main.transform.forward,
				                                ObjectManager.instance.cPlane.transform.rotation,
				                                ObjectManager.instance.cPlane.transform);
				this.goHistory.Add(goItem);
			}
		}
		// Delete all previously created points.
		public void DeletePoints(){
			for (int i=this.goHistory.Count-1; i >= 0; i--)
				GameObject.DestroyImmediate(this.goHistory[i]);
			this.goHistory = new List<GameObject>();
		}
	}
}