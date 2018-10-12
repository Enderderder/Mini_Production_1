using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InteractPopup : MonoBehaviour {

    private Transform playerCamera;

	void OnEnable ()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.4f);
        //Debug.Log("Hello");
    }
	
	void Update ()
    {
        transform.LookAt(playerCamera.position, new Vector3(0, 1, 0));
    }
}
