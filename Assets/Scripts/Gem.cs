using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Gem : MonoBehaviour {

    public float FloatingRange = 0.5f;
    public float FloatingSpeed = 2f;
    public float StartingOffset;
    public float spinMultiplier;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Sine Wave
        float y = FloatingRange * Mathf.Sin((Time.time * FloatingSpeed) + StartingOffset);

        Vector3 resultVec = startPos;
        resultVec.y += y;
        transform.position = resultVec;
        transform.Rotate(new Vector3(0, spinMultiplier, 0));
    }
}
