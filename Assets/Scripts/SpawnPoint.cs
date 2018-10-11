using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

    public GameObject playerPrefab;

	// Use this for initialization
	void Start ()
    {
        if (GameObject.Find("TempCharacter") == null)
        {
            Instantiate(playerPrefab, transform.position, transform.rotation);
        }

        GameObject.Find("TempCharacter").transform.position = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
