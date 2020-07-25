//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Counter element, responsible for keeping track of the value.
	public class Counter : MonoBehaviour {
		[Tooltip("UI elements.")]
		public Button P, N;
		[Tooltip("Label text to display value.")]
		public Text label;
		// Value of the counter.
		[HideInInspector]
		public int value = 0;
        
		// Start is called before the first frame update
		void Start() {
			// P.onClick.AddListener(CounterPlus);
			// N.onClick.AddListener(CounterMinus);
		}
		// Update value On UI click.
		public void CounterPlus() {
			CounterChanged(1);
		}
		public void CounterMinus() {
			CounterChanged(-1);
		}
		private void CounterChanged(int shift){
			this.value += shift;
			this.label.text = this.value.ToString();
			#if DEBUG
			Debug.Log("Counter: Value: " + this.value);
			#endif
			// Inform UI Manager.
			ParameterUIMenu.instance.OnValueChanged();
		}
	}
}