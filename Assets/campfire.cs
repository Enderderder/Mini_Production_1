using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class campfire : MonoBehaviour {
    public GameObject[] listofenemys;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

            foreach (GameObject enemy in listofenemys)
            {
                enemy.GetComponent<EnemyMovement>().currHealth = 100;
                enemy.SetActive(true);
            }
        }
    }
}
