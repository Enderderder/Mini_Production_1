using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InteractPopup : MonoBehaviour {

    private Transform playerCamera;

	void OnEnable ()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        transform.DOPunchScale(new Vector3(0.06f, 0.06f, 0), 0.4f);
    }
	
	void Update ()
    {
        transform.LookAt(playerCamera.position, new Vector3(0, 1, 0));
    }
}
