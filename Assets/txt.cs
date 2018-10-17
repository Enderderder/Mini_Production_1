using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class txt : MonoBehaviour {
    private Transform playerCam;
    // Use this for initialization
    void Start () {
        playerCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }
	
	// Update is called once per frame
	void Update () {
        this.transform.position = new Vector3(this.transform.position.x,this.transform.position.y + 1 * Time.deltaTime,this.transform.position.z);
        this.transform.LookAt(playerCam.position);
    }
}
