using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Player : MonoBehaviour, IKillable
{
	[Header("Player Stat")]
	public float TotalHealth = 100f;
	public float CurrHealth;
	public float AttackDmg = 50f;
	public float Deffence = 50f;
	public float Speed = 300f;
    public float AttackRange = 3f;
    public float AttackRadius = 1f;
    public bool darkaura = false;
    public GameObject effect;
    public bool novillager = false;
    [Header("Config")]
    public LayerMask AttackingLayer;

    // Behaviour Flags ==============
    private bool m_canLightAttack;

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

    void Awake()
	{
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
        }

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
			Find("DeffendValue").gameObject.GetComponent<TextMeshProUGUI>();
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
        killCount = 0;
    }
	void Update()
	{

        if (darkaura)
        {
            effect.SetActive(true);
        }

        if (Deffence >= 60)
        {
            darkaura = true;
        }

        if (AttackDmg >= 60)
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
            if (m_canLightAttack)
            {
                StartCoroutine(LightAttack());
            }
        }
	}
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item") //If we collide with an item that we can pick up
        {
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

        // Shoot out a ray infront of the player
        Ray attackRay = new Ray(this.transform.position, this.transform.forward);

        RaycastHit[] raycastHits;
        // Cast out the ray as a sphere shape in the attack range
        raycastHits = 
            Physics.SphereCastAll(attackRay, AttackRadius, AttackRange, AttackingLayer, QueryTriggerInteraction.Ignore);
        Debug.DrawRay(transform.position, transform.forward * AttackRange, Color.blue, 2f, false);

        m_animator.SetBool("IsAttacking", true);
        yield return new WaitForSeconds(0.5f);
        foreach (RaycastHit hitResult in raycastHits)
        {
            Debug.Log("Hit: " + hitResult.transform.gameObject.name);
            // Do whatever the other object needs to be react
            IKillable killableObj = hitResult.transform.GetComponent<IKillable>();
            if (killableObj != null)
            {
                killableObj.TakeDamage(AttackDmg);
                if (hitResult.transform.tag == "Enemy")
                {
                    killCount++;
                }
            }
        }
        //yield return new WaitForSeconds(0.5f);
        m_animator.SetBool("IsAttacking", false);
        m_canLightAttack = true;
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
		atkLabel.SetText(AttackDmg.ToString());
		defLabel.SetText(Deffence.ToString());
		spdLabel.SetText(Speed.ToString());
	}

    /* Interface Implementation =================================*/

    // IKillable
    public void TakeDamage(float _value)
    {
        CurrHealth -= _value;
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
        StopAllCoroutines();
        
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
