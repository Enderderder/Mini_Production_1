using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour, IKillable
{
    [Header("Stats")]
    public float TotalHealth = 500f;
    public float CurrHealth;
    public float AttackDmg = 30f;
    public float Defense = 50f;

    // The player object
    private GameObject m_playerTarget;

    // The Animator
    private Animator m_animator;

    void Awake()
    {
        // Get the animator controller
        m_animator = GetComponentInChildren<Animator>();
    }
	void Start ()
    {
        // Find the target
        m_playerTarget = GameObject.FindGameObjectWithTag("Player");
    }
	void Update ()
    {

    }

    public IEnumerator DeathResult()
    {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("Chris");
    }

    /* Interface Implementation =================================*/

    // IKillable
    public void TakeDamage(float _value)
    {
        CurrHealth -= _value;
    }
    public void CheckDeath()
    {
        if (IsAlive() == false)
        {
            KillEntity();
        }
    }
    public void KillEntity()
    {
        m_animator.SetBool("IsDead", true);
        StartCoroutine(DeathResult());
    }
    public bool IsAlive()
    {
        if (CurrHealth <= 0f)
        {
            return false;
        }
        return true;
    }

    // ============================================================
}
