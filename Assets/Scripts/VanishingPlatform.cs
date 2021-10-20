using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishingPlatform : MonoBehaviour
{
    public float vanishTime = 1f;
    public float step = 0.1f;

    private bool vaninshingNow;
    private float remaining;

    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        remaining = vanishTime;
        sr = GetComponent<SpriteRenderer>();
    }

    public void TouchedMe()
    {
        if (!vaninshingNow)
        {
            StartCoroutine(Vanish());
        }
        vaninshingNow = true;
    }

    IEnumerator Vanish()
    {
        while (remaining > 0)
        {
            remaining -= Time.deltaTime;
            yield return null;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, remaining / vanishTime);
        }
        Destroy(gameObject);
    }
}
