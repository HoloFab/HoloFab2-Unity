//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
#if WINDOWS_UWP
using Microsoft.MixedReality.Toolkit.UI;
#endif

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Slider UI element, responsible for keeping track of the value.
	public class FloatSlider : MonoBehaviour {
		[Tooltip("Object Holding the Slider.")]
		public GameObject goSlider;
		[Tooltip("Label text to display value.")]
		public Text label;
		// Value of the toggle.
		[HideInInspector]
		public float value=0.0f;
        
		// void Start() {
		// 	// Subscribe Button Event.
		// 	slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
		// }
		// Update value On UI click.
		public void ValueChangeCheck() {
			#if WINDOWS_UWP
			this.value = this.goSlider.GetComponent<PinchSlider>().SliderValue;
			#else
			this.value = this.goSlider.GetComponent<Slider>().value;
			#endif
			this.label.text = this.value.ToString("0.00");
			#if DEBUG
			Debug.Log("Slider: Value: " + this.value);
			#endif
			// Inform UI Manager.
			ParameterUIMenu.instance.OnValueChanged();
		}
	}
}