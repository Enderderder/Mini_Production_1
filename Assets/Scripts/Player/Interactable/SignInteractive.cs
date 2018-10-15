using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SignInteractive : MonoBehaviour {

    public float LetterPauseTime;
    public string[] conversationOrder;

    private int currDialogue;
    private GameObject dialogueBox;
    private Text conversationText;
    private GameObject pressE_UI;

    private void Start()
    {
        pressE_UI = transform.Find("PressE_UI").gameObject;
        pressE_UI.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            pressE_UI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(PopupDialogue(other.gameObject));
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

    IEnumerator PopupDialogue(GameObject player)
    {
        player.GetComponent<PlayerMoveTemp>().enabled = false;
        pressE_UI.SetActive(false);

        dialogueBox.SetActive(true);

        for (int i = 0; i < conversationOrder.Length; i++)
        {
            conversationText.text = "";
            foreach (char item in conversationOrder[i])
            {
                conversationText.text += item;
                yield return new WaitForSeconds(LetterPauseTime);
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        dialogueBox.SetActive(false);

        player.GetComponent<PlayerMoveTemp>().enabled = true;
        pressE_UI.SetActive(true);
    }

    private void Update()
    {
        if (dialogueBox == null)
        {
            dialogueBox = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().dialogueBox;
            conversationText = dialogueBox.transform.Find("ConversationText").GetComponent<Text>();
        }
    }
}
