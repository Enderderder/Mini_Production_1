﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Rock : MonoBehaviour
{
    [Header("Stats")]
    public float TotalHealth = 1f;
    public float CurrHealth;
    //public Animator anim;

    public GameObject brokenRock;
    public GameObject Ore;
    public AudioSource Break;

    void Start ()
    {
        // Set to full health at the beginning
        CurrHealth = TotalHealth;
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
            GameObject broken = Instantiate(brokenRock, transform.position, transform.rotation);
            broken.GetComponent<Animator>().SetTrigger("Break");
            Destroy(gameObject);

            Camera.main.DOShakePosition(0.15f, 0.5f, 40);

            GameObject Gem = Instantiate(Ore, transform.position + new Vector3(0.25f, 1, 0), transform.rotation);

            Break.Play();

            return false;
        }

        return true;
    }


    // ============================================================
}
