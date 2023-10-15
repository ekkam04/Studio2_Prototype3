using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // main control variables
    public bool allowJump = true;
    public bool allowDash = true;
    public bool allowInvert = true;
    public bool autoMove = true;

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
    public GameObject transitionVCam;
    public float playerVCamAmplitude = 1f;

    public RectTransform[] redArrows;
    public RectTransform[] greenArrows;

    public RectTransform[] blackPanels;

    public TMP_Text tutorialText;
    public Button spaceButton;
    public GameObject tutorialUI;

    public GameObject respawnEffect;
    public GameObject[] respawnOrbs;

    public GameObject effectLocation;
    public GameObject pulseEffect;
    public GameObject dashEffect;
    public ParticleSystem groundParticles;
    public float arrowTransparency = 0.5f;

    public float jumpHeightApex = 2f;
    public float jumpDuration = 1f;
    public float dashDuration = 0.5f;

    float currentJumpDuration;
    bool respawning = false;

    public float downwardsGravityMultiplier = 1f;

    public float speed = 1.0f;
    public float maxSpeed = 5.0f;
    // public float dashSpeed = 1.0f;
    // public float dashMaxSpeed = 5.0f;
    public float groundDrag;

    public bool isJumping = false;
    public bool hasLanded = true;
    public bool isGrounded;
    public bool allowDoubleJump = false;
    public Vector3 gravityDirection = Vector3.down;

    bool doubleJumped = false;
    bool isDashing = false;
    public bool jumpReleased = false;
    public bool dashReleased = false;

    float gravity;
    float initialJumpVelocity;
    float jumpStartTime;
    float dashStartTime;

    IEnumerator StartTutorialCoroutineInstance = null;
    IEnumerator TutorialCoroutineInstance1 = null;
    IEnumerator TutorialCoroutineInstance2 = null;
    IEnumerator TutorialCoroutineInstance3 = null;
    IEnumerator TutorialCoroutineInstance4 = null;

    Vector3 checkpointPosition;

    public float groundDistance = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        checkpointPosition = transform.position;

        playerVCamAmplitude = playerVCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain;

        gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
        initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;

        // Cursor.lockState = CursorLockMode.None;
        // Cursor.visible = true;

        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        Invoke("HideTransitionVCam", 1f);
    }

    void HideTransitionVCam()
    {
        transitionVCam.SetActive(false);
        autoMove = true;
        StartTutorialCoroutineInstance = PlayTutorialUI();
        StartCoroutine(StartTutorialCoroutineInstance);
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
                if (hasLanded != true)
                {
                    groundParticles.Play();
                    hasLanded = true;
                }
            }
        }
        else
        {
            isGrounded = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGrounded && allowDoubleJump && !doubleJumped && !isJumping)
            {
                print("Dash!");
                doubleJumped = true;
                if (allowDash) StartDash();
            }
            else if (!isGrounded && allowDoubleJump && !doubleJumped && isJumping)
            {
                print("Invert!");
                doubleJumped = true;
                if (allowInvert) InvertGravity();
            }
            else if (isGrounded)
            {
                doubleJumped = false;
                if (allowJump) StartJump(jumpHeightApex, jumpDuration);
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) && isJumping)
        {
            jumpReleased = true;
        }

        if (Input.GetKeyUp(KeyCode.Space) && isDashing)
        {
            dashReleased = true;
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(PlayRespawnAnimation());
        }

        if (transform.position.y < -6f || transform.position.y > 7.5f)
        {
            StartCoroutine(PlayRespawnAnimation());
        }

        if (Input.GetKey(KeyCode.Space))
        {
            spaceButton.interactable = false;
        }
        else
        {
            spaceButton.interactable = true;
        }
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
        if (respawning) 
        {
            rb.velocity = new Vector2(0, 0);
            ParallaxController.speedMultiplier = 0;
            return;
        }

        ParallaxController.speedMultiplier = 1;

        // Move player
        if (autoMove) MovePlayer();

        if (isDashing)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(transform.right * speed * 2, ForceMode2D.Impulse);
            
            if (Time.time - dashStartTime >= dashDuration)
            {
                isDashing = false;
                dashEffect.SetActive(false);
                StopShakingCamera();
            }

            if (dashReleased)
            {
                dashReleased = false;
                isDashing = false;
                dashEffect.SetActive(false);
                StopShakingCamera();
            }
        }
        else if (isJumping)
        {
            rb.AddForce(-gravityDirection * gravity, ForceMode2D.Force);

            if (Time.time - jumpStartTime >= currentJumpDuration)
            {
                isJumping = false;
                hasLanded = false;
            }

            if (jumpReleased)
            {
                if (rb.velocity.y > 0.1f || rb.velocity.y < 0.1f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                }
                jumpReleased = false;
            }
        }
        else
        {
            anim.SetBool("jumpingDown", true);
            anim.SetBool("jumpingUp", false);
            if (!isDashing) rb.AddForce(gravityDirection * -gravity * downwardsGravityMultiplier, ForceMode2D.Force);
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

    void StartDash()
    {
        isDashing = true;
        dashStartTime = Time.time;
        dashEffect.SetActive(true);
        StartShakingCamera();
    }

    void InvertGravity()
    {
        gravityDirection = -gravityDirection;
        StopCoroutine(PlayArrowAnimation());
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
    }

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

    IEnumerator PlayRespawnAnimation()
    {
        if (respawning) yield break;
        respawning = true;
        sr.enabled = false;
        rb.velocity = new Vector2(0, 0);
        
        if (StartTutorialCoroutineInstance != null) StopCoroutine(StartTutorialCoroutineInstance);
        if (TutorialCoroutineInstance1 != null) StopCoroutine(TutorialCoroutineInstance1);
        if (TutorialCoroutineInstance2 != null) StopCoroutine(TutorialCoroutineInstance2);
        if (TutorialCoroutineInstance3 != null) StopCoroutine(TutorialCoroutineInstance3);
        if (TutorialCoroutineInstance4 != null) StopCoroutine(TutorialCoroutineInstance4);
        tutorialUI.SetActive(false);

        // pulseEffect.SetActive(true);
        // pulseEffect.GetComponent<Animator>().SetTrigger("pulse");
        // Invoke("DisablePulse", 0.5f);

        respawnEffect.transform.localRotation = Quaternion.Euler(0, 0, 0);
        respawnEffect.SetActive(true);

        for (int i = 0; i < respawnOrbs.Length; i++)
        {
            respawnOrbs[i].transform.localPosition = new Vector3(0, 0, 0);
            respawnOrbs[i].GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
        }

        // use leantween and rotate respawnEffect 360 degrees
        LeanTween.rotateZ(respawnEffect, 180, 2f).setEaseOutCubic();

        for (int i = 0; i < respawnOrbs.Length; i++)
        {
            LeanTween.color(respawnOrbs[i], new Color(1f, 1f, 1, 1), 1f).setEaseOutCubic();
        }

        float angle = 0;
        float angleIncrement = 360 / respawnOrbs.Length;
        float movingSpeed = 0.5f;
        float movingDistance = 0.3f;

        // use leantween and move the 8 orbs from the player outwards in a circle
        for (int i = 0; i < respawnOrbs.Length; i++)
        {
            LeanTween.moveLocalX(respawnOrbs[i], Mathf.Cos(angle * Mathf.Deg2Rad) * movingDistance, movingSpeed).setEaseOutCubic();
            LeanTween.moveLocalY(respawnOrbs[i], Mathf.Sin(angle * Mathf.Deg2Rad) * movingDistance, movingSpeed).setEaseOutCubic();
            angle += angleIncrement;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < respawnOrbs.Length; i++)
        {
            LeanTween.color(respawnOrbs[i], new Color(1f, 1f, 1, 0), 1f).setEaseOutCubic();
        }

        // use leantween and move the 8 orbs from the player back inwards in a circle
        for (int i = 0; i < respawnOrbs.Length; i++)
        {
            LeanTween.moveLocalX(respawnOrbs[i], 0, movingSpeed).setEaseOutCubic();
            LeanTween.moveLocalY(respawnOrbs[i], 0, movingSpeed).setEaseOutCubic();
            yield return new WaitForSeconds(0.05f);
        }

        // move the black panels to the right of the screen
        for (int i = 0; i < blackPanels.Length; i++)
        {
            blackPanels[i].anchoredPosition = new Vector2(1930, blackPanels[i].anchoredPosition.y);
            blackPanels[i].gameObject.SetActive(true);
        }

        // use leantween and move the black panels to the left of the screen
        for (int i = 0; i < blackPanels.Length; i++)
        {
            LeanTween.moveX(blackPanels[i], 0, 1f).setEaseOutCubic();
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1f);

        respawnEffect.SetActive(false);
        Respawn();

        // set player v cam tracked object offset y to 5
        playerVCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_TrackedObjectOffset.y = 5;

        // use leantween and move the black panels to the right of the screen
        for (int i = 0; i < blackPanels.Length; i++)
        {
            LeanTween.moveX(blackPanels[i], 1930, 1f).setEaseOutCubic();
            yield return new WaitForSeconds(0.05f);
        }

        playerVCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_TrackedObjectOffset.y = 0;
    }

    IEnumerator PromptTutorial(string textToShow, float durationToShow, int tutorialNumber)
    {
        // set tutorial text
        tutorialText.text = textToShow;

        // move tutorial UI to y = 500
        tutorialUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 450);
        tutorialUI.SetActive(true);

        // use leantween and move tutorial UI down
        LeanTween.moveLocalY(tutorialUI, 0, 1f).setEaseOutCubic();

        yield return new WaitForSeconds(1.5f);

        // spaceButton.interactable = false;
        // yield return new WaitForSeconds(0.1f);
        // spaceButton.interactable = true;

        yield return new WaitForSeconds(durationToShow);

        // use leantween and move tutorial UI up
        LeanTween.moveLocalY(tutorialUI, 450, 1f).setEaseOutCubic();

        yield return new WaitForSeconds(1f);

        // disable tutorial UI
        tutorialUI.SetActive(false);
    }

    IEnumerator PlayTutorialUI()
    {
        yield return new WaitForSeconds(4f);

        TutorialCoroutineInstance1 = PromptTutorial("Tap to Jump!", 5f, 1);
        StartCoroutine(TutorialCoroutineInstance1);

        yield return new WaitForSeconds(8f);

        TutorialCoroutineInstance2 = PromptTutorial("Hold to Jump longer!", 5f, 1);
        StartCoroutine(TutorialCoroutineInstance2);

        yield return new WaitForSeconds(9f);

        TutorialCoroutineInstance3 = PromptTutorial("Double-Tap quickly to flip gravity!", 5f, 1);
        StartCoroutine(TutorialCoroutineInstance3);
        allowInvert = true;

        yield return new WaitForSeconds(10f);
    }

    void StartShakingCamera()
    {
        playerVCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 1;
    }

    void StopShakingCamera()
    {
        playerVCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
    }

    void Respawn()
    {
        gravityDirection = Vector3.down;
        sr.flipY = false;
        transform.position = checkpointPosition;
        sr.enabled = true;
        respawning = false;
        StartTutorialCoroutineInstance = PlayTutorialUI();
        StartCoroutine(StartTutorialCoroutineInstance);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        print("collision"+ collision.gameObject.name);
        if (collision.gameObject.tag == "Box")
        {
            StartCoroutine(PlayRespawnAnimation());
        }
    }
}