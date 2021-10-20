using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public GameObject startPoint;
    public GameObject endPoint;
    
    private float positionPercentage;
    private bool goBack;

    private Rigidbody2D rb2d;
    
    public Vector3 newPosition;
    public Vector3 oldPosition;
    private string currentState = "Moving To Position 1";
    public float smooth = 100;
    public float resetTime;
    public Vector2 trackVelocity;

    private Vector2 lastPos;
 
    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        ChangeTarget();
    }
     
    // Update is called once per frame
    void FixedUpdate ()
    {
        positionPercentage += Time.deltaTime * smooth;
        var pos = Vector3.Lerp(oldPosition, newPosition, positionPercentage);
        rb2d.MovePosition(pos);
        
        trackVelocity = (rb2d.position - lastPos) * 1 / Time.fixedDeltaTime;
        lastPos = rb2d.position;
    }
 
    void ChangeTarget () 
    {
        if(currentState == "Moving To Position 1")
        {
            currentState = "Moving To Position 2";
            newPosition = endPoint.transform.position;
            oldPosition = startPoint.transform.position;
        }
        else if (currentState == "Moving To Position 2")
        {
            currentState = "Moving To Position 1";
            oldPosition = endPoint.transform.position;
            newPosition = startPoint.transform.position;
        }
        
        positionPercentage = 0f;
        Invoke (nameof(ChangeTarget), resetTime);
    }
 
}
