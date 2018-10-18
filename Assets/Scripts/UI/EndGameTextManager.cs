using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class EndGameTextManager : MonoBehaviour {

    public float LetterPauseTime = 0.03f;
    public Image overlay;
    public Text textLabel;
    public string[] textOrder;

    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            Destroy(GameObject.FindGameObjectWithTag("Player"));
        }
        StartCoroutine(StartText());
    }

    IEnumerator StartText()
    {
        overlay.DOFade(1, 0);

        yield return new WaitForSeconds(2);

        for (int i = 0; i < textOrder.Length; i++)
        {
            overlay.DOFade(0, 0);
            textLabel.text = "";
            foreach (char item in textOrder[i])
            {
                textLabel.text += item;
                yield return new WaitForSeconds(LetterPauseTime);
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            overlay.DOFade(1, 2);
            yield return new WaitForSeconds(2);
        }

        overlay.DOFade(0, 2);
        textLabel.text = "The more you have, the worse it is!";
        yield return new WaitForSeconds(3);
        overlay.DOFade(1, 2);
        yield return new WaitForSeconds(2);

        //Application.Quit();
        SceneManager.LoadScene("MainMenu");
    }
}
