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
    private GameObject playerCam;

    private void Awake()
    {
        dialogueBox = GameObject.Find("DialogueBoxPlayer");
        playerCam = GameObject.Find("PlayerCam");
        dialogueBox.SetActive(false);
        conversationText = dialogueBox.transform.Find("ConversationText").GetComponent<Text>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(PopupDialogue(other.gameObject));
        }
    }

    IEnumerator PopupDialogue(GameObject player)
    {
        player.GetComponent<PlayerMoveTemp>().enabled = false;

        Vector3 camPos = playerCam.transform.position;
        Vector3 camRot = playerCam.transform.eulerAngles;
        //if (player.GetComponent<Player>())

        if (transform.childCount > 0)
        {
            playerCam.GetComponent<PlayerCamera>().enabled = false;
            playerCam.transform.DOMove(transform.GetChild(0).position, 1.0f, false);
            playerCam.transform.DORotate(transform.GetChild(0).transform.eulerAngles, 1.0f);
            yield return new WaitForSeconds(2);
        }

        dialogueBox.SetActive(true);

        for (int i = 0; i < conversationOrder.Length; i++)
        {
            conversationText.text = "";
            foreach (char item in conversationOrder[i])
            {
                conversationText.text += item;
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        }

        dialogueBox.SetActive(false);

        if (transform.childCount > 0)
        {
            playerCam.transform.DOMove(camPos, 1.0f, false);
            playerCam.transform.DORotate(camRot, 1.0f);
            yield return new WaitForSeconds(1);
            playerCam.GetComponent<PlayerCamera>().enabled = true;
        }

        player.GetComponent<PlayerMoveTemp>().enabled = true;
        gameObject.SetActive(false);
    }
}
