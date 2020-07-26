//#define DEBUG
#undef DEBUG

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Process Received Meshes and update relevant gameObjects.
	// TODO:
	// - Check if shader now is fixed.
	// - Difference between Prefabs is only tag?
	public class MeshProcessor : MonoBehaviour {
		[Tooltip("An Prefab of a mesh.")]
		public GameObject goPrefabMesh;
		// [Tooltip("An Prefab of a mesh.")]
		// public GameObject goPrefabMeshPlus;
		// - Generated Object Tags.
		public string tagMesh = "MeshTCP";
		public string tagMeshPlus = "MeshUDP";
        
		// Decode Received Data.
		public void ProcessMesh(List<MeshData> receivedMeshes, SourceType sourceType) {
			#if DEBUG
			Debug.Log("Mesh: Received Count: " + receivedMeshes.Count + ". Type: " + (int)sourceType);
			#endif
            
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
            
			// Find relevant objects based on source to update
			GameObject[] goRelevantMeshes;
			string currentTag;
			GameObject currentPrefab;
			switch (sourceType) {
			 case (SourceType.TCP):
				 #if DEBUG
				 Debug.Log("Mesh: TCP");
				 #endif
				 currentPrefab = this.goPrefabMesh;
				 currentTag = this.tagMesh;
				 break;
			 case (SourceType.UDP):
				 #if DEBUG
				 Debug.Log("Mesh: UDP");
				 #endif
				 currentPrefab = this.goPrefabMesh;
				 currentTag = this.tagMeshPlus;
				 break;
			 default:
				 return;
			}
			goRelevantMeshes = GameObject.FindGameObjectsWithTag(currentTag);
            
			List<Vector3> currentVertices;
			List<int> currentFaces;
			List<Color> currentColors;
			// Loop through all received meshes.
			for (int i = 0; i < receivedMeshes.Count; i++) {
				currentVertices = new List<Vector3>();
				currentFaces = new List<int>();
				currentColors = new List<Color>();
				// Condition Vertex based data (points and colors). // Later: normals etc.
				for (int j = 0; j < receivedMeshes[i].vertices.Count; j++) {
					currentVertices.Add(new Vector3(receivedMeshes[i].vertices[j][0],
					                                receivedMeshes[i].vertices[j][1],
					                                receivedMeshes[i].vertices[j][2]));
					int colorIndex = (receivedMeshes[i].colors.Count == receivedMeshes[i].vertices.Count) ? j : 0;
					currentColors.Add(new Color((float)receivedMeshes[i].colors[colorIndex][1] / 255.0f,
					                            (float)receivedMeshes[i].colors[colorIndex][2] / 255.0f,
					                            (float)receivedMeshes[i].colors[colorIndex][3] / 255.0f,
					                            1.0f));
				}
				float alpha = (float)receivedMeshes[i].colors[0][0] / 255.0f;
				// Condition Faces.
				for (int j = 0; j < receivedMeshes[i].faces.Count; j++) {
					currentFaces.Add(receivedMeshes[i].faces[j][1]);
					currentFaces.Add(receivedMeshes[i].faces[j][2]);
					currentFaces.Add(receivedMeshes[i].faces[j][3]);
					// If quad - add second face.
					if ((receivedMeshes[i].faces[j][0] == 1)) {
						currentFaces.Add(receivedMeshes[i].faces[j][1]);
						currentFaces.Add(receivedMeshes[i].faces[j][3]);
						currentFaces.Add(receivedMeshes[i].faces[j][4]);
					}
				}
                
				// Set Values
				GameObject meshInstance;
				if (i < goRelevantMeshes.Length) { // TODO: Should be some sort of ID not orders
					#if DEBUG
					Debug.Log("Mesh: Updating old Mesh");
					#endif
					meshInstance = goRelevantMeshes[i];
				} else {
					#if DEBUG
					Debug.Log("Mesh: Adding new Mesh");
					#endif
					meshInstance = Instantiate(currentPrefab, ObjectManager.instance.cPlane.transform.position, ObjectManager.instance.cPlane.transform.rotation, ObjectManager.instance.cPlane.transform);
					meshInstance.tag = currentTag;
				}
				meshInstance.GetComponent<Renderer>().material.SetFloat("_ShadowStrength", 1.0f);
				meshInstance.GetComponent<Renderer>().material.SetFloat("_Alpha", alpha);
				meshInstance.GetComponent<MeshFilter>().mesh = MeshUtilities.DecodeMesh(currentVertices, currentFaces, currentColors);
			}
            
			// Delete Extraneous Meshes.
			if (goRelevantMeshes.Length > receivedMeshes.Count) {
				#if DEBUG
				Debug.Log("Mesh: Meshes extra deleting.");
				#endif
				DeleteMeshes(sourceType, receivedMeshes.Count);
			}
		}
		// Delete Tags Starting from Last Index.
		public void DeleteMeshes(SourceType sourceType, int lastIndex=0) {
			GameObject[] goRelevantMeshes;
			switch (sourceType) {
			 case (SourceType.TCP):
				 goRelevantMeshes = GameObject.FindGameObjectsWithTag(this.tagMesh);
				 break;
			 case (SourceType.UDP):
				 goRelevantMeshes = GameObject.FindGameObjectsWithTag(this.tagMeshPlus);
				 break;
			 default:
				 return;
			}
			for (int i = goRelevantMeshes.Length-1; i >= lastIndex; i--) {
				Destroy(goRelevantMeshes[i]);
			}
			Resources.UnloadUnusedAssets();
		}
	}
}