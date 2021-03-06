﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour, IKillable
{
	[Header("Player Stat")]
	public float TotalHealth = 100f;
	public float CurrHealth;
	public float lightAttackDmg = 50f;
    public float heavyAttackmutiply = 1.5f;
	public float Defence = 50f;
	public float Speed = 300f;
    public float AttackRange = 3f;
    public float AttackRadius = 1f;
    public bool darkaura = false;
    public GameObject effect;
    public bool novillager = false;
    [Header("Config")]
    public LayerMask AttackingLayer;
    public AudioSource Slash;
    public AudioSource Sweep;
    public AudioSource Pickup;
    public Image gameover;
    private Image[] gameoverchild;


    // Behaviour Flags ==============
    private bool m_canLightAttack;
    private bool m_canBigAttack;

    // Animation Control ============
    private Animator m_animator;

    // Player HUD ===================
    private GameObject m_healthBarUI;

	// Player Stats Panel ===========
	private GameObject m_statsCanvas;
	private TextMeshProUGUI atkLabel;
	private TextMeshProUGUI defLabel;
	private TextMeshProUGUI spdLabel;

    [System.NonSerialized]
    public int killCount;

    public Inventory inventory;
    private static bool created = false;

    private GameObject lastRockInRange;
    public GameObject dialogueBox;

    [System.NonSerialized] public int BlackGemsCollected;
    [System.NonSerialized] public int BlueGemsCollected;

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
        }

        dialogueBox.SetActive(false);

        // Reference Player animator
        m_animator = GetComponentInChildren<Animator>();

		// Reference the health UI
		m_healthBarUI = 
			transform.Find("PlayerUICanvas/PlayerHealthBar").gameObject;

		// Reference the stats panel UI
		m_statsCanvas = 
			transform.Find("PlayerUICanvas/StatsPanelCanvas").gameObject;
        atkLabel = m_statsCanvas.transform.
            Find("AttackValue").gameObject.GetComponent<TextMeshProUGUI>();
        defLabel = m_statsCanvas.transform.
            Find("DefendValue").gameObject.GetComponent<TextMeshProUGUI>();
        spdLabel = m_statsCanvas.transform.
            Find("SpeedValue").gameObject.GetComponent<TextMeshProUGUI>();
    }
	void Start()
	{
        // Set the health to full health
        CurrHealth = TotalHealth;
        UpdateHealthBar();

        // Hide the stats panel at the beginning
        m_statsCanvas.SetActive(false);

        // Flag set to default
        m_canLightAttack = true;
        m_canBigAttack = true;
        killCount = 0;
        gameoverchild = this.gameover.GetComponentsInChildren<Image>();
        foreach (Image ui in gameoverchild) {
            ui.DOFade(0, 0);
        }
        gameover.gameObject.SetActive(false);
    }

	void Update()
	{
        

        if (darkaura)
        {
            effect.SetActive(true);
        }

        if (Defence >= 60)
        {
            darkaura = true;
        }

        if (lightAttackDmg >= 60)
        {
            novillager = true;
        }

        CheckDeath();
		if (Input.GetKeyDown(KeyCode.P))
		{
			StatsPanelOnOff();
		}

        if (Input.GetButtonDown("Attack"))
        {
            if (m_canLightAttack && m_canBigAttack)
            {
                StartCoroutine(LightAttack());
            }
        }
        if (Input.GetButtonDown("BigAttack"))
        {
            if (m_canBigAttack && m_canLightAttack)
            {
                StartCoroutine(BigAttack());
            }
        }

        // Shoot out a ray infront of the player
        Ray attackRay = new Ray(this.transform.position, this.transform.forward);

        RaycastHit[] raycastHits;
        // Cast out the raysa as a sphere shape in the attack range
        raycastHits =
            Physics.SphereCastAll(attackRay, AttackRadius, AttackRange, AttackingLayer, QueryTriggerInteraction.Ignore);
        //Debug.DrawRay(transform.position, transform.forward * AttackRange, Color.blue, 2f, false);

        foreach (RaycastHit hitResult in raycastHits)
        {
            if (hitResult.transform.tag == "Rock")
            {
                hitResult.transform.GetComponent<Rock>().isMinable = true;
                lastRockInRange = hitResult.transform.gameObject;

                if (Input.GetKeyDown(KeyCode.E) && m_canBigAttack && m_canLightAttack)
                {
                    StartCoroutine(MineRock(hitResult.transform.gameObject));
                }
            }
        }

        if (lastRockInRange != null)
        {
            lastRockInRange.GetComponent<Rock>().isMinable = false;
            lastRockInRange = null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item") //If we collide with an item that we can pick up
        {
            Pickup.Play();
            inventory.AddItem(other.GetComponent<Item>()); //Adds the item to the inventory.
            Destroy(other.gameObject);
        }

        if (other.tag == "MutiItem")
        {
            for (int i = 0; i < other.GetComponent<MutiItem>().items.Length; i++)
            {
                inventory.AddItem(other.GetComponent<MutiItem>().items[i]);
            }
            Destroy(other.gameObject);
        }
    }

    private IEnumerator LightAttack()
    {
        // Make light attack on CD
        m_canLightAttack = false;
        m_canBigAttack = false;

        // Shoot out a ray infront of the player
        Ray attackRay = new Ray(this.transform.position, this.transform.forward);

        RaycastHit[] raycastHits;
        // Cast out the raysa as a sphere shape in the attack range
        raycastHits = 
            Physics.SphereCastAll(attackRay, AttackRadius, AttackRange, AttackingLayer, QueryTriggerInteraction.Collide);
        Debug.DrawRay(transform.position, transform.forward * AttackRange, Color.blue, 2f, false);

        m_animator.SetBool("Attack", true);
        Slash.Play();
        Slash.pitch = Random.Range(1.6f, 2.4f);

        foreach (RaycastHit hitResult in raycastHits)
        {
            // Do whatever the other object needs to be react
            IKillable killableObj = hitResult.transform.GetComponentInParent<IKillable>();
            if (killableObj != null)
            {
                killableObj.TakeDamage(lightAttackDmg);
                if (hitResult.transform.tag == "Enemy")
                {
                    killCount++;
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        m_animator.SetBool("Attack", false);
        m_canLightAttack = true;
        m_canBigAttack = true;
    }
    private IEnumerator BigAttack()
    {
        m_canLightAttack = false;
        m_canBigAttack = false;

        // Disable the movement as the big attack
        GetComponent<PlayerMoveTemp>().enabled = false;

        // Start the animation
        m_animator.SetBool("BigAttack", true);

        // Wait a bit before the swinbg apply damage
        yield return new WaitForSeconds(0.5f);

        // Play the sound FX
        Sweep.Play();
        Sweep.pitch = Random.Range(0.86f, 1.16f);

        RaycastHit[] raycastHits;
        // Cast out the raysa as a sphere shape in the attack range
        raycastHits = 
            Physics.SphereCastAll(this.transform.position, AttackRange, Vector3.forward, 0.0f, AttackingLayer, QueryTriggerInteraction.Collide);
        Debug.DrawRay(transform.position, transform.forward * AttackRange, Color.blue, 2f, false);

        // Check the hit result and apply damage as needs
        foreach (RaycastHit hitResult in raycastHits)
        {
            // Do whatever the other object needs to be react
            IKillable killableObj = hitResult.transform.GetComponentInParent<IKillable>();
            if (killableObj != null)
            {
                killableObj.TakeDamage(lightAttackDmg * heavyAttackmutiply);
                if (hitResult.transform.tag == "Enemy")
                {
                    killCount++;
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        m_animator.SetBool("BigAttack", false);
        m_canLightAttack = true;
        m_canBigAttack = true;
        GetComponent<PlayerMoveTemp>().enabled = true;
    }

    private IEnumerator MineRock(GameObject _rock)
    {
        GetComponent<PlayerMoveTemp>().enabled = false;
        transform.LookAt(new Vector3(_rock.transform.position.x, transform.position.y, _rock.transform.position.z));
        GetComponent<Rigidbody>().freezeRotation = true;

        m_canLightAttack = false;
        m_canBigAttack = false;
        
        m_animator.SetBool("Mine", true);
        yield return new WaitForSeconds(0.65f);
        _rock.GetComponent<Rock>().TakeDamage(1);
        yield return new WaitForSeconds(0.5f);
        GetComponent<PlayerMoveTemp>().enabled = true;
        m_canLightAttack = true;
        m_canBigAttack = true;
    }

    public void UpdateHealthBar()
	{
		// Set the health bar to current health by percentage
		m_healthBarUI.GetComponent<Slider>().value = CurrHealth / TotalHealth;
	}
	private void StatsPanelOnOff()
	{
		if (m_statsCanvas.activeSelf == false)
		{
			m_statsCanvas.SetActive(true);
			UpdateStatsPanel();
		}
		else if (m_statsCanvas.activeSelf == true)
		{
			m_statsCanvas.SetActive(false);
		}
	}
	public void UpdateStatsPanel()
	{
		atkLabel.SetText(lightAttackDmg.ToString());
		defLabel.SetText(Defence.ToString());
		spdLabel.SetText(Speed.ToString());
	}

    /* Interface Implementation =================================*/

    // IKillable
    public void TakeDamage(float _value)
    {
        if (_value - Defence <= 0) { CurrHealth--; }
        else { CurrHealth -= _value - Defence; }
        
        UpdateHealthBar();
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
        //StopAllCoroutines();
        GetComponent<PlayerMoveTemp>().enabled = false;
        if (m_animator.GetCurrentAnimatorStateInfo(0).IsName("Death") && m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            gameover.gameObject.SetActive(true);
            if (SceneManager.GetActiveScene().name == "Boss")
            {

                foreach (Image ui in gameoverchild)
                {
                    ui.DOFade(1, 5);
                }
            }
            else
            {
                foreach (Image ui in gameoverchild)
                {
                    ui.DOFade(1, 1);
                }
            }
        }
    }
    public bool IsAlive()
    {
        if (CurrHealth <= 0f)
        {
            return false;
        }
        return true;
    }

    public void respawn()
    {
        CurrHealth = 100;
        UpdateHealthBar();
        m_animator.SetBool("IsDead", false);
        GetComponent<PlayerMoveTemp>().enabled = true;
        foreach (Image ui in gameoverchild)
        {
            ui.DOFade(0, 1);
        }
        gameover.gameObject.SetActive(false);

        if (SceneManager.GetActiveScene().name == "Boss")
        {
            SceneManager.LoadScene("ChrisTest");
        }
    }


    // ============================================================


}
