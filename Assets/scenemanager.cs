using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class scenemanager : MonoBehaviour {

	public void loadmenu()
    {
        SceneManager.LoadScene(0);
        Destroy(transform.root.gameObject);
    }

    public void Quitgame()
    {
        Application.Quit();
    }
}
