using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

    public GameObject playerPrefab;

	// Use this for initialization
	void Start ()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            Instantiate(playerPrefab, transform.position, transform.rotation);
        }

        GameObject.FindGameObjectWithTag("Player").transform.position = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
