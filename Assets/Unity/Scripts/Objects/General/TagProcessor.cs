//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Process Received Tags and update relevant gameObjects.
	// TODO:
	// - Should Objects be place in CPlane or in Object MAnager like robots?
	public class TagProcessor : MonoBehaviour {
		[Tooltip("A prefab of a tag.")]
		public GameObject goPrefabTag;
		// - Tag tag.
		private string tagTag = "labels";
        
		// Decode Received Data.
		public void ProcessTag(TagData receivedTags) {
			GameObject[] goTags = GameObject.FindGameObjectsWithTag(tagTag);
            
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
            
			// Loop through tags received.
			for (int i = 0; i < receivedTags.text.Count; i++) {
				#if DEBUG
				Debug.Log("Tag: total tags found: " + receivedTags.text.Count);
				#endif
				// Ensure tag exists.
				GameObject tagInstance;
				if (i < goTags.Length) { // Update old tag
					#if DEBUG
					Debug.Log("Tag: tag old updating.");
					#endif
					tagInstance = goTags[i];
				} else { // Create new tag
					#if DEBUG
					Debug.Log("Tag: tag new instantiating.");
					#endif
					tagInstance = Instantiate(this.goPrefabTag, ObjectManager.instance.cPlane.transform.position,
					                          ObjectManager.instance.cPlane.transform.rotation,
					                          ObjectManager.instance.cPlane.transform);
				}
                
				// Prepare Values
				Vector3 currentLocalPosition = new Vector3(receivedTags.textLocation[i][0],
				                                           receivedTags.textLocation[i][1],
				                                           receivedTags.textLocation[i][2]);
				Color currentColor = new Color((float)receivedTags.textColor[i][1] / 255.0f,
				                               (float)receivedTags.textColor[i][2] / 255.0f,
				                               (float)receivedTags.textColor[i][3] / 255.0f,
				                               (float)receivedTags.textColor[i][0] / 255.0f);
				TextMeshProUGUI labelText = tagInstance.GetComponent<TextMeshProUGUI>();
                
				// Set Values.
				// - Text Location
				tagInstance.transform.localPosition = currentLocalPosition;
				// - Text
				labelText.text = receivedTags.text[i];
				// - Text Size
				labelText.fontSize = receivedTags.textSize[i];
				// - Text Clor
				labelText.color = currentColor;
			}
            
			// Delete Extraneous Tags.
			if (receivedTags.text.Count < goTags.Length) {
				#if DEBUG
				Debug.Log("Tag: tags extra deleting.");
				#endif
				DeleteTags(receivedTags.text.Count);
			}
		}
		// Delete Tags Starting from Last Index.
		public void DeleteTags(int lastIndex = 0) {
			GameObject[] goTags = GameObject.FindGameObjectsWithTag(this.tagTag);
			for (int i = goTags.Length-1; i >= lastIndex; i--) {
				Destroy(goTags[i]);
			}
			Resources.UnloadUnusedAssets();
		}
	}
}