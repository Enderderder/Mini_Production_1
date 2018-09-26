using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pushplayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject.FindGameObjectWithTag("Player").transform.position = this.transform.position;
	}
	

}
