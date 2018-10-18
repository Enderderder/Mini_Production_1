using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class BossTeleport : MonoBehaviour {

    public GameObject pressE_UI;
    public Image blackFadeImage;
    private GameObject playerCam;

	// Use this for initialization
	void Start () {
        pressE_UI.SetActive(false);
        playerCam = GameObject.FindGameObjectWithTag("MainCamera");

        blackFadeImage.DOFade(1, 0);
        blackFadeImage.DOFade(0, 1);
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
                StartCoroutine(NextScene());
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

    IEnumerator NextScene()
    {
        blackFadeImage.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Boss");
    }
}
