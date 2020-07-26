using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloFab {
	public class Interactible_Movable : MonoBehaviour {
		public GameObject goTriggerMoveXY;
		public GameObject goTriggerMoveZ;
		public GameObject goTriggerRotateZ;
        
		public enum InteractionType {Inactive, MoveXY, MoveZ, RotateZ}
		// public delegate void OnClickAction(); // Move to Meta Class
        
		[HideInInspector]
		public InteractionType state = InteractionType.Inactive;
		// Flag for starting the actions
		private bool flagDragStart;
		// Extract object orientation. Here so that if orienting by object can be done locally specifically here.
		[HideInInspector]
		public Plane orientationPlane;
		// A point of hit set from the interaction manager.
        
		private Vector3 startPosition;
		// // Internal history of orientation for rotating.
		// private Vector3 lastDragOrientation;
        
		// public OnClickAction OnStartMoveXY;
		// public OnClickAction OnEndMoveXY;
		// public OnClickAction OnStartMoveZ;
		// public OnClickAction OnEndMoveZ;
		// public OnClickAction OnStartRotateZ;
		// public OnClickAction OnEndRotateZ;
        
		////////////////////////////////////////////////////////////////////////
		// Check if given object is the trigger and react.
		public bool CheckTrigger(GameObject goHit){
			if ((this.goTriggerMoveXY != null) && (this.goTriggerMoveXY == goHit)) {
				this.state = InteractionType.MoveXY;
				this.orientationPlane = new Plane(transform.up, transform.position);
			} else if ((this.goTriggerMoveZ != null) && (this.goTriggerMoveZ == goHit)) {
				this.state = InteractionType.MoveZ;
				this.orientationPlane = new Plane(Camera.main.transform.forward, transform.position);
			} else if ((this.goTriggerRotateZ != null) && (this.goTriggerRotateZ == goHit)) {
				this.state = InteractionType.RotateZ;
				this.orientationPlane = new Plane(transform.up, transform.position);
			} else
				return false;
			UpdateAppearance();
			this.flagDragStart = true;
			this.startPosition = transform.position;
			return true;
		}
		// TODO make Prpare Function to do all start stuff
		// If events to trigger animations are set - call them.
		private void UpdateAppearance(){
			// if ((this.state == InteractionType.MoveXY) && (OnStartMoveXY != null))
			// 	OnStartMoveXY();
			// else if ((this.state == InteractionType.Inactive) && (OnEndMoveXY != null))
			// 	OnEndMoveXY();
			// else if ((this.state == InteractionType.MoveZ) && (OnStartMoveZ != null))
			// 	OnStartMoveZ();
			// else if ((this.state == InteractionType.Inactive) && (OnEndMoveZ != null))
			// 	OnEndMoveZ();
			// else if ((this.state == InteractionType.RotateZ) && (OnStartRotateZ != null))
			// 	OnStartRotateZ();
			// else if ((this.state == InteractionType.Inactive) && (OnEndRotateZ != null))
			// 	OnEndRotateZ();
		}
		public void StopMoving(){
			this.state = InteractionType.Inactive;
			UpdateAppearance();
		}
		////////////////////////////////////////////////////////////////////////
		void Update(){
			if (this.state != InteractionType.Inactive) {
				Vector3 dragDifference = InteractionManager.instance.DragMoveDifference(this.flagDragStart);
				switch (this.state) {
				 case InteractionType.MoveXY:
					 MoveXY(dragDifference);
					 break;
				 case InteractionType.MoveZ:
					 MoveZ(dragDifference);
					 break;
				 case InteractionType.RotateZ:
					 float rotateDifference = InteractionManager.instance.DragRotateDifference(this.flagDragStart);
					 // Vector3 currentDragOrientation = currentPlanePosition - transform.position;
					 // if (this.flagDragStart)
					 //  this.lastDragOrientation = currentDragOrientation;
					 RotateZ(rotateDifference);
					 break;
				 default:
					 break;
				}
				this.flagDragStart = false;
			}
		}
		// Update position and rotation per frame.
		private void MoveXY(Vector3 dragDifference){
			float currentZ = transform.position.y;
			transform.position = this.startPosition + dragDifference;
			transform.position = new Vector3(transform.position.x, currentZ, transform.position.z);
		}
		private void MoveZ(Vector3 dragDifference){
			float newY = (this.startPosition + dragDifference).y;
			transform.position = new Vector3(transform.position.x, newY, transform.position.z);
		}
		// private void RotateZ(Vector3 currentDragOrientation){
		// 	Vector3 rt = Quaternion.AngleAxis(1, this.orientationPlane.normal) * this.lastDragOrientation; // a trick to check direction of rotation?
		// 	float a1 = Vector3.Angle(this.lastDragOrientation, currentDragOrientation);
		// 	float a2 = Vector3.Angle(rt, currentDragOrientation);
		// 	if (a2 > a1) a1 *= -1;
		// 	transform.RotateAroundLocal(this.orientationPlane.normal, Mathf.Deg2Rad * a1);
		// 	this.lastDragOrientation = currentDragOrientation;
		// }
		private void RotateZ(float rotateDifference){
			transform.Rotate(this.orientationPlane.normal, rotateDifference);// Mathf.Deg2Rad * 
        }
	}
}