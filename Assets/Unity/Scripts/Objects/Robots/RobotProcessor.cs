//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Process Received Robots and update relevant gameObjects.
	public class RobotProcessor : MonoBehaviour {
		[Tooltip("A dictionary of supported robots - accessible from Unity directly.")]
		public List<UnityRobot> robotDictionary = new List<UnityRobot>();
		// Dictionary not serializable in unity. Otherwise better to have them as dictionary of name to robotexample
        
		// - Tool tag
		private string tagTool = "tool_Object";
		// - Keep track of robots by ids.
		[HideInInspector]
		public Dictionary<int, GameObject> robotsInstantiated = new Dictionary<int, GameObject>();
        
		// Decode Received Data.
		public void ProcessRobot(List<RobotData> receivedRobots) {
			#if DEBUG
			Debug.Log("Robot: got robots: " + receivedRobots.Count);
			#endif
            
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
            
			// Loop through all received robots.
			for (int i = 0; i < receivedRobots.Count; i++) {
				int robotID = receivedRobots[i].robotID;
				string bot = receivedRobots[i].robotName;
				double[] basePlane = receivedRobots[i].robotPlane;
				EndeffectorData endEffector = receivedRobots[i].endEffector;
                
				#if DEBUG
				Debug.Log("Robot: " + bot);
				#endif
				bool flagFound = false;
				foreach(UnityRobot unityRobot in this.robotDictionary) {
					if (bot == unityRobot.name) {
						ProcessHoloBot(unityRobot.tag, unityRobot.goExample, basePlane, endEffector, robotID);
						flagFound = true;
						break;
					}
				}
				if (!flagFound) {
					#if DEBUG
					Debug.Log("Robot: robot not recognized");
					#endif
				}
			}
		}
		private void ProcessHoloBot(string tag, GameObject goPrefab, double[] basePlane, EndeffectorData endEffector, int robotID) {
			GameObject goHoloBot;
			//GameObject goHoloBot = GameObject.FindGameObjectWithTag(tag);
			// If HoloBot not found - add it.
			//if (goHoloBot == null) {
			//	#if DEBUG
			//	Debug.Log("Robot: robot doesn't exist. Creating.");
			//	#endif
			//	goHoloBot = CreateBot(ObjectManager.instance.cPlane, goPrefab, endEffector, robotID);
			//}
			if (!this.robotsInstantiated.ContainsKey(robotID)) {
				goHoloBot = CreateBot(ObjectManager.instance.cPlane, goPrefab, endEffector, robotID);
				this.robotsInstantiated.Add(robotID, goHoloBot);
			} else {
				goHoloBot = this.robotsInstantiated[robotID];
				if (goHoloBot.tag != tag) {
					DestroyImmediate(goHoloBot);
					goHoloBot = CreateBot(ObjectManager.instance.cPlane, goPrefab, endEffector, robotID);
					this.robotsInstantiated[robotID] = goHoloBot;
				}
			}
			// Update HoloBot transform.
			goHoloBot.transform.SetPositionAndRotation(ObjectManager.instance.cPlane.transform.position + new Vector3((float)basePlane[0],
			                                                                                                          (float)basePlane[1],
			                                                                                                          (float)basePlane[2]),
			                                           ObjectManager.instance.cPlane.transform.rotation * new Quaternion(-(float)basePlane[5],
			                                                                                                             (float)basePlane[6],
			                                                                                                             (float)basePlane[4],
			                                                                                                             (float)basePlane[3]));
		}
        
		private GameObject CreateBot(GameObject cPlane, GameObject goPrefab, EndeffectorData endEffector, int robotID) { // int port, double[] tcp
			#if DEBUG
			Debug.Log("Robot: Instantiating");
			#endif
			GameObject goHoloBot = Instantiate(goPrefab, ObjectManager.instance.cPlane.transform.position, ObjectManager.instance.cPlane.transform.rotation, ObjectManager.instance.cPlane.transform);
			goHoloBot.GetComponentInChildren<RobotController>().robotID = robotID;
            
			foreach (MeshFilter meshFilter in goHoloBot.GetComponentsInChildren<MeshFilter>()) {
				if (meshFilter.gameObject.tag == this.tagTool) {
					#if DEBUG
					Debug.Log("Robot: Found Tool");
					#endif
					CreateMesh(endEffector, meshFilter);
				}
			}
			return goHoloBot;
		}
        
		private void CreateMesh(EndeffectorData endEffector, MeshFilter tool) { //, double[] tcp
			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
            
			if (endEffector.vertices != null) {
				#if DEBUG
				Debug.Log("Robot: Adding Verticies . . . !");
				#endif
				for (int j = 0; j < endEffector.vertices.Count; j++) {
					vertices.Add(new Vector3(endEffector.vertices[j][0],
					                         endEffector.vertices[j][1],
					                         endEffector.vertices[j][2]));
				}
			}
            
			if (endEffector.faces != null) {
				#if DEBUG
				Debug.Log("Robot: Adding Faces . . . !");
				#endif
				for (int j = 0; j < endEffector.faces.Count; j++) {
					triangles.Add(endEffector.faces[j][1]);
					triangles.Add(endEffector.faces[j][2]);
					triangles.Add(endEffector.faces[j][3]);
                    
					if ((endEffector.faces[j][0] == 1)) {
						triangles.Add(endEffector.faces[j][1]);
						triangles.Add(endEffector.faces[j][3]);
						triangles.Add(endEffector.faces[j][4]);
					}
				}
			}
			tool.mesh = MeshUtilities.DecodeMesh(vertices, triangles);
			tool.transform.Rotate(0, 180, -90);
		}
        
		public void DeleteRobots() {
			List<GameObject> robots = this.robotsInstantiated.Values.ToList();
			for (int i = robots.Count - 1; i >=0; i--) {
				DestroyImmediate(robots[i]);
			}
			Resources.UnloadUnusedAssets();
		}
	}
}