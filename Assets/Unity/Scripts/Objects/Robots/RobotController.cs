//#define DEBUG
#undef DEBUG

﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Process Received Robot Controllers and update relevant gameObjects.
	// TODO:
	// - Should we Send amount of Axis as first number?
	// - if send list maybe keep it as list here as well?
	// - Later: Ugly, Should be called direclty and not sent to all Controllers.
	public class RobotController : MonoBehaviour {
		[Tooltip("Robot ID associated with this Robot Controller.")]
		public int robotID;
		[HideInInspector]
		public double[] axis;
        
		// Decode Received Data.
		public void ProcessRobotController(RobotControllerData receivedRobotController) {//List<RobotControllerData> receivedRobotControllers) {
			this.axis = receivedRobotController.robotAxisAngles.ToArray();
			// foreach (RobotControllerData receivedRobotController in receivedRobotControllers) {
			// 	if (this.robotID == receivedRobotController.robotID) {
			//			return;
			// 	}
			// }
		}
	}
}