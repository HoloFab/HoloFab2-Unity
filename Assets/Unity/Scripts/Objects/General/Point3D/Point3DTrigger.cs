using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	public class Point3DTrigger : MonoBehaviour {
		// Buffer parent object not to look for it everyt time.
		private Point3DController pointController;
		// Function to be Raised on mouse event.
		void OnMouseDown(){
			if (this.pointController == null)
				this.pointController = transform.parent.GetComponent<Point3DController>();
			this.pointController.ToggleState();
		}
	}
}