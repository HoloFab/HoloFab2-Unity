// #define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Generatable UI Controlling manager.
	// TODO:
	// - Move canvases height offset (55) up.
	// - Check if generic version works. If so delete commented code.
	// - Later: Make into more generic list of types with corresponding variables.
	// - Later: Automatically find buttons and canvasses by tags (?)
	public class ParameterUIMenu : MonoBehaviour {
		// Static accessor.
		private static ParameterUIMenu _instance;
		public static ParameterUIMenu instance {
			get {
				if (ParameterUIMenu._instance == null)
					ParameterUIMenu._instance = FindObjectOfType<ParameterUIMenu>();
				return ParameterUIMenu._instance;
			}
		}
        
		// Necessary variables.
		[Header("Variables Set From Scene.")]
		[Tooltip("Buttons to control each type of generatable UI.")]
		public Button buttonBooleanAdder;
		[Tooltip("Buttons to control each type of generatable UI.")]
		public Button buttonCounterAdder;
		[Tooltip("Buttons to control each type of generatable UI.")]
		public Button buttonSliderAdder;
		[Tooltip("Button to delete UI items.")]
		public Button buttonDeleter;
		[Tooltip("Parent Canvases for each type of generatable UI.")]
		public Canvas canvasBooleanToggle, canvasCounter, canvasSlider;
        
		[Header("Variables Set From Prefabs.")]
		[Tooltip("Prefab Prefabs of each type of generatable UI.")]
		public GameObject goPrefabUIBooleanToggle;
		[Tooltip("Prefab Prefabs of each type of generatable UI.")]
		public GameObject goPrefabUICounter;
		[Tooltip("Prefab Prefabs of each type of generatable UI.")]
		public GameObject goPrefabUISlider;
        
		// Secondary Variables with presets.
		[Header("Preset Variables.")]
		[Tooltip("Tags for generatable UI items.")]
		public string tagUIItemBoolean = "Toggle";
		[Tooltip("Tags for generatable UI items.")]
		public string tagUIItemCounter = "Counter";
		[Tooltip("Tags for generatable UI items.")]
		public string tagUIItemSlider = "Slider";
		[Tooltip("Limiting amounts for each type of generatable UI.")]
		public int UILimitCount = 6;
        
		public int initialSize = 110;
		public int maximumSize = 625;
        
		[Header("Adjustable panel from scene")]
		[Tooltip("Adjustable UI panel")]
		public GameObject panel;
		private RectTransform rt;
		private float maxY;
        
		// Local variables.
		// - UI item amounts to keep track.
		private int amountBooleanToggle = 0, amountCounter = 0, amountSlider = 0;
		// Network variables.
		// Stored message to avoid unnecessary traffic.
		private static string lastMessage;
		// Reference to the Sender.
		private static UDPSendComponent sender;
        
		void Start() {
			// Instanses of panel variables
			rt = panel.GetComponent<RectTransform>();
		}
		//////////////////////////////////////////////////////////////////////////
		// Generic UI adding function.
		private void TryAddUIItem(ref int amount, int limit, GameObject goPrefab, Canvas cParent, float height) {
			if (amount < limit) {
				#if DEBUG
				Debug.Log("ParameterUIMenu: Adding new UI Element.");
				#endif
				//determinning the position in Y
				float poseY = amount * height / limit;
                
				//Updating the size of panel
				if (poseY != 0) {
					if (maxY <= poseY) {
						rt.sizeDelta = new Vector2(rt.sizeDelta.x, poseY + initialSize);
						maxY = poseY;
					}
				}
                
				//Adding
				GameObject goUIItem = Instantiate(goPrefab, cParent.gameObject.transform);
				RectTransform rectTransform = goUIItem.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(0, poseY);
				amount++;
                
				// Inform UI Manager.
				ParameterUIMenu.instance.OnValueChanged();
			}
		}
		// Add Boolean Toggle UI item.
		public void TryAddBooleanToggle() {
			TryAddUIItem(ref this.amountBooleanToggle, this.UILimitCount,
			             this.goPrefabUIBooleanToggle, this.canvasBooleanToggle,
			             this.maximumSize);
		}
		// Add Counter UI item.
		public void TryAddCounter() {
			TryAddUIItem(ref this.amountCounter, this.UILimitCount,
			             this.goPrefabUICounter, this.canvasCounter,
			             this.maximumSize);
		}
		// Add Slider UI item.
		public void TryAddSlider() {
			TryAddUIItem(ref this.amountSlider, this.UILimitCount,
			             this.goPrefabUISlider, this.canvasSlider,
			             this.maximumSize);
		}
		// Delete all user generated UIs.
		public void DeleteGeneratedUI() {
			GameObject[] goBooleans = GameObject.FindGameObjectsWithTag(this.tagUIItemBoolean);
			GameObject[] goCounters = GameObject.FindGameObjectsWithTag(this.tagUIItemCounter);
			GameObject[] goSliders = GameObject.FindGameObjectsWithTag(this.tagUIItemSlider);
            
			for (int i = goBooleans.Length-1; i >= 0; i--) DestroyImmediate(goBooleans[i]);
			for (int i = goCounters.Length-1; i >= 0; i--) DestroyImmediate(goCounters[i]);
			for (int i = goSliders.Length-1; i >= 0; i--) DestroyImmediate(goSliders[i]);
			Resources.UnloadUnusedAssets();
            
			this.amountBooleanToggle = 0;
			this.amountCounter = 0;
			this.amountSlider = 0;
            
			// Inform UI Manager.
			ParameterUIMenu.instance.OnValueChanged();
            
			// Setting the Initial size of panel
			rt.sizeDelta = new Vector2(rt.sizeDelta.x, initialSize);
			maxY = initialSize;
		}
        
		//////////////////////////////////////////////////////////////////////////
		// React to a value change.
		public void OnValueChanged() {
			if (UDPReceiveComponent.flagUICommunicationStarted) {
				#if DEBUG
				Debug.Log("ParameterUIMenu: Updating UI values.");
				#endif
				// Find Objects.
				GameObject[] goBooleans = GameObject.FindGameObjectsWithTag(this.tagUIItemBoolean);
				GameObject[] goCounters = GameObject.FindGameObjectsWithTag(this.tagUIItemCounter);
				GameObject[] goSliders = GameObject.FindGameObjectsWithTag(this.tagUIItemSlider);
				#if DEBUG
				Debug.Log("ParameterUIMenu: Found items: booleans: " + goBooleans.Length + ", counters: " + goCounters.Length + ", sliders: " + goSliders.Length);
				#endif
                
				// Extract data.
				List<bool> bools = new List<bool>();
				List<int> ints = new List<int>();
				List<float> floats = new List<float>();
				foreach (GameObject goItem in goBooleans)
					bools.Add(goItem.GetComponent<BooleanToggle>().value);
				foreach (GameObject goItem in goCounters)
					ints.Add(goItem.GetComponent<Counter>().value);
				foreach (GameObject goItem in goSliders)
					floats.Add(goItem.GetComponent<FloatSlider>().value);
				UIData ui = new UIData(bools, ints, floats);
                
				// Encode and if changed - send it.
				byte[] data = EncodeUtilities.EncodeData("UIDATA", ui, out string currentMessage);
				if (ParameterUIMenu.lastMessage != currentMessage) { // TODO: Technically not necessary now since we call directly from UI elements themselves.
					ParameterUIMenu.lastMessage = currentMessage;
					#if DEBUG
					Debug.Log("ParameterUIMenu: values changed, sending: " + currentMessage);
					#endif
                    
					if (ParameterUIMenu.sender == null) ParameterUIMenu.sender = FindObjectOfType<UDPSendComponent>();
					if (ParameterUIMenu.sender == null) {
						#if DEBUGWARNING
						Debug.Log("ParameterUIMenu: No sender Found.");
						#endif
						return;
					}
					ParameterUIMenu.sender.SendUI(data);
				}
			}
		}
	}
}