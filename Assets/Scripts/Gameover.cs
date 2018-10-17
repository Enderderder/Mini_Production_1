using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gameover : MonoBehaviour {

    public void GotoMain()
    {
        SceneManager.LoadScene(0);
        Destroy(this.transform.root.gameObject);
    }

    public void respawn()
    {
        GameObject.Find("Spawnpoint").GetComponent<SpawnPoint>().playerrespawn();
    }
}
