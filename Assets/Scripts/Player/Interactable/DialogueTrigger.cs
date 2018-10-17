using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DialogueTrigger : MonoBehaviour {

    public float LetterPauseTime;
    public string[] nameOrder;
    public string[] conversationOrder;

    private int currDialogue;
    private GameObject dialogueBox;
    private Text nameText;
    private Text conversationText;
    private GameObject playerCam;

    private void Awake()
    {
        playerCam = GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player)
        {
            dialogueBox = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().dialogueBox;
            conversationText = dialogueBox.transform.Find("ConversationText").GetComponent<Text>();
            nameText = dialogueBox.transform.Find("NameText").GetComponent<Text>();
            dialogueBox.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            dialogueBox = other.gameObject.GetComponent<Player>().dialogueBox;
            conversationText = dialogueBox.transform.Find("ConversationText").GetComponent<Text>();
            nameText = dialogueBox.transform.Find("NameText").GetComponent<Text>();
            dialogueBox.SetActive(false);
            StartCoroutine(PopupDialogue(other.gameObject));
        }
    }

    IEnumerator PopupDialogue(GameObject player)
    {
        player.GetComponent<PlayerMoveTemp>().enabled = false;

        Vector3 camPos = playerCam.transform.position;
        Vector3 camRot = playerCam.transform.eulerAngles;

        List<Transform> childrenTransforms = new List<Transform>();

        foreach(Transform child in transform)
        {
            childrenTransforms.Add(child);
        }

        for (int i = 0; i < conversationOrder.Length; i++)
        {
            if (transform.childCount >= i + 1)
            {
                playerCam.GetComponent<PlayerCamera>().enabled = false;
                playerCam.transform.DOMove(childrenTransforms[i].position, 1.0f, false);
                playerCam.transform.DORotate(childrenTransforms[i].eulerAngles, 1.0f);
                yield return new WaitForSeconds(1.5f);
            }

            dialogueBox.SetActive(true);

            if (nameOrder.Length > i)
            {
                nameText.text = nameOrder[i] + ":";
            }

            conversationText.text = "";
            foreach (char item in conversationOrder[i])
            {
                conversationText.text += item;
                yield return new WaitForSeconds(LetterPauseTime);
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            dialogueBox.SetActive(false);
        }

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

    private void Update()
    {
        if (dialogueBox == null)
        {
            dialogueBox = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().dialogueBox;
            conversationText = dialogueBox.transform.Find("ConversationText").GetComponent<Text>();
            nameText = dialogueBox.transform.Find("NameText").GetComponent<Text>();
            dialogueBox.SetActive(false);
        }
    }
}
