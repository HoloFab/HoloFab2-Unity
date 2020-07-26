//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Process Received Labels and update relevant gameObjects.
	// TODO:
	// - Should Objects be place in CPlane or in Object MAnager like robots?
	public class LabelProcessor : MonoBehaviour {
		[Tooltip("A prefab of a Label.")]
		public GameObject goPrefabLabel;
		// - Tag Label.
		public string tagLabel = "Label";
        
		// Decode Received Data.
		public void ProcessTag(LabelData receivedLabels) {
			GameObject[] goLabels = GameObject.FindGameObjectsWithTag(tagLabel);
            
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
            
			// Loop through tags received.
			for (int i = 0; i < receivedLabels.text.Count; i++) {
				#if DEBUG
				Debug.Log("Label: total tags found: " + receivedLabels.text.Count);
				#endif
				// Ensure tag exists.
				GameObject labelInstance;
				if (i < goLabels.Length) { // Update old tag
					#if DEBUG
					Debug.Log("Label: tag old updating.");
					#endif
					labelInstance = goLabels[i];
				} else { // Create new tag
					#if DEBUG
					Debug.Log("Label: tag new instantiating.");
					#endif
					labelInstance = Instantiate(this.goPrefabLabel, ObjectManager.instance.cPlane.transform.position,
					                            ObjectManager.instance.cPlane.transform.rotation,
					                            ObjectManager.instance.cPlane.transform);
					labelInstance.tag = tagLabel;
				}
                
				// Prepare Values
				Vector3 currentLocalPosition = new Vector3(receivedLabels.textLocation[i][0],
				                                           receivedLabels.textLocation[i][1],
				                                           receivedLabels.textLocation[i][2]);
				Color currentColor = new Color((float)receivedLabels.textColor[i][1] / 255.0f,
				                               (float)receivedLabels.textColor[i][2] / 255.0f,
				                               (float)receivedLabels.textColor[i][3] / 255.0f,
				                               (float)receivedLabels.textColor[i][0] / 255.0f);
				TextMeshProUGUI labelText = labelInstance.GetComponent<TextMeshProUGUI>();
                
				// Set Values.
				// - Text Location
				labelInstance.transform.localPosition = currentLocalPosition;
				// - Text
				labelText.text = receivedLabels.text[i];
				// - Text Size
				labelText.fontSize = receivedLabels.textSize[i];
				// - Text Clor
				labelText.color = currentColor;
			}
            
			// Delete Extraneous Labels.
			if (receivedLabels.text.Count < goLabels.Length) {
				#if DEBUG
				Debug.Log("Label: tags extra deleting.");
				#endif
				DeleteLabels(receivedLabels.text.Count);
			}
		}
		// Delete Labels Starting from Last Index.
		public void DeleteLabels(int lastIndex = 0) {
			GameObject[] goLabels = GameObject.FindGameObjectsWithTag(this.tagLabel);
			for (int i = goLabels.Length-1; i >= lastIndex; i--) {
				Destroy(goLabels[i]);
			}
			Resources.UnloadUnusedAssets();
		}
	}
}