using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public Transform orientation;
    public Transform cameraObj;

    public float rotationSpeed = 3f;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer sr;
    public GameObject playerVCam;

    public RectTransform[] redArrows;
    public RectTransform[] greenArrows;

    public GameObject effectLocation;
    public GameObject pulseEffect;
    public GameObject dashEffect;
    public float arrowTransparency = 0.5f;

    public float jumpHeightApex = 2f;
    public float jumpDuration = 1f;

    float currentJumpDuration;

    public float downwardsGravityMultiplier = 1f;

    public float speed = 1.0f;
    public float maxSpeed = 5.0f;
    public float groundDrag;

    public bool isJumping = false;
    public bool hasLanded = true;
    public bool isGrounded;
    public bool allowDoubleJump = false;
    public Vector3 gravityDirection = Vector3.down;

    bool doubleJumped = false;

    float gravity;
    float initialJumpVelocity;
    float jumpStartTime;

    public float groundDistance = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
        initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Ground check
        if (
            (Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0f, -transform.up, groundDistance, LayerMask.GetMask("Ground"))
            && gravityDirection == Vector3.down)
            || (Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0f, transform.up, groundDistance, LayerMask.GetMask("Ground"))
            && gravityDirection == Vector3.up)
            )
        {
            isGrounded = true;
            if (!isJumping)
            {
                anim.SetBool("jumpingDown", false);
                anim.SetBool("jumpingUp", false);
            }
        }
        else
        {
            isGrounded = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGrounded && allowDoubleJump && !doubleJumped) //  && allowDoubleJump && !doubleJumped
            {
                doubleJumped = true;
                // StartJump(jumpHeightApex, jumpDuration);
                InvertGravity();
            }
            else if (isGrounded)
            {
                doubleJumped = false;
                StartJump(jumpHeightApex, jumpDuration);
            }
        }

        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

        // Limit velocity
        ControlSpeed();

    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position + new Vector3(0, -groundDistance, 0), new Vector3(0.5f, 0.5f, 0));
    }

    void MovePlayer()
    {
        // move player to the right constantly with force
        rb.AddForce(transform.right * speed, ForceMode2D.Impulse);
    }

    void ControlSpeed()
    {
        Vector2 flatVelocity = new Vector2(rb.velocity.x, 0);
        // Limit velocity if needed
        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector2 limitedVelocity = flatVelocity.normalized * maxSpeed;
            rb.velocity = new Vector2(limitedVelocity.x, rb.velocity.y);
        }
    }

    void FixedUpdate()
    {

        // Move player
        MovePlayer();

        // Jumping
        if (isJumping)
        {
            rb.AddForce(-gravityDirection * gravity, ForceMode2D.Force);

            if (Time.time - jumpStartTime >= currentJumpDuration)
            {
                isJumping = false;
                hasLanded = false;
            }
        }
        else
        {
            anim.SetBool("jumpingDown", true);
            anim.SetBool("jumpingUp", false);
            rb.AddForce(gravityDirection * -gravity * downwardsGravityMultiplier, ForceMode2D.Force);
        }
    }

    void StartJump(float heightApex, float duration)
    {
        // Recalculate gravity and initial velocity
        gravity = -2 * heightApex / (duration * duration);
        initialJumpVelocity = Mathf.Abs(gravity) * duration;
        currentJumpDuration = duration;

        isJumping = true;
        anim.SetBool("jumpingUp", true);
        jumpStartTime = Time.time;
        rb.velocity = -gravityDirection * initialJumpVelocity;
    }

    void InvertGravity()
    {
        gravityDirection = -gravityDirection;
        StartCoroutine(PlayArrowAnimation());
        sr.flipY = !sr.flipY;

        if (effectLocation.transform.localPosition.y < 0.1f) effectLocation.transform.localPosition = new Vector3(0, 0.175f, 0);
        else effectLocation.transform.localPosition = new Vector3(0, 0, 0);

        pulseEffect.SetActive(true);
        pulseEffect.GetComponent<Animator>().SetTrigger("pulse");
        Invoke("DisablePulse", 0.5f);
    }

    void DisablePulse()
    {
        pulseEffect.SetActive(false);
        // set cinemachine virtual camera tracked object offset to opposite of current offset
        // playerVCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_TrackedObjectOffset = new Vector3(playerVCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_TrackedObjectOffset.x, -playerVCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_TrackedObjectOffset.y, playerVCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_TrackedObjectOffset.z);
    }

    // void PlayArrowAnimation()
    // {
    //     if (gravityDirection == Vector3.down)
    //     {
    //         // set red arrows y position to 100
    //         for (int i = 0; i < redArrows.Length; i++)
    //         {
    //             redArrows[i].position = new Vector3(redArrows[i].position.x, 500, redArrows[i].position.z);
    //         }
            
    //         // use leantween and move red arrows to the bottom of the screen
    //         for (int i = 0; i < redArrows.Length; i++)
    //         {
    //             LeanTween.moveY(redArrows[i], -200, 0.5f).setEaseOutCubic();
    //         }
    //     }
    // }

    IEnumerator PlayArrowAnimation()
    {
        if (gravityDirection == Vector3.down)
        {
            for (int i = 0; i < redArrows.Length; i++)
            {
                redArrows[i].anchoredPosition = new Vector2(redArrows[i].anchoredPosition.x, 500);
                redArrows[i].gameObject.SetActive(true);
                LeanTween.alpha(redArrows[i], arrowTransparency, 0f);
            }
            
            // use leantween and move red arrows to the bottom of the screen
            for (int i = 0; i < redArrows.Length; i++)
            {
                LeanTween.moveY(redArrows[i], -200, 0.5f).setEaseOutCubic();
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.1f);

            // use leantween and set arrow alpha to 0
            for (int i = 0; i < redArrows.Length; i++)
            {
                LeanTween.alpha(redArrows[i], 0, 0.5f).setEaseOutCubic();
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.75f);

            // disable red arrows
            for (int i = 0; i < redArrows.Length; i++)
            {
                redArrows[i].gameObject.SetActive(false);
            }

        }
        else
        {
            for (int i = 0; i < greenArrows.Length; i++)
            {
                greenArrows[i].anchoredPosition = new Vector2(greenArrows[i].anchoredPosition.x, -500);
                greenArrows[i].gameObject.SetActive(true);
                LeanTween.alpha(greenArrows[i], arrowTransparency, 0f);
            }
            
            // use leantween and move green arrows to the top of the screen
            for (int i = 0; i < greenArrows.Length; i++)
            {
                LeanTween.moveY(greenArrows[i], 200, 0.5f).setEaseOutCubic();
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.1f);

            // use leantween and set arrow alpha to 0
            for (int i = 0; i < greenArrows.Length; i++)
            {
                LeanTween.alpha(greenArrows[i], 0, 0.5f).setEaseOutCubic();
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.75f);

            // disable green arrows
            for (int i = 0; i < greenArrows.Length; i++)
            {
                greenArrows[i].gameObject.SetActive(false);
            }
        }
    }
}