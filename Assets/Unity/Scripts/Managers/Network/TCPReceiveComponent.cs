// #define DEBUG
// #define DEBUG2
#define DEBUGWARNING
#undef DEBUG
#undef DEBUG2
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;
using System.Threading;

/// <summary>
/// Receives Data from MeshStreaming and Positioner Component
/// </summary>


namespace HoloFab {
	// Unity Component Interfacing TCP Receive class.
	public class TCPReceiveComponent : MonoBehaviour {
		[Header("Necessary Variables.")]
		[Tooltip("A port for UDP communication to listen on.")]
		public int localPortOverride = 11111;
        
		// Local Variables.
		private string sourceName = "TCP Receive Component";
		private TCPReceive tcpReceiver;
		// - last interpreted message.
		private string lastMessage = "";
        
		// Unity Functions.
		void OnEnable() {
			if (this.tcpReceiver != null)
				this.tcpReceiver.Disconnect();
			this.tcpReceiver = new TCPReceive(this.localPortOverride);
			this.tcpReceiver.Connect();
		}
		void OnDisable() {
			this.tcpReceiver.Disconnect();
		}
		void Update() {
			// if (!this.tcpReceiver.flagConnectionFound) {
			// 	#if DEBUGWARNING && DEBUG2
			// 	DebugUtilities.UniversalWarning(this.sourceName, "Connection not Found.");
			// 	#endif
			// 	this.tcpReceiver.Connect();
			// 	if (!this.tcpReceiver.flagConnectionFound) return;
			// }
			if (this.tcpReceiver.dataMessages.Count > 0) {
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Parsing input . . .");
				#endif
				// Try to interprete message. If recognized - remove from the queue.
				if (InterpreteData(this.tcpReceiver.dataMessages.Peek())) //[this.tcpReceiver.dataMessages.Count-1]);
					this.tcpReceiver.dataMessages.Dequeue();
			}
		}
		/////////////////////////////////////////////////////////////////////////////
		// A function responsible for decoding and reacting to received TCP data.
		private bool InterpreteData(string message) {
			if (!string.IsNullOrEmpty(message)) {
				message = EncodeUtilities.StripSplitter(message);
				if (this.lastMessage != message) {
					this.lastMessage = message;
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "New message found: " + message);
					#endif
					string[] messageComponents = message.Split(new string[] {EncodeUtilities.headerSplitter}, 2, StringSplitOptions.RemoveEmptyEntries);
					if (messageComponents.Length > 1) {
						string header = messageComponents[0], content = messageComponents[1];
						#if DEBUG
						DebugUtilities.UniversalDebug(this.sourceName, "Header: " + header + ", content: " + content);
						#endif
						if (header == "MESHSTREAMING") {
							InterpreteMesh(content, SourceType.TCP);
							return true;
						} else if (header == "HOLOBOTS") {
							InterpreteHoloBots(content);
							return true;
						} else {
							#if DEBUGWARNING
							DebugUtilities.UniversalWarning(this.sourceName, "Header Not Recognized");
							#endif
						}
					} else {
						#if DEBUGWARNING
						DebugUtilities.UniversalWarning(this.sourceName, "Improper message");
						#endif
					}
				}
			}
			return true;// Since we have one interpreter anyway
		}
		// Functions to interprete and react to determined type of messages: // TODO: Join with UDP interpreters?
		// - Mesh
		private void InterpreteMesh(string data, SourceType meshSourceType){
			ObjectManager.instance.GetComponent<MeshProcessor>().ProcessMesh(EncodeUtilities.InterpreteMesh(data), meshSourceType);
		}
		// - HoloBots
		private void InterpreteHoloBots(string data){
			ObjectManager.instance.GetComponent<RobotProcessor>().ProcessRobot(EncodeUtilities.InterpreteHoloBots(data));
		}
	}
}