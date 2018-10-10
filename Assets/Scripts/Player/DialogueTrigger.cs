using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DialogueTrigger : MonoBehaviour {

    public float LetterPauseTime;
    public float sentencePauseTime;
    public string[] conversationOrder;

    private int currDialogue;
    private GameObject dialogueBox;
    private Text conversationText;
    private GameObject PlayerCamera;

    private void Awake()
    {
        dialogueBox = GameObject.Find("DialogueBoxPlayer");
        PlayerCamera = GameObject.Find("PlayerCam");
        dialogueBox.SetActive(false);
        conversationText = dialogueBox.transform.Find("ConversationText").GetComponent<Text>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (transform.childCount > 0) { PlayerCamera.transform.DOMove(transform.GetComponentInChildren<Transform>().position, 1.0f, false); }
            StartCoroutine(PopupDialogue(other.gameObject));
        }
    }

    IEnumerator PopupDialogue(GameObject player)
    {
        player.GetComponent<PlayerMoveTemp>().enabled = false;

        if (player.GetComponent<Player>())
        
        dialogueBox.SetActive(true);

        for (int i = 0; i < conversationOrder.Length; i++)
        {
            conversationText.text = "";
            foreach (char item in conversationOrder[i])
            {
                yield return new WaitForSeconds(LetterPauseTime);
                conversationText.text += item;
            }
            yield return new WaitForSeconds(sentencePauseTime);
        }

        dialogueBox.SetActive(false);
        player.GetComponent<PlayerMoveTemp>().enabled = true;
        gameObject.SetActive(false);
    }
}
