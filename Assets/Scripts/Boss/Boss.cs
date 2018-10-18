using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
    [Header("Taunting")]
    public float TauntingTime;
    [Header("Tail Attack")]
    public float TailAttackRange;
    public float WaitTimeBeforeTailAtk;
    public float WaitTimeAfterTailAtk;
    [Header("Laser Attack")]
    public float AimingTime;
    public float LaserAtkDmgPerTick;
    public float LaserDuration;
    public float RecoverTimeAfterLaser;

    [Header("Reference")]
    public GameObject TailAttackOrigin;
    public GameObject RangeIndication;
    public GameObject LaserIndicationOrigin;
    public GameObject LaserParticle;
    public GameObject HealthBar;
    public GameObject TailAttackSound;

    // The player object
    private GameObject m_playerTarget;

    // Laser attack standing location
    private GameObject m_laserAtkStandingLocation;

    // The light of the portal
    private GameObject m_portalLight;

    // The Animator
    private Animator m_animator;
    private NavMeshAgent m_navAgent;

    // Flags
    /************************************************************************/

    private bool m_actionLock;
    private bool m_hasAlerted;
    private bool m_hasDeadTaunted;

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

        m_hasDeadTaunted = false;

        // Set the health to full at the beginning
        CurrHealth = TotalHealth;

        // Find the target
        m_playerTarget = GameObject.FindGameObjectWithTag("Player");

        // Get the standing location for when shooting laser
        m_laserAtkStandingLocation = GameObject.Find("BossLaserStandingLocation");

        // Get the portal light
        m_portalLight = GameObject.Find("PortalLight");
        m_portalLight.SetActive(false);

        UpdateHealthBar();
    }
	void Update ()
    {
        // Check if the player exists
        if (m_playerTarget == null)
        {
            // Try to find the player again
            m_playerTarget = GameObject.FindGameObjectWithTag("Player");
            return;
        }

        // Check the alert detection
        if (!m_hasAlerted && DistanceToPlayer() <= AlertRange)
        {
            m_hasAlerted = true;

            // Taunts the player when alerted
            StartCoroutine(Taunting());
        }

        // Check if the player is alive
        if (m_playerTarget.GetComponent<Player>().IsAlive())
        {
            if (m_actionLock == false && m_hasAlerted)
            {
                if (CurrHealth > TotalHealth * 0.50)
                {
                    int randNum = Random.Range(0, 5);

                    if (randNum > 1)
                    {
                        StartCoroutine(TailAttackMove());
                    }
                    else
                    {
                        StartCoroutine(Taunting());
                    }
                }
                else if (CurrHealth > 0)
                {
                    int randNum = Random.Range(0, 10);

                    if (randNum > 5)
                    {
                        StartCoroutine(LaserAttackMove());
                    }
                    else if (randNum > 1)
                    {
                        StartCoroutine(TailAttackMove());
                    }
                    else
                    {
                        StartCoroutine(Taunting());
                    }
                }
            }
        }
        else
        {
            if (m_hasDeadTaunted == false)
            {
                StartCoroutine(Taunting());
                m_hasDeadTaunted = true;
            }
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

        // Play the sound
        TailAttackSound.GetComponent<AudioSource>().Play();

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

        m_navAgent.SetDestination(m_laserAtkStandingLocation.transform.position);
        bool isAtLocation = false;
        while (isAtLocation == false)
        {
            // Check if medusa is close enough to the location
            Vector3 finalLocation = m_laserAtkStandingLocation.transform.position;
            if (Vector3.Distance(this.transform.position, finalLocation) < 2.0f)
            {
                isAtLocation = true;
            }
            yield return null;
        }
        m_navAgent.SetDestination(this.transform.position);

        // Keep rotate towards player to aim
        Coroutine rotationCoroutine = StartCoroutine(RotateTowardsPlayer());

        // Start the aiming
        Coroutine aimingCoroutine = StartCoroutine(AimingLaser());

        // Wait for the time to aim
        yield return new WaitForSeconds(AimingTime);

        // Turn off the indication
        StopCoroutine(aimingCoroutine);

        // Play the animation and undraw the indication asap
        m_animator.SetTrigger("LaserAttack");
        UndrawLaserIndication();

        // Wait for the animation charge
        yield return new WaitForSeconds(1.0f);

        // Rotate the particle
        Vector3 aimDirection = m_playerTarget.transform.position - LaserParticle.transform.position;
        Quaternion laserRotation = Quaternion.LookRotation(aimDirection);
        LaserParticle.transform.rotation = laserRotation;

        // Draw the particle effect
        ParticleSystem laserParticle = LaserParticle.GetComponent<ParticleSystem>();
        laserParticle.Play();

        // Wait for the laser travel time
        yield return new WaitForSeconds(0.5f);

        // Apply damage
        Coroutine laserDmgCoroutine = StartCoroutine(ShootOutLaser());
        yield return new WaitForSeconds(LaserDuration);
        StopCoroutine(laserDmgCoroutine);

        // Stop aiming as about to fire
        StopCoroutine(rotationCoroutine);

        // Wait for the recovery of shooting the laser
        yield return new WaitForSeconds(RecoverTimeAfterLaser);

        // Free the action lock as this action has finished
        m_actionLock = false;
    }

    /*
     *	Stop moving and do the laser attack
     */
    public IEnumerator Taunting()
    {
        // Lock the action
        m_actionLock = true;

        // Keep Rotating towards player
        Coroutine rotationCoroutine = StartCoroutine(RotateTowardsPlayer());

        // Start the animation
        m_animator.SetTrigger("Taunt");

        // Wait till the animation finishes
        yield return new WaitForSeconds(TauntingTime);

        // Stop Rotating
        StopCoroutine(rotationCoroutine);

        // Small idle time
        yield return new WaitForSeconds(2.0f);

        // Free the action lock as this action has finished
        m_actionLock = false;
    }

    /*
     * Dealing damage every tick
     */
    public IEnumerator ShootOutLaser()
    {
        Vector3 aimLocation;
        Vector3 aimDirection;

        while (true)
        {
            // Get the newest player location
            aimLocation = m_playerTarget.transform.position;

            // Get the direction of the laser being shoot
            aimDirection = Vector3.Normalize(aimLocation - LaserIndicationOrigin.transform.position);

            // Rotate the laser as shooting
            Quaternion laserRotation = Quaternion.LookRotation(aimDirection);
            LaserParticle.transform.rotation = laserRotation;

            // Shoot out a ray
            RaycastHit rayHit;
            Physics.Raycast(LaserIndicationOrigin.transform.position, aimDirection, out rayHit);

            GameObject hitObj = rayHit.collider.gameObject;
            if (hitObj.tag == "Player")
            {
                hitObj.GetComponent<Player>().TakeDamage(LaserAtkDmgPerTick * Time.deltaTime);
            }

            // Create a collider and see if it hits the player
//             float laserLength =
//                 Vector3.Distance(LaserIndicationOrigin.transform.position, aimLocation);
// 
//             RaycastHit[] laserHits;
//             laserHits =
//                 Physics.SphereCastAll(LaserIndicationOrigin.transform.position, 2.0f, aimDirection, laserLength);

//             foreach (RaycastHit hitResult in laserHits)
//             {
//                 GameObject resultObj = hitResult.collider.gameObject;
//                 if (resultObj.tag == "Player")
//                 {
//                     resultObj.GetComponent<Player>().TakeDamage(LaserAtkDmgPerTick);
//                     break;
//                 }
//             }

            yield return null;
        }
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
     * Aiming the laser beam at player
     */
    public IEnumerator AimingLaser()
    {
        while (true)
        {
            Vector3 aimDirection = Vector3.Normalize(m_playerTarget.transform.position - LaserParticle.transform.position);

            // Shoot out a ray that hit something and get the position
            RaycastHit hitResult;
            Physics.Raycast(LaserIndicationOrigin.transform.position, aimDirection, out hitResult);
            Vector3 hitPoint = hitResult.point;

            DrawLaserIndication(hitPoint);

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
        yield return new WaitForSeconds(2.0f);

        m_portalLight.SetActive(true);
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

    /*
     * Update the medusa health bar
     */
    private void UpdateHealthBar()
    {
        HealthBar.GetComponent<Slider>().value = CurrHealth / TotalHealth;
    }

    /* Interface Implementation =================================*/

    // IKillable
    public void TakeDamage(float _value)
    {
        CurrHealth -= _value;
        UpdateHealthBar();
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
