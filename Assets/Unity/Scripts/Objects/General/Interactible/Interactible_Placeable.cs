using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloFab {
	// A structure to allow the object to snap to the scanned environment.
	public class Interactible_Placeable : MonoBehaviour {
		[Tooltip("If is placing on start.")]
		public bool flagPlacingOnStart = true;
		[Tooltip("If on placement orient as well.")]
		public bool flagOrient = false;
		[Tooltip("A distance to hover at.")]
		public float hoverDistance = 2f;
		[Tooltip("A distance to start snapping at.")]
		public float maxSnapDistance = 4f;

		[Tooltip("An Object click on which triggers placement.")]
		public GameObject goTrigger;

		// Internal variable to keep track of placement.
		private bool flagPlacing;

		public delegate void OnClickAction(); // Move to Meta Class
		public OnClickAction OnStartPlacing;
		public OnClickAction OnEndPlacing;

		// Set initial state.
		void OnEnable(){
			this.flagPlacing = this.flagPlacingOnStart;
			if (this.flagPlacing)
				InteractionManager.instance.activePlaceable = this;
			UpdateAppearance();
            TryPlace();
        }

		////////////////////////////////////////////////////////////////////////
		// Check if given object is the trigger and react.
		public bool CheckTrigger(GameObject goHit){
			if (this.goTrigger == goHit) {
				this.flagPlacing = true;
				UpdateAppearance();
				return true;
			} else
				return false;
		}
		public void ForcePlacement(){
			this.flagPlacing = true;
			InteractionManager.instance.activePlaceable = this;
			UpdateAppearance();
		}
		// If events to trigger animations are set - call them.
		private void UpdateAppearance(){
			if ((this.flagPlacing) && (OnStartPlacing != null))
				OnStartPlacing();
			else if ((!this.flagPlacing) && (OnEndPlacing != null))
				OnEndPlacing();
		}
		// If clicked on mesh - try to snap if the object is currently placing.
		// NB! check the distance if the object is hovering (not on mesh) - ignore click.
		public bool OnTrySnap(){
			if ((this.flagPlacing) && (InteractionManager.instance.flagHitGaze)) {
				float distance = Distance2Camera(InteractionManager.instance.hitGaze.point);
				if (distance < this.maxSnapDistance) {
					this.flagPlacing = false;
					UpdateAppearance();
					return true;
				}
            }
            else {
                DebugUtilities.UserMessage("Try to look at scanned mesh or come closer to it.");
            }
			return false;
		}
		////////////////////////////////////////////////////////////////////////
		void Update(){
			// If placement activated
			if (this.flagPlacing) {
                TryPlace();
			}
		}
        private void TryPlace() {
            float distance = this.hoverDistance;
            Vector3 normal = Vector3.up;
            if (InteractionManager.instance.flagHitGaze)
                if (ObjectManager.instance.CheckEnvironmentObject(InteractionManager.instance.hitGaze.collider.gameObject))
                {
                    distance = Distance2Camera(InteractionManager.instance.hitGaze.point);
                    if (distance < this.maxSnapDistance)
                    {
                        distance = Mathf.Min(distance, this.maxSnapDistance);
                        normal = InteractionManager.instance.hitGaze.normal;
                    }
                    else
                        distance = this.hoverDistance;
                }
            Place(distance, normal);
        }
		// Evaluate distance to the hit.
		private float Distance2Camera(Vector3 point){
			return (Camera.main.transform.position - point).magnitude;
		}
		// Position the object at a given point and orientation.
		private void Place(float distance, Vector3 normal){
			Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * distance;

			transform.position = position;
			if (this.flagOrient)
				transform.localRotation = Quaternion.FromToRotation(transform.up, normal);
		}
	}
}
