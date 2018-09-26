using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatyRock : MonoBehaviour
{
    [Header("Config")]
    public float FloatingRange = 0.5f;
    public float FloatingSpeed = 2f;
    public float StartingOffset;

    private Vector3 m_beginningPos;

	void Start ()
    {
        // Set the beginning position
        m_beginningPos = this.transform.position;
    }
	
	void Update ()
    {
        // Sine Wave
        float y = FloatingRange * Mathf.Sin( (Time.time * FloatingSpeed) + StartingOffset );

        Vector3 resultVec = m_beginningPos;
        resultVec.y += y;
        this.transform.position = resultVec;

    }
}
