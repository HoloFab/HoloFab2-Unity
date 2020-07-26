//#define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Unity Component Interfacing UDP Receive class.
	public class UDPReceiveComponent : MonoBehaviour {
		[Header("Necessary Variables.")]
		[Tooltip("A port for UDP communication to listen on.")]
		public int localPortOverride = 12121;
        
		// Local Variables.
		private string sourceName = "UDP Receive Component";
		private UDPReceive udpReceiver;
		// - last interpreted message.
		private string lastMessage = "";
		// - IP Address received.
		[HideInInspector]
		public static bool flagUICommunicationStarted = false;
        
		// Unity Functions.
		void OnEnable() {
			this.udpReceiver = new UDPReceive(this.localPortOverride);
			this.udpReceiver.Connect();
		}
		void OnDisable() {
			this.udpReceiver.Disconnect();
		}
		void Update() {
			if (this.udpReceiver.dataMessages.Count > 0) {
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Parsing input . . .");
				#endif
				// Try to interprete message. If recognized - remove from the queue.
				if (InterpreteData(this.udpReceiver.dataMessages.Peek())) //[this.udpReceiver.dataMessages.Count-1]);
					this.udpReceiver.dataMessages.Dequeue();
			}
		}
		/////////////////////////////////////////////////////////////////////////////
		// A function responsible for decoding and reacting to received UDP data.
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
							InterpreteMesh(content, SourceType.UDP);
							return true;
						} else if (header == "CONTROLLER") {
							InterpreteRobotController(content);
							return true;
						} else if (header == "HOLOTAG") {
							InterpreteLabel(content);
							return true;
						} else if(header == "IPADDRESS") {
							InterpreteIPAddress(content);
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
		// Functions to interprete and react to determined type of messages:
		// - Mesh
		private void InterpreteMesh(string data, SourceType meshSourceType){
			ObjectManager.instance.GetComponent<MeshProcessor>().ProcessMesh(EncodeUtilities.InterpreteMesh(data), meshSourceType);
		}
		// - RobotControllers
		private void InterpreteRobotController(string data){
			List<RobotControllerData> controllersData = EncodeUtilities.InterpreteRobotController(data);
            
			RobotProcessor processor = ObjectManager.instance.GetComponent<RobotProcessor>();
			foreach (RobotControllerData controllerData in controllersData)
				if(processor.robotsInstantiated.ContainsKey(controllerData.robotID))
					processor.robotsInstantiated[controllerData.robotID].GetComponentInChildren<RobotController>().ProcessRobotController(controllerData);
		}
		// - Tag
		private void InterpreteLabel(string data){
			ObjectManager.instance.GetComponent<LabelProcessor>().ProcessTag(EncodeUtilities.InterpreteLabel(data));
		}
		// - IP address
		private void InterpreteIPAddress(string data){
			string remoteIP = EncodeUtilities.InterpreteIPAddress(data);
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Remote IP: " + remoteIP);
			#endif
			// TODO: Add ip integrity check
			// TODO: Should not be stored in udp sender.
			UDPSendComponent sender = gameObject.GetComponent<UDPSendComponent>();
			sender.remoteIP = remoteIP;
			UDPReceiveComponent.flagUICommunicationStarted = true;
			// Inform UI Manager.
			ParameterUIMenu.instance.OnValueChanged();
		}
	}
}