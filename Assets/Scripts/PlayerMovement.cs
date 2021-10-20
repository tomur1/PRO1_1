using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float playerJumpForce = 5f;
    public float actualjumpForce;
    public float constantSpeedModifier = 300;
    public float sprintSpeedUp = 3f;
    public float dashSpeed = 10f;
    public float maxSpeed = 3f;

    public int groundRaycasts = 5;
    public int horizontalRaycasts = 3;
    public float raycastLenght = 0.1f;

    public TextMeshProUGUI playerSpeedText;
    public TextMeshProUGUI playerStateText;
    public Camera playerCamera;

    private float maxSpeedLimit;
    
    private float horizontalMovement;
    private bool wantToJump;
    private bool wantToSprint;
    private bool wantToDash;

    private GameObject movingPlatform;
    private bool onMovingPlatform;
    
    private bool touchesGround;
    private bool recentlyJumped;
    private bool shouldBeKilled;
    private bool closeToLeftWall;
    private bool closeToRightWall;
    private bool dashing;
    private float playerDirection;
    private LastTouched lastTouchedGround;
    private bool inCatapult;
    private bool enteredCatapult;
    public bool stayStill;
    
    private enum LastTouched
    {
        WALL,
        MOVING_PLATFORM,
        JUMPING_PLATFORM,
        VANISHING_PLATFORM,
        CATAPULT_PLATFORM
        
    }

    private Rigidbody2D rb2d;
    
    // Start is called before the first frame update
    void Start()
    {
        lastTouchedGround = LastTouched.WALL;
        rb2d = GetComponent<Rigidbody2D>();
        StartCoroutine(UpdateVelocityText());
    }

    IEnumerator UpdateVelocityText()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            playerSpeedText.SetText("Movement Speed: " + rb2d.velocity.x.ToString("#.#"));
        }
        
    }

    IEnumerator Dash()
    {
        dashing = true;
        yield return new WaitForSeconds(0.3f);
        dashing = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        playerStateText.SetText("State: Walking");
        
        if (stayStill)
        {
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            playerStateText.SetText("State: Frozen");
            return;
        }

        if (shouldBeKilled)
        {
            SceneManager.LoadScene(0);
        }
        
        if (inCatapult)
        {
            playerStateText.SetText("State: In catapult");
            if (enteredCatapult)
            {
                
                rb2d.velocity = Vector2.zero;
                rb2d.constraints = RigidbodyConstraints2D.None;
                enteredCatapult = false;
            }
            return;
        }
        
        if (dashing)
        {
            playerStateText.SetText("State: Dashing");
            rb2d.velocity = new Vector2(-playerDirection * dashSpeed, 0);
            return;
        }

        actualjumpForce = playerJumpForce;
        
        GetMovement();
        CheckRaycasts();
        
        maxSpeedLimit = maxSpeed;
        
        float actualHorizontalMovement = horizontalMovement;
        if (wantToDash)
        {
            StartCoroutine(Dash());
        }else if (wantToSprint)
        {
            playerStateText.SetText("State: Sprinting");
            maxSpeedLimit = maxSpeed + sprintSpeedUp;
        }

        ApplyMovement(actualHorizontalMovement);
    }

    private void ApplyMovement(float actualHorizontalMovement)
    {
        // Handle jump
        if (touchesGround && wantToJump && !recentlyJumped)
        {
            rb2d.AddForce(Vector2.up * (actualjumpForce * Time.fixedDeltaTime), ForceMode2D.Impulse);
            StartCoroutine(ResetJump());
        }
        
        // Not touching keyboard
        if (actualHorizontalMovement == 0)
        {
            //on platform
            if (onMovingPlatform && !recentlyJumped)
            {
                rb2d.velocity = movingPlatform.GetComponent<MovingPlatform>().trackVelocity;
                return;
            }
            
            if (!touchesGround && lastTouchedGround == LastTouched.MOVING_PLATFORM)
            {
                return;
            }
            
            if (Math.Abs(rb2d.velocity.x) < 0.2f)
            {
                rb2d.velocity = new Vector2(0, rb2d.velocity.y);
            }
            else
            {
                rb2d.AddForce(new Vector2(playerDirection, 0) * (Time.fixedDeltaTime * constantSpeedModifier),
                    ForceMode2D.Force);
            }
        }
        else
        {
            // Moving slower than limit
            if (Mathf.Abs(rb2d.velocity.x) <= maxSpeedLimit || onMovingPlatform || lastTouchedGround == LastTouched.MOVING_PLATFORM)
            {
                rb2d.AddForce(new Vector2(actualHorizontalMovement, 0) * (Time.fixedDeltaTime * constantSpeedModifier),
                    ForceMode2D.Force);
            }
            else
            {
                rb2d.AddForce(new Vector2(-rb2d.velocity.x, 0) * (Time.fixedDeltaTime * constantSpeedModifier * 0.1f),
                    ForceMode2D.Force);
            }
        }
    }

    IEnumerator ResetJump()
    {
        recentlyJumped = true;
        yield return new WaitForSeconds(0.1f);
        recentlyJumped = false;
    }

    private void CheckRaycasts()
    {
        var spriteSize = GetComponent<SpriteRenderer>().bounds;
        touchesGround = false;
        shouldBeKilled = false;
        onMovingPlatform = false;
        movingPlatform = null;
        for (int i = 0; i <= groundRaycasts; i++)
        {
            // Shoot rays
            var offset = (spriteSize.size.x / groundRaycasts) * i;
            
            var hit = Physics2D.Raycast(transform.position - spriteSize.extents + new Vector3(offset, 0, 0),
                Vector2.down, raycastLenght);

            // Handle hits
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    touchesGround = true;
                    lastTouchedGround = LastTouched.WALL;
                }else if (hit.collider.CompareTag("KillOnTouch"))
                {
                    shouldBeKilled = true;
                }else if (hit.collider.CompareTag("Moving Platform"))
                {
                    touchesGround = true;
                    movingPlatform = hit.collider.gameObject;
                    onMovingPlatform = true;
                    lastTouchedGround = LastTouched.MOVING_PLATFORM;
                }else if (hit.collider.CompareTag("Jumping Platform"))
                {
                    touchesGround = true;
                    actualjumpForce = playerJumpForce + hit.collider.GetComponent<JumpingPlatform>().jumpForceAdd;
                    lastTouchedGround = LastTouched.JUMPING_PLATFORM;
                }else if (hit.collider.CompareTag("Vanishing Platform"))
                {
                    touchesGround = true;
                    hit.collider.GetComponent<VanishingPlatform>().TouchedMe();
                    lastTouchedGround = LastTouched.VANISHING_PLATFORM;
                }else if (hit.collider.CompareTag("Catapult Platform"))
                {
                    touchesGround = true;
                    hit.collider.GetComponent<CatapultPlatform>().PlayerEntered(gameObject);
                    playerCamera.gameObject.transform.SetParent(null);
                    hit.collider.GetComponent<CatapultPlatform>().MoveCameraTo(playerCamera);
                    inCatapult = true;
                    enteredCatapult = true;
                }
            }
            
            Debug.DrawRay(transform.position - spriteSize.extents + new Vector3(offset, 0 , 0), Vector2.down * raycastLenght);
        }
        
        closeToLeftWall = false;
        for (int i = 0; i <= horizontalRaycasts; i++)
        {
            var offset = (spriteSize.size.y / horizontalRaycasts) * i;
            var hit = Physics2D.Raycast(transform.position - spriteSize.extents + new Vector3(0, offset , 0), Vector2.left * raycastLenght);
            
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    closeToLeftWall = true;
                }else if (hit.collider.CompareTag("KillOnTouch"))
                {
                    shouldBeKilled = true;
                }
            }
            
            Debug.DrawRay(transform.position - spriteSize.extents + new Vector3(0, offset , 0), Vector2.left * raycastLenght);
        }

        closeToRightWall = false;
        for (int i = 0; i <= horizontalRaycasts; i++)
        {
            var offset = (spriteSize.size.y / horizontalRaycasts) * i;
            var hit = Physics2D.Raycast(transform.position + spriteSize.extents - new Vector3(0, offset , 0), Vector2.right * raycastLenght);
            
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    closeToRightWall = true;
                }else if (hit.collider.CompareTag("KillOnTouch"))
                {
                    shouldBeKilled = true;
                }
            }
            
            Debug.DrawRay(transform.position + spriteSize.extents - new Vector3(0, offset , 0), Vector2.right * raycastLenght);
        }
    }

    private void GetMovement()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        wantToJump = Input.GetAxisRaw("Jump") == 1;
        wantToSprint = Input.GetAxisRaw("Sprint") == 1;
        wantToDash = Input.GetAxisRaw("Dash") == 1;
        
        if (rb2d.velocity.x > 0)
        {
            playerDirection = -1;
        }
        else if (rb2d.velocity.x < 0)
        {
            playerDirection = 1;
        }
        else
        {
            playerDirection = 0;
        }
    }
}
