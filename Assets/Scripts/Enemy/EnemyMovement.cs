using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;

public class EnemyMovement : StateMachine, IKillable
{
    [Header("Stats")]
    public float attackDamage;
    public float attackSpeed;
    public float attackRange = 3f;
    public float attackRadius = 1f;
    public float maxHealth = 100;
    public float currHealth;

    [Header("Movement")]
    public bool isWandering;
    public float wanderOffsetX;
    public float wanderOffsetZ;
    public float ChangeDirectionSpeed;

    public bool damagingplayer = false;
    // Navigation Agent
    private NavMeshAgent agent;
    public Animator anim;
    public SkinnedMeshRenderer meshren;

    public GameObject blackGem;

    public ParticleSystem deathparticle;
    public AudioSource Boom;
    public float IdleToPatrolStandby = 4;
    public Transform[] PatrolPoints;
    Transform Target = null;
    public float PatrolPointThreshold = 0.5f;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = this.GetComponent<Animator>();
    }
    private void Start()
    {
        this.Register(new Idle(this));
        this.Register(new Patrol(this));
        this.Register(new Chase(this));
        this.SetStartState(StateType.Idle);
        //StartCoroutine(MoveToPos());
        currHealth = maxHealth;
    }
    private void Update()
    {

        this.UpdateMachine();

        if (Boom == null)
        {
            Boom = GameObject.Find("BoomSound").GetComponent<AudioSource>();
        }

        // Shoot out a ray infront of the player
        Ray attackRay = new Ray(this.transform.position, this.transform.forward);

        RaycastHit[] raycastHits;
        // Cast out the ray as a sphere shape in the attack range
        raycastHits = Physics.SphereCastAll(attackRay, attackRadius, attackRange, LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
        Debug.DrawRay(transform.position, transform.forward * attackRange, Color.blue, 1f, false);



        //anim.SetTrigger("Attack");
        foreach (RaycastHit hitResult in raycastHits)
        {
            if (hitResult.rigidbody.tag == "Player" && hitResult.distance < 1)
            {
                anim.SetBool("Attack", true);
                damagingplayer = true;
            }
            else
            {
                damagingplayer = false;
            }
        }
        //anim.SetBool("Attack", false);


        


    }
    private void OnTriggerStay(Collider other)
    {
        //if (GetComponentInChildren<Collider>().GetType() == typeof(SphereCollider) && other.tag == "Player")
        //{
        if (IsAlive() && other.tag == "Player")
        {

            if (IsAlive()) {
                Target = other.transform;
            }
            
        }

        //}


    }
    //private void OnTriggerExit(Collider other)
    //{
    //    if (GetComponent<Collider>().GetType() == typeof(SphereCollider) && other.tag == "Player" && IsAlive())
    //    {
    //        isWandering = true;
    //        //StartCoroutine(MoveToPos());
    //    }
  
    //}
    //private void OnCollisionStay(Collision collision)
    //{
    //    if (collision.gameObject.tag == "Player")
    //    {
    //        damagingplayer = true;
            
    //    }
    //}

    //IEnumerator MoveToPos()
    //{

    //    Vector3 newPos = new Vector3(Random.Range(transform.position.x - wanderOffsetX, transform.position.x + wanderOffsetX),
    //        transform.position.y,
    //        Random.Range(transform.position.z - wanderOffsetZ, transform.position.z + wanderOffsetZ));

    //    agent.SetDestination(newPos);
    //    agent.updateRotation = true;
    //    yield return new WaitForSeconds(ChangeDirectionSpeed);

    //    if (isWandering)
    //    {
    //        StartCoroutine(MoveToPos());
    //    }
    //}
    IEnumerator DamageEffect()
    {
        anim.SetTrigger("Hurt");
        meshren.material.color = new Color(1, 0, 0);
        yield return new WaitForSeconds(0.1f);
        meshren.material.color = new Color(1, 1, 1);
    }
    public void Attack()
    {
        //yield return new WaitForSeconds(attackSpeed);
        if (damagingplayer == true) {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().TakeDamage(Mathf.Abs(attackDamage - GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Defence));
        }
        
    }

    public void DoneAttack()
    {
        anim.SetBool("Attack", false);
        damagingplayer = false;
    }

    /* Interface Implementation =================================*/

    // IKillable
    public void TakeDamage(float _value)
    {
        currHealth -= _value;
        StartCoroutine(DamageEffect());
        CheckDeath();
    }
    public void CheckDeath()
    {
        if(IsAlive() == false)
        {
            KillEntity();
        }
    }
    public void KillEntity()
    {
        GetComponent<BoxCollider>().enabled = false;
        //GetComponent<SphereCollider>().enabled = false;
        anim.SetBool("Attack", false);
        StopAllCoroutines();
        agent.SetDestination(this.transform.position);
        anim.SetBool("Death", true);

        StartCoroutine(DeathTimer());
    }

    IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(1);

        Instantiate(deathparticle, transform.position, Quaternion.identity);
        // Shake Camera
        Camera.main.DOKill(true);
        Camera.main.DOShakePosition(0.1f, 0.5f, 40);
        Camera.main.DOFieldOfView(50f, 0.2f).From();

        Boom.Play();

        Instantiate(blackGem, transform.position, transform.rotation);

        TweenPost();

        Destroy(gameObject);


    }

    public PostProcessVolume PostVol;

    void TweenPost()
    {
        DOTween.To( () => PostVol.weight, x => PostVol.weight = x, 1f, 0.1f );
        DOTween.To(() => PostVol.weight, x => PostVol.weight = x, 0, 0.2f).SetDelay(0.1f);
    }

    public void DoneDeath()
    {
        this.tag = "MutiItem";
        this.GetComponent<BoxCollider>().isTrigger = true;
    }

    public bool IsAlive()
    {
        if (currHealth <= 0)
        {
            return false;
        }
        return true;
    }



    public class Idle : IState
    {
        float TimeToPatrol;
        public Idle(StateMachine machine) : base(machine)
        {


        }
        public override StateType ID
        {
            get
            {
                return StateType.Idle;
            }
        }

        public override string Name
        {
            get
            {
                return "Idle State";


            }
        }

        public override void OnEnd()
        {

        }

        public override void OnStart()
        {
            EnemyMovement npc = (EnemyMovement)Machine;
            float maxTime = npc.IdleToPatrolStandby;
            //get random idle time before patrol state
            this.TimeToPatrol = UnityEngine.Random.Range(0, maxTime);
        }

        public override void OnUpdate()
        {
            TimeToPatrol -= Time.deltaTime;
            if (TimeToPatrol <= 0)
            {
                Machine.ChangeState(StateType.Patroling, "Patroling");
                return;
            }
            EnemyMovement npc = (EnemyMovement)Machine;
            if (npc.Target != null)
                Machine.ChangeState(StateType.Chasing, "Chasing");
        }
    }

    public class Patrol : IState
    {
        int currentPatrolIdx = 0;
        int pointToIdle;
        public Patrol(StateMachine machine) : base(machine)
        {

        }
        public override StateType ID
        {
            get
            {
                return StateType.Patroling;
            }
        }

        public override string Name
        {
            get
            {
                return "Patrol State";
            }
        }

        public override void OnEnd()
        {

        }

        public override void OnStart()
        {
            EnemyMovement npc = (EnemyMovement)Machine;
            pointToIdle = UnityEngine.Random.Range(0, npc.PatrolPoints.Length);
        }

        public override void OnUpdate()
        {
            EnemyMovement npc = (EnemyMovement)Machine;
            Transform target = npc.PatrolPoints[currentPatrolIdx];
            Vector3 posA = npc.transform.position;
            Vector3 posB = target.position;
            Vector3 deltaPos = posB - posA;
            float distance = deltaPos.magnitude;

            if (distance <= npc.PatrolPointThreshold)//point has been reached, go to another point
            {
                if (currentPatrolIdx == pointToIdle)
                {
                    Machine.ChangeState(StateType.Idle, "Idle");
                    return;
                }
                currentPatrolIdx++;
            }
            if (currentPatrolIdx >= npc.PatrolPoints.Length)
                currentPatrolIdx = 0;

            target = npc.PatrolPoints[currentPatrolIdx];
            npc.agent.SetDestination(target.position);


            if (npc.Target != null)
                Machine.ChangeState(StateType.Chasing, "Chasing");
        }
    }

    public class Chase : IState
    {
        float CurrentSpeed;

        public Chase(StateMachine machine) : base(machine)
        {

        }
        public override StateType ID
        {
            get
            {
                return StateType.Chasing;
            }
        }

        public override string Name
        {
            get
            {
                return "Chase state";
            }
        }

        public override void OnEnd()
        {
            EnemyMovement npc = (EnemyMovement)Machine;
            npc.agent.speed = CurrentSpeed;//Put back original value, cleaning up 
        }

        public override void OnStart()
        {
            EnemyMovement npc = (EnemyMovement)Machine;
            CurrentSpeed = npc.agent.speed;
            npc.agent.speed *= 1.5f;
        }

        public override void OnUpdate()
        {
            EnemyMovement npc = (EnemyMovement)Machine;
            Transform target = npc.Target;
            if (this.Name == "Chase state")
            {
                npc.agent.speed = 10;
            }
            npc.agent.SetDestination(target.position);
        }
    }
    // ============================================================
}
