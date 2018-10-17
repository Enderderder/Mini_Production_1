using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class campfire : MonoBehaviour {
    public GameObject listofenemys;
    public EnemyMovement[] enemys;
    public GameObject presseui;
	// Use this for initialization
	void Start () {
        enemys = listofenemys.GetComponentsInChildren<EnemyMovement>();
        presseui.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            presseui.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E)) {
                other.GetComponent<Player>().CurrHealth = 100;
                other.GetComponent<Player>().UpdateHealthBar();
                foreach (EnemyMovement enemy in enemys)
                {
                    enemy.GetComponent<EnemyMovement>().currHealth = 100;
                    enemy.gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            presseui.SetActive(false);
        }
    }
}
