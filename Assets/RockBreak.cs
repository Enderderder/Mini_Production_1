using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RockBreak : MonoBehaviour {

    void Start()
    {
        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        yield return new WaitForSeconds(1);
        transform.DOMoveY(transform.position.y - 2, 5);
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
}
