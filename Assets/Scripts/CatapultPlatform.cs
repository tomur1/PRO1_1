using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultPlatform : MonoBehaviour
{
    public Transform aim;
    public GameObject strenghtBar;
    private GameObject player;

    public float StrenghtSpeed = 1f;
    public float RotateSpeed = 1f;
    public float maxAngle = 0.5f;
    public float minAngle = 0.1f;
    private bool adding;
    private float Radius = 0.1f;
    
    private float orgScale;
    public float maxScale = 3f;
    private bool addingForce = true;
    private bool holdingSpace = false;
    private bool stoppedHoldingSpace = false;
    private Vector2 shootingDirection;
 
    private Vector2 _centre;
    private float _angle;
    private bool playerEntered;
    private bool playerShoot;

    public Transform cameraPos;
    public float cameraSize;

    public float cameraDuration;

    public float force = 10f;
    // Start is called before the first frame update
    void Start()
    {
        _centre = transform.position;
        Radius = Vector2.Distance(aim.position, _centre);
        _angle = minAngle;
        adding = true;
        orgScale = strenghtBar.transform.localScale.x;
    }

    public void PlayerEntered(GameObject player)
    {
        this.player = player;
        playerEntered = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerEntered || playerShoot)
        {
            return;
        }

        if (!holdingSpace)
        {
            MoveAim();
        }
        
        ScalePower();

        if (stoppedHoldingSpace)
        {
            ShootPlayer();
            stoppedHoldingSpace = false;
        }
        
    }
    
    public void MoveCameraTo(Camera camera)
    {
        StartCoroutine(MoveCamera(cameraPos.position, camera));
    }

    IEnumerator MoveCamera(Vector3 position, Camera camera)
    {
        for (int i = 0; i < cameraDuration; i++)
        {
            float progress = i / cameraDuration;

            camera.transform.position = Vector3.Lerp(camera.transform.position, position, progress);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, cameraSize, progress);
            yield return null;
        }
    }

    private void ShootPlayer()
    {
        var playerRb2d = player.GetComponent<Rigidbody2D>();
        playerRb2d.AddForce(shootingDirection * (force * strenghtBar.transform.localScale.x * Time.deltaTime), ForceMode2D.Impulse);
        playerRb2d.AddTorque(10f);
        playerShoot = true;
    }

    void ScalePower()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            holdingSpace = true;
            float scaleX;
            var localScale = strenghtBar.transform.localScale;
            if (addingForce)
            {
                scaleX = localScale.x + Time.deltaTime;
            }
            else
            {
                scaleX = localScale.x - Time.deltaTime;
            }

            if (scaleX > maxScale)
            {
                addingForce = false;
            }

            if (scaleX < orgScale)
            {
                addingForce = true;
            }
            
            localScale = new Vector3(scaleX, localScale.y, localScale.z);
            strenghtBar.transform.localScale = localScale;
        }
        else
        {
            if (holdingSpace)
            {
                stoppedHoldingSpace = true;
                holdingSpace = false;
            }
        }

        
    }

    void MoveAim()
    {
        if (adding)
        {
            _angle += RotateSpeed * Time.deltaTime;    
        }
        else
        {
            _angle -= RotateSpeed * Time.deltaTime;
        }

        if (_angle < minAngle)
        {
            adding = true;
        }else if (_angle > maxAngle)
        {
            adding = false;
        }
      
        var offset = new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle)) * Radius;
        aim.transform.position = _centre + offset;
        
        shootingDirection = aim.transform.position - transform.position;
        var angle = Mathf.Atan2(shootingDirection.y, shootingDirection.x) * Mathf.Rad2Deg;
        aim.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
