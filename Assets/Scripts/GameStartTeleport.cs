using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class GameStartTeleport : MonoBehaviour {

    public Image fadeBlackImage;

	void Start ()
    {
        fadeBlackImage.DOFade(1, 0);
        fadeBlackImage.DOFade(0, 1);
	}

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(GoToNextScene());
    } 

    IEnumerator GoToNextScene()
    {
        fadeBlackImage.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("ChrisTest");
    }
}
