//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

namespace HoloFab {
    [RequireComponent(typeof(RectTransform))]
    public class SwipeUI : MonoBehaviour, IDragHandler, IEndDragHandler {
        [Tooltip("Percentage of horizontal swipe to trigger action.")]
        [Range(0, 1.00f)]
        public float swipePercentageThreshod = 0.2f;
        [Tooltip("Duration of swipe animation.")]
        public float easingDuration = 0.5f;
        [Tooltip("State to be in on the start (true - open, false - closed).")]
        public bool flagStartState = false;
        [Tooltip("Direction of swiping (true - right, false - left).")]
        public bool flagSwipeRight = false;
        [Tooltip("Screen Width percentage to be left in sight in closed state.")]
        [Range(0, 1.00f)]
        public float closedStateRemainder = .05f;
        [Tooltip("Distance to drag before activating dragging.")]
        public float startDraggingThreshold = 50f;

        // Local State Variables:
        // - current state to track.
        private bool flagCurrentState = false;
        // - location on game start to be used as start position.
        private Vector3 panelOpenLocation;
        // - UI's pivot X position, necessary for calculating position in closed state.
        private float UIPivotX;
        // - UI's width, necessary for calculating position in closed state.
        private float UIWidth;
        // - dragging activated
        private bool flagDragging = false;
        private Vector3 dragStartLocation;
        private float correction = 0;

        void Start() {
            // Store current position to be used in open state.
            this.panelOpenLocation = transform.position;
            // Extract UI data, necessary for calculating position in closed state
            RectTransform rectTransform = GetComponent<RectTransform>();
            this.UIPivotX = rectTransform.pivot.x;
            this.UIWidth = rectTransform.rect.width;
            // Force initial state.
            this.flagCurrentState = this.flagStartState;
            EnforceState(true);
        }
        // Function triggered during the drag.
        public void OnDrag(PointerEventData data) {
            Vector2 difference = data.pressPosition - data.position;
            if ((!this.flagDragging) && (Mathf.Abs(difference.magnitude) > this.startDraggingThreshold)) {
                this.flagDragging = true;
                this.dragStartLocation = transform.position;
                this.correction = -difference.x;
            }
            if (this.flagDragging) {
                // Update current position.
                transform.position = this.dragStartLocation - new Vector3(difference.x+this.correction, 0, 0);
            }
        }
        // Function triggered on end of the drag.
        public void OnEndDrag(PointerEventData data) {
            // Check if action is triggered and update the state or restore it.
            float swipePercentage = (data.pressPosition.x - data.position.x) / Screen.width;
            if (swipePercentage >= this.swipePercentageThreshod) {
                #if DEBUG
                Debug.Log("Swipe: Right to Left");
                #endif
                this.flagCurrentState = this.flagSwipeRight;
            }
            else if (swipePercentage <= -this.swipePercentageThreshod) { 
                #if DEBUG
                Debug.Log("Swipe: Left to Right");
                #endif
                this.flagCurrentState = !this.flagSwipeRight;
            }
            EnforceState();

            this.flagDragging = false;
        }
        // Enforce the state.
        private void EnforceState(bool flagForce=false) {
            Vector3 targetLocation;
            if (this.flagCurrentState == true)
                targetLocation = this.panelOpenLocation;
            else {
                float x = (this.flagSwipeRight) ? Screen.width * (1 - this.closedStateRemainder) : Screen.width * this.closedStateRemainder - this.UIWidth;
                targetLocation = new Vector3(x + (this.UIPivotX * this.UIWidth), transform.position.y, transform.position.z);
            }
            float duration = (flagForce) ? 0.000001f : this.easingDuration;
            StartCoroutine(SmoothMove(transform.position, targetLocation, duration));
        }
        // Start moving the UI.
        IEnumerator SmoothMove(Vector3 startPosition, Vector3 endPosition, float totalDuration) {
            float animationFactor = 0f;
            while (animationFactor <= 1.0) {
                animationFactor += Time.deltaTime / totalDuration;
                transform.position = Vector3.Lerp(startPosition, endPosition, Mathf.SmoothStep(0f, 1f, animationFactor));
                yield return null;
            }
        }
    }
}