using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour, IKillable
{
    [Header("Stats")]
    public float TotalHealth = 500f;
    public float CurrHealth;
    public float TailAttackDmg = 30f;
    public float Defense = 50f;

    [Header("Configuration")]
    [Header("Alert")]
    public float AlertRange;
    [Header("Tail Attack")]
    public float TailAttackRange;
    public float WaitTimeBeforeTailAtk;
    public float WaitTimeAfterTailAtk;
    [Header("Laser Attack")]
    public float AimingTime;

    [Header("Reference")]
    public GameObject TailAttackOrigin;
    public GameObject RangeIndication;

    // The player object
    private GameObject m_playerTarget;

    // The Animator
    private Animator m_animator;
    private NavMeshAgent m_navAgent;

    // Flags
    /************************************************************************/

    private bool m_actionLock;
    private bool m_hasAlerted;

    /************************************************************************/
    
    void Awake()
    {
        // Get the animator controller
        m_animator = GetComponentInChildren<Animator>();

        // Get the navigation agent
        m_navAgent = GetComponent<NavMeshAgent>();
    }
	void Start ()
    {
        // Free the action lock
        m_actionLock = false;

        // Start with un-alert
        m_hasAlerted = false;

        // Set the health to full at the beginning
        CurrHealth = TotalHealth;

        // Find the target
        m_playerTarget = GameObject.FindGameObjectWithTag("Player");

        if (m_playerTarget)
        {
            if (m_actionLock == false)
            {
                StartCoroutine(TailAttackMove());
            }
            

            //m_navAgent.SetDestination(m_playerTarget.transform.position);
            //m_animator.SetBool("IsWalking", true);
        }
    }
	void Update ()
    {
        if (!m_hasAlerted && DistanceToPlayer() <= AlertRange)
        {
            m_hasAlerted = true;
        }

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

    /************************************************************************/

    /*
     *	Check if the player is inside the tail attack range
     *	the range will be a percentage of the attck range
     *	this will make sure player needs to move far away enough
     */
    public bool InTailAttackRange()
    {
        // Check the distance between medusa and the player
        if (DistanceToPlayer() <= TailAttackRange * 0.8f)
        {
            return true;
        }
        return false;
    }

    /*
     *	Rotate the facing of medusa towards the player constantly
     */
    public IEnumerator RotateTowardsPlayer()
    {
        while(true)
        {
            // Get the direction that needs to be face
            Vector3 rotateDirection =
                m_playerTarget.transform.position - this.transform.position;

            // Get the quaternion rotaion result
            Quaternion destinationRotation = Quaternion.LookRotation(rotateDirection);

            // Set the object rotation
            this.transform.rotation = destinationRotation;

            yield return null;
        }
    }

    /*
     *	Start to get close range to the player and
     *	do the tail slam
     */
    public IEnumerator TailAttackMove()
    {
        // Lock the action
        m_actionLock = true;

        // Get to the close range of player
        yield return StartCoroutine(ChasePlayer());

        // While standing still, keep looking at the player
        Coroutine rotationCoroutine = StartCoroutine(RotateTowardsPlayer());

        // Draw the alert indication of the tail attack
        DrawCircleRange(TailAttackRange);

        // Wait certain amount of time
        yield return new WaitForSeconds(WaitTimeBeforeTailAtk);

        // Undraw the indication as the attack happens
        UndrawCircleRange();

        // Stop rotating as about to attack
        StopCoroutine(rotationCoroutine);

        // Playe the attack animation
        m_animator.SetTrigger("TailAttack");

        // Create a box cast
        RaycastHit[] tailAttackHits;
        Vector3 boxBound = new Vector3(TailAttackRange, 1.0f, TailAttackRange);
        tailAttackHits = Physics.SphereCastAll(TailAttackOrigin.transform.position, TailAttackRange, Vector3.forward, 0.0f);
        //tailAttackHits = Physics.BoxCastAll(TailAttackOrigin.transform.position, boxBound, Vector3.forward, Quaternion.identity, 0.0f);

        // Check if it hits the player
        foreach (RaycastHit hitInfo in tailAttackHits)
        {
            GameObject hitObj = hitInfo.collider.gameObject;
            if (hitObj.tag == "Player")
            {
                // Apply the damage
                hitObj.GetComponent<Player>().TakeDamage(TailAttackDmg);
            }
        }

        // Wait certain amount of time
        yield return new WaitForSeconds(WaitTimeAfterTailAtk);

        // Free the action lock as this action has finished
        m_actionLock = true;
    }

    /*
     *	Stop moving and do the laser attack
     */
     public IEnumerator LaserAttackMove()
    {
        // Keep rotate towards player to aim
        Coroutine rotationCoroutine = StartCoroutine(RotateTowardsPlayer());

        // Wait for the time to aim
        yield return new WaitForSeconds(AimingTime);

        // Stop aiming as about to fire
        StopCoroutine(rotationCoroutine);




        yield return null;
    }

    /*
     *	Keep moving towards the plauer target while 
     *	not close enough to the player
     */
    public IEnumerator ChasePlayer()
    {
        // Starts walking
        m_animator.SetBool("IsWalking", true);

        // Check the distance
        while (!InTailAttackRange())
        {
            // Constantly check the player position and move towards it
            m_navAgent.SetDestination(m_playerTarget.transform.position);

            yield return null;
        }

        // When reach the target, stop
        m_navAgent.SetDestination(this.transform.position);
        m_animator.SetBool("IsWalking", false);
    }

    /*
     *	Do the death thing
     */
    public IEnumerator DeathResult()
    {
        

        //m_animator.SetBool("IsDead", true);

        yield return new WaitForSeconds(2.0f);
        Debug.Log("The boss has been defeated");

        //SceneManager.LoadScene("Chris");
    }

    /*
     *	Draws the range indication
     */
    private void DrawCircleRange(float _range)
    {
        LineRenderer rangeCircle = RangeIndication.GetComponent<LineRenderer>();

        rangeCircle.positionCount = 50 + 1;
        rangeCircle.useWorldSpace = false;

        float x;
        float z;

        float angle = 20f;

        for (int i = 0; i < (50 + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * _range;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * _range;

            rangeCircle.SetPosition(i, new Vector3(x, 0, z));

            angle += (360f / 50);
        }
    }

    /*
     *	Clear the range indication
     */
    private void UndrawCircleRange()
    {
        LineRenderer circleToBeClear = RangeIndication.GetComponent<LineRenderer>();

        circleToBeClear.positionCount = 0;
    }

    /*
     *	Get the distance to the player target
     *	(Only if the player exist)
     */
    private float DistanceToPlayer()
    {
        if (m_playerTarget)
        {
            return Vector3.Distance(this.transform.position, m_playerTarget.transform.position);
        }
        else
        {
            Debug.Log("Unable to get the distance to player as the player don't exist");
            return 0.0f;
        }
    }

    /* Interface Implementation =================================*/

    // IKillable
    public void TakeDamage(float _value)
    {
        CurrHealth -= _value;
        CheckDeath();
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
        // Stop the behaviour thats happening
        StopAllCoroutines();

        // Start the a
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
