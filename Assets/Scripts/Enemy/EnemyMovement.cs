using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour, IKillable
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

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = this.GetComponent<Animator>();
    }
    private void Start()
    {
        StartCoroutine(MoveToPos());
        currHealth = maxHealth;
    }
    private void Update()
    {
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
            }
        }
        //anim.SetBool("Attack", false);

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (IsAlive()) {
                isWandering = false;
                agent.SetDestination(other.transform.position);
                agent.updateRotation = true;
            }
            else
            {
                agent.SetDestination(this.transform.position);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && IsAlive())
        {
            isWandering = true;
            StartCoroutine(MoveToPos());
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            damagingplayer = true;
            
        }
    }

    IEnumerator MoveToPos()
    {

        Vector3 newPos = new Vector3(Random.Range(transform.position.x - wanderOffsetX, transform.position.x + wanderOffsetX),
            transform.position.y,
            Random.Range(transform.position.z - wanderOffsetZ, transform.position.z + wanderOffsetZ));

        agent.SetDestination(newPos);
        agent.updateRotation = true;
        yield return new WaitForSeconds(ChangeDirectionSpeed);

        if (isWandering)
        {
            StartCoroutine(MoveToPos());
        }
    }
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
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().TakeDamage(attackDamage - GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Deffence);
        }
        damagingplayer = false;
    }

    public void DoneAttack()
    {
        anim.SetBool("Attack", false);
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
        anim.SetBool("Attack", false);
        StopAllCoroutines();
        agent.SetDestination(this.transform.position);
        anim.SetBool("Death", true);

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

    // ============================================================
}
