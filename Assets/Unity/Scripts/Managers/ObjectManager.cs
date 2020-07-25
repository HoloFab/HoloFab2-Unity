//#define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
#endif
#if UNITY_ANDROID
using System.Threading;
using GoogleARCore.Examples.Common;
using GoogleARCore.Examples.HelloAR;
#endif

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Generatable Object manager.
	// TODO:
	// - Later: Move processors here?
	[RequireComponent(typeof(MeshProcessor))]
	[RequireComponent(typeof(RobotProcessor))]
	[RequireComponent(typeof(TagProcessor))]
	[RequireComponent(typeof(Point3DProcessor))]
	public class ObjectManager : Type_Manager<ObjectManager> {
		// - CPlane object tag.
		private string tagCPlane = "CPlane";
		private string layerScanMesh = "Spatial Awareness";
		// - Local reference of CPlane object
		public GameObject cPlane;
		// - Meshes of the environment
		public List<MeshRenderer> scannedEnvironment;
		// Keep track of the scanned grid status.
		[HideInInspector]
		public bool flagGridVisible = true;
        
		// Local Variables.
		private string sourceName = "Object Manager";
        
		void OnEnable(){
			DebugUtilities.UserMessage("Hollo World . . .");
			#if UNITY_ANDROID
			Thread.Sleep(1500);
			#endif
			DebugUtilities.UserMessage("Your IP is:\n" + NetworkUtilities.LocalIPAddress());
			#if UNITY_ANDROID
			Thread.Sleep(3500);
			#endif
			DebugUtilities.UserMessage("Place your CPlane by tapping on scanned mesh.");
		}
		////////////////////////////////////////////////////////////////////////
		// If c plane is not found - hint user and return false.
		public bool CheckCPlane(){
			if (this.cPlane == null) {
				this.cPlane = GameObject.FindGameObjectWithTag(this.tagCPlane);
				if (this.cPlane == null) {
					DebugUtilities.UserMessage("Place your CPlane by tapping on scanned mesh.");
					return false;
				}
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "CPlane: " + this.cPlane);
				#endif
			}
			// #if UNITY_ANDROID
			// HoloFabARController.cPlaneInstance = this.cPlane;
			// #endif
			return true;
		}
		////////////////////////////////////////////////////////////////////////
		// Find environment meshes
		public void FindMeshes(){
			this.scannedEnvironment = new List<MeshRenderer>();
			#if WINDOWS_UWP
			// Microsoft Windows MRTK
			// Cast the Spatial Awareness system to IMixedRealityDataProviderAccess to get an Observer
			var access = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
			// Get the first Mesh Observer available, generally we have only one registered
			var observers = access.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
			// Loop through all known Meshes
			foreach (var observer in observers) {
				foreach (SpatialAwarenessMeshObject meshObject in observer.Meshes.Values) {
					this.scannedEnvironment.Add(meshObject.Renderer);
				}
			}
			#elif UNITY_ANDROID
			// Android ARkit
			PointcloudVisualizer[] visualizers = FindObjectsOfType<PointcloudVisualizer>();
			foreach (PointcloudVisualizer visualizer in visualizers)
				this.scannedEnvironment.Add(visualizer.gameObject.GetComponent<MeshRenderer>());
			#endif
		}
		// Toggle all meshes.
		public void ToggleEnvironmentMeshes(){
			FindMeshes();
			this.flagGridVisible = !this.flagGridVisible;
			foreach (MeshRenderer renderer in this.scannedEnvironment)
				renderer.enabled = this.flagGridVisible;
		}
		// Check IF object is and environment Mesh.
		public bool CheckEnvironmentObject(GameObject goItem){
			return goItem.layer == LayerMask.NameToLayer(this.layerScanMesh);
		}
	}
}