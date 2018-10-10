using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour, IKillable
{
    [Header("Stats")]
    public float TotalHealth = 500f;
    public float CurrHealth;
    public float AttackDmg = 30f;
    public float Defense = 50f;

    [Header("Configuration")]
    public float WaitTimeBeforeTailAtk;
    public float WaitTimeAfterTailAtk;

    // The player object
    private GameObject m_playerTarget;

    // The Animator
    private Animator m_animator;
    private NavMeshAgent m_navAgent;

    void Awake()
    {
        // Get the animator controller
        m_animator = GetComponentInChildren<Animator>();

        // Get the navigation agent
        m_navAgent = GetComponent<NavMeshAgent>();
    }
	void Start ()
    {
        // Find the target
        m_playerTarget = GameObject.FindGameObjectWithTag("Player");

        if (m_playerTarget)
        {
            m_navAgent.SetDestination(m_playerTarget.transform.position);
            m_animator.SetBool("IsWalking", true);
        }
    }
	void Update ()
    {
        // Check if the agent has reach the destination
        if (!m_navAgent.pathPending)
        {
            if (m_navAgent.remainingDistance <= m_navAgent.stoppingDistance)
            {
                if (!m_navAgent.hasPath || m_navAgent.velocity.sqrMagnitude == 0.0f)
                {
                    m_animator.SetBool("IsWalking", false);
                }
            }
        }


    }

    public IEnumerator DeathResult()
    {
        yield return new WaitForSeconds(3.0f);
        Debug.Log("The boss has been defeated");

        //SceneManager.LoadScene("Chris");
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
