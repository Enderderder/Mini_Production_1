using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuControl : MonoBehaviour
{
	[Header("UI Reference")]
	public GameObject MainMenu;
	public GameObject ControlPanel;
	public GameObject QuitConfirm;

	private bool m_inSubPanel;

	void Start()
	{
		m_inSubPanel = false;
		MainMenu.SetActive(true);
		ControlPanel.SetActive(false);
		QuitConfirm.SetActive(false);
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (m_inSubPanel)
			{
				ResetMenuState();
			}
		}
	}

	/** Member Function */
	public void ResetMenuState()
	{
		m_inSubPanel = false;
		MainMenu.SetActive(true);
		MainMenu.GetComponent<GraphicRaycaster>().enabled = true;
		ControlPanel.SetActive(false);
		QuitConfirm.SetActive(false);
	}

	/** UI Button Functions */
	public void StartGame()
	{
        SceneManager.LoadScene(1);
	}
	public void Controls()
	{
		m_inSubPanel = true;
		MainMenu.GetComponent<GraphicRaycaster>().enabled = false;
		ControlPanel.SetActive(true);
	}
	public void ControlBackToMain()
	{
		ControlPanel.SetActive(false);
		MainMenu.GetComponent<GraphicRaycaster>().enabled = true;
	}
	public void RequestQuitGame()
	{
		m_inSubPanel = true;
		MainMenu.GetComponent<GraphicRaycaster>().enabled = false;
		QuitConfirm.SetActive(true);
	}
	public void ConfirmQuit(bool _confirmed)
	{
		if (_confirmed)
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
		}
		else
		{
			MainMenu.GetComponent<GraphicRaycaster>().enabled = true;
			QuitConfirm.SetActive(false);
		}
	}

}
