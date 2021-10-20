using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Target : MonoBehaviour
{
    public int points;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI congratsText;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            scoreText.SetText("Scored: " + points);
            scoreText.gameObject.SetActive(true);
            congratsText.gameObject.SetActive(true);
            other.gameObject.GetComponent<PlayerMovement>().stayStill = true;
        }
    }
    
}
