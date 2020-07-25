//#define DEBUG2
#define DEBUGWARNING
#undef DEBUG2
// #undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;

using HoloFab;
using HoloFab.CustomData;

public class UDPBroadcastComponent : MonoBehaviour {
	// Settings:
	public int remotePort = 8888;
	public float expireTime = 1f;
	public string broadcastMessage = "HelloWorld!";
	// Interanl Objects
	private byte[] requestData;
	private UDPSend udpBroadcaster;
	private IEnumerator broadcastingRoutine;
	private string sourceName = "UDP Broadcasting Component";
    
	float lastTime = 0;
    
	// Start is called before the first frame update
	void OnEnable() {
		this.requestData = Encoding.ASCII.GetBytes(this.broadcastMessage);
		this.udpBroadcaster = new UDPSend(string.Empty, this.remotePort);
		this.broadcastingRoutine = BroadcastingRoutine();
		StartCoroutine(this.broadcastingRoutine);
	}
	void OnDisable() {
		StopCoroutine(this.broadcastingRoutine);
	}
	private IEnumerator BroadcastingRoutine(){
		while (true) {
			#if DEBUG2
			DebugUtilities.UniversalDebug(this.sourceName, "Broadcasting a message: " + this.broadcastMessage);
			#endif
			this.udpBroadcaster.Broadcast(this.requestData);
			if (!this.udpBroadcaster.flagSuccess) {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Couldn't broadcast the message.");
				#endif
			}
			yield return new WaitForSeconds(this.expireTime);
		}
	}
}