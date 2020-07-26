//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

#if WINDOWS_UWP
using Microsoft.MixedReality.Toolkit;
#elif UNITY_ANDROID
// using GoogleARCore.Examples.Common;
#endif

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Unity Component Controlling Addition and Removal of Generated Objects.
	public class GeneralUIMenu : MonoBehaviour {
		[Header("Variables Set From Scene.")]
		[Tooltip("Button to exit application.")]
		public Button buttonExit;
		[Tooltip("Button to Destroy CPlane.")]
		public Button buttonDestroyCplane;
		[Tooltip("Button to Toggle Grid.")]
		public Button buttonToggleGrid;
		[Tooltip("Button to Delete Objects.")]
		public Button buttonObjects;
        
		// Events to be raised on clicks:
		// - exit application
		public void OnExit() {
			#if DEBUG
			Debug.Log("General UI: exit.");
			#endif
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else // UNITY_ANDROID
			Application.Quit();
			#endif
		}
		// - Destroy CPlane
		public void OnDestroyCPlane() {
			#if DEBUG
			Debug.Log("General UI: reset C-Plane.");
			#endif
			//TODO not actually delete c plane but start placing it (put infron tf camera and activate placeable)
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
			ObjectManager.instance.cPlane.GetComponent<Interactible_Placeable>().ForcePlacement();
			//#if WINDOWS_UWP
			//ObjectManager.instance.cPlane.GetComponent<Interactible_Placeable>().ForcePlacement();
			//#else
			//ObjectManager.instance.cPlane.SetActive(false);
			////DestroyImmediate(ObjectManager.instance.cPlane);
			////Resources.UnloadUnusedAssets();
			//#endif
		}
		// - Toggle AR Core Grid
		public void OnTogglePointsAndGrids() {
			#if DEBUG
			Debug.Log("General UI: toggle scanned environment visibility.");
			#endif
			ObjectManager.instance.ToggleEnvironmentMeshes();
			#if WINDOWS_UWP
			// Microsoft Windows MRTK
			// Toggle Mesh Observation from all Observers
			if (ObjectManager.instance.flagGridVisible)
				CoreServices.SpatialAwarenessSystem.ResumeObservers();
			else
				CoreServices.SpatialAwarenessSystem.SuspendObservers();
			#elif UNITY_ANDROID
			// Android ARkit
			DetectedPlaneVisualizerExtender.flagVisible = ObjectManager.instance.flagGridVisible;
			#endif
		}
		// - Destroy Objects
		public void OnDestroyObjects() {
			#if DEBUG
			Debug.Log("General UI: destroy all objects.");
			#endif
			ObjectManager.instance.gameObject.GetComponent<MeshProcessor>().DeleteMeshes(SourceType.TCP);
			ObjectManager.instance.gameObject.GetComponent<MeshProcessor>().DeleteMeshes(SourceType.UDP);
			ObjectManager.instance.gameObject.GetComponent<LabelProcessor>().DeleteLabels();
			ObjectManager.instance.gameObject.GetComponent<RobotProcessor>().DeleteRobots();
			ObjectManager.instance.gameObject.GetComponent<Point3DProcessor>().DeletePoints();
		}
		public void OnAdd3DPoint() {
			#if DEBUG
			Debug.Log("General UI: add a point.");
			#endif
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
            
			ObjectManager.instance.gameObject.GetComponent<Point3DProcessor>().AddPoint();
		}
	}
}