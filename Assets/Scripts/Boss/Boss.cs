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
    public float LaserChargeTime;
    public float RecoverTimeAfterLaser;

    [Header("Reference")]
    public GameObject TailAttackOrigin;
    public GameObject RangeIndication;
    public GameObject LaserIndicationOrigin;

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
    }
	void Update ()
    {
        if (!m_hasAlerted && DistanceToPlayer() <= AlertRange)
        {
            m_hasAlerted = true;
        }

        // Check if the player exist
        if (m_playerTarget)
        {
            if (m_actionLock == false && m_hasAlerted)
            {
                StartCoroutine(LaserAttackMove());
            }
        }
        else
        {
            // Try to find the player again
            m_playerTarget = GameObject.FindGameObjectWithTag("Player");
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

        // Check if it hits the player
        foreach (RaycastHit hitInfo in tailAttackHits)
        {
            GameObject hitObj = hitInfo.collider.gameObject;
            if (hitObj.tag == "Player")
            {
                // Apply the damage
                hitObj.GetComponentInParent<Player>().TakeDamage(TailAttackDmg);
                break;
            }
        }

        // Wait certain amount of time
        yield return new WaitForSeconds(WaitTimeAfterTailAtk);

        // Free the action lock as this action has finished
        m_actionLock = false;
    }

    /*
     *	Stop moving and do the laser attack
     */
     public IEnumerator LaserAttackMove()
    {
        // Lock any action
        m_actionLock = true;

        // Keep rotate towards player to aim
        Coroutine rotationCoroutine = StartCoroutine(RotateTowardsPlayer());

        // Start the aiming
        Coroutine aimingCoroutine = StartCoroutine(AimingLaser());

        // Wait for the time to aim
        yield return new WaitForSeconds(AimingTime);

        // Stop aiming as about to fire
        StopCoroutine(aimingCoroutine);
        StopCoroutine(rotationCoroutine);

        // Charge up the laser
        yield return new WaitForSeconds(LaserChargeTime);

        // Play the animation and undraw the indication asap
        m_animator.SetTrigger("LaserAttack");
        UndrawLaserIndication();

        // Wait for the recovery of shooting the laser
        yield return new WaitForSeconds(RecoverTimeAfterLaser);

        // Free the action lock as this action has finished
        m_actionLock = false;
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

    public IEnumerator AimingLaser()
    {
        while (true)
        {
            DrawLaserIndication(m_playerTarget.transform.position);

            yield return null;
        }
    }

    /*
     *	Do the death thing
     */
    public IEnumerator DeathResult()
    {
        // Stop the navigation function of the medusa
        m_navAgent.isStopped = true;

        // Play the death animation
        m_animator.SetTrigger("Death");

        // Give some time for the animation to play as
        // well as slow down the pace
        yield return new WaitForSeconds(4.0f);

        Debug.Log("The boss has been defeated");
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
     *	Draw the laser indication line towards a point
     *	in the world
     */
    private void DrawLaserIndication(Vector3 _targetLocation)
    {
        // Get the component
        LineRenderer laserLine = LaserIndicationOrigin.GetComponent<LineRenderer>();

        laserLine.positionCount = 2;

        laserLine.SetPosition(0, LaserIndicationOrigin.transform.position);
        laserLine.SetPosition(1, _targetLocation);
    }

    /*
     *	Clear the laser indication
     */
    private void UndrawLaserIndication()
    {
        LineRenderer laserLine = LaserIndicationOrigin.GetComponent<LineRenderer>();

        laserLine.positionCount = 0;
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

        // Start the death action
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
