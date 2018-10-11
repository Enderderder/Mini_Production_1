using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class BossTeleport : MonoBehaviour {

    public GameObject pressE_UI;
    private GameObject playerCam;

	// Use this for initialization
	void Start () {
        pressE_UI.SetActive(false);
        playerCam = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {


        if (other.tag == "Player")
        {
            pressE_UI.SetActive(true);
            pressE_UI.transform.LookAt(playerCam.transform.position);

            if (Input.GetKeyDown(KeyCode.E))
            {
                SceneManager.LoadScene("Boss");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            pressE_UI.SetActive(false);
        }
    }
}
