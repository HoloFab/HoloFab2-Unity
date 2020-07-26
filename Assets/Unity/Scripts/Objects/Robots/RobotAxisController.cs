using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Component on a joint of HoloBot Responsible for Updating local rotation.
	// TODO:
	// - Fix Models so axis value is always same tilt. (for more axis)
	// - Make into an event on receiving robotController data, instead of updating every frame.
	public class RobotAxisController : MonoBehaviour {
		[Header("Necessary Variables.")]
		[Tooltip("Robot Controller managing this joint.")]
		public RobotController robotController;
		[Tooltip("This Joint Index.")]
		public int axisIndex = 0;
        
		void Update() {
			ApplyPose(robotController.axis);
		}
		// Apply given pose.
		public void ApplyPose(double[] currentPose) {
			float curretValue = (float)currentPose[this.axisIndex - 1];
			// Fix Robot orientations.
			float tiltAroundX = 0, tiltAroundY = 0, tiltAroundZ = 0;
			switch (this.axisIndex) {
			 case 1:
				 tiltAroundY = -curretValue;
				 break;
			 case 2:
				 tiltAroundZ = 90 - curretValue;
				 break;
			 case 3:
			 case 5:
				 tiltAroundZ = -curretValue;
				 break;
			 case 4:
			 case 6:
				 tiltAroundX = curretValue;
				 break;
			 default:
				 break;
			}
			// Rotate the joint by converting the angles into a quaternion.
			transform.localRotation = Quaternion.Euler(tiltAroundX, tiltAroundY, tiltAroundZ);
		}
	}
}