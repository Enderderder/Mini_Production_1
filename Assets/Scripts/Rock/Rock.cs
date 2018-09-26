using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rock : MonoBehaviour, IKillable
{
    [Header("Stats")]
    public float TotalHealth = 1f;
    public float CurrHealth;

    // Healthbar UI
    private Canvas healthbarCanvas;
    private Slider healthbar;

	void Start ()
    {
        // Set to full health at the beginning
        CurrHealth = TotalHealth;
        healthbarCanvas = GetComponentInChildren<Canvas>();
        healthbar = healthbarCanvas.GetComponentInChildren<Slider>();
        healthbar.maxValue = TotalHealth;
    }

    void Update()
    {
        healthbar.value = CurrHealth;
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
        this.tag = "Item";
        this.GetComponent<BoxCollider>().isTrigger = true;
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
