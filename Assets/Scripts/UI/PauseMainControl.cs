using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMainControl : MonoBehaviour
{
	[Header("UI Reference")]
	public GameObject PausePanel;
	public GameObject ControlPanel;

	private bool m_isPaused;
	private bool m_inSubPanel;

	void Start ()
	{
		m_isPaused = false;
		m_inSubPanel = false;
		PausePanel.SetActive(false);
		ControlPanel.SetActive(false);
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (m_isPaused && !m_inSubPanel)
			{
				ResumeGame();
			}
			else if (m_isPaused && m_inSubPanel)
			{
				ResetPanelState();
			}
			else
			{
				PauseGame();
			}
		}
	}

	/** Member Functions */
	public void ResetPanelState()
	{
		m_isPaused = true;
		m_inSubPanel = false;
		PausePanel.SetActive(true);
		PausePanel.GetComponent<GraphicRaycaster>().enabled = true;
		ControlPanel.SetActive(false);
	}

	/** UI Button Functions */
	public void PauseGame()
	{
		m_isPaused = true;
		// Set the timeScale to 0 
		Time.timeScale = 0;
		//Show the panel up
		PausePanel.SetActive(true);
	}
	public void ResumeGame()
	{
		m_isPaused = false;
		// De-activate the pause menu
		PausePanel.SetActive(false);
		// Resume the game by setting the time scale back to 1
		Time.timeScale = 1f;
	}
	public void Controls()
	{
		m_inSubPanel = true;
		PausePanel.GetComponent<GraphicRaycaster>().enabled = false;
		ControlPanel.SetActive(true);
	}
	public void ControlBackToPause()
	{
		m_inSubPanel = false;
		ControlPanel.SetActive(false);
		PausePanel.GetComponent<GraphicRaycaster>().enabled = true;
	}
	public void GotoMainMenu()
	{
        // Reset the time scale since moving scene
        Time.timeScale = 1f;
        // Load into the main menu scene
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
	}

}
