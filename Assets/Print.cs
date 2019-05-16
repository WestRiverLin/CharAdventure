using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestTool{

	public class Print : MonoBehaviour {

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void DebugOutput(string str1="none",string str2="none",string str3="none"){
			Debug.Log(str1+","+str2+","+str3);
		}
	}

}
