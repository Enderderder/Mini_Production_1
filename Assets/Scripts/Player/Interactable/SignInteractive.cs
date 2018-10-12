using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignInteractive : MonoBehaviour {

    public float LetterPauseTime;
    public string[] conversationOrder;

    private int currDialogue;
    private GameObject dialogueBox;
    private Text conversationText;
    private GameObject pressE_UI;
    private Transform mainCamera;

    private void Awake()
    {
        pressE_UI = transform.Find("PressE_UI").gameObject;
        pressE_UI.SetActive(false);
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (pressE_UI.Awake == false)
            {
                pressE_UI.SetActive(true);
            }

            pressE_UI.transform.LookAt(mainCamera.position, new Vector3(0, 1, 0));
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
            dialogueBox = GameObject.Find("DialogueBoxPlayer");
            conversationText = dialogueBox.transform.Find("ConversationText").GetComponent<Text>();
            dialogueBox.SetActive(false);
        }
    }
}
