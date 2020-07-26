///////////////// Debug Flag /////////////////
#define DEBUG
// #undef DEBUG

using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_DebugWindow : MonoBehaviour {
	#if DEBUG
	public TextMeshProUGUI label;
	public bool flagRolling = true;
    
	public List <string> logs;
	private int size = 50;
	private int i = 0;
    
	void OnEnable(){
		//this.label = GetComponent<TextMeshProUGUI>();
		Application.logMessageReceivedThreaded += LogMessage;
		this.logs = new List<string>(new string[this.size]);
		this.i = 0;
	}
	void OnDisable(){
		Application.logMessageReceivedThreaded -= LogMessage;//logMessageReceived
	}
	public void LogMessage(string logString, string stackTrace, LogType type){
		logString += "\n";
		if (this.flagRolling)
			this.logs.Insert(0, logString);
		else
			this.logs.Insert(this.i, logString);
		if (this.logs.Count >= this.size)
			this.logs.RemoveAt(this.size);
		this.label.text = string.Join("\n", this.logs.ToArray());
		this.i = (this.i + 1) % this.size;
	}
	#endif
}