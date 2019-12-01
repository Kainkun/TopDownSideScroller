using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [HideInInspector]
    public static Player script;

    bool isTopDown;
    float isTopDownTime;

    [SerializeField]
    bool usingController;
    [SerializeField]
    float transitionSpeed = 2f;
    [SerializeField]
    GameObject Bullet;
    [SerializeField]
    GameObject BulletTrail;
    [SerializeField]
    float aimSpeed;
    public Transform gunPivot;
    [SerializeField]
    float shootPower = 5;

    float gunReload;

    [Header("Top Down")]

    [SerializeField]
    float TDAcceleration = 100;
    [SerializeField]
    float TDmaxMoveSpeed = 5;
    [SerializeField]
    float TDstopSpeedPercent = 0.5f;

    [Header("Side Scroll")]

    [SerializeField]
    float Acceleration = 100;
    [SerializeField]
    float gravityScale = 5;
    [SerializeField]
    float maxMoveSpeed = 5;
    [SerializeField]
    float stopSpeedPercent = 0.5f;
    [SerializeField]
    float jumpForce = 15f;
    [SerializeField]
    float maxFallSpeed = 20f;


    Vector2 move, aimDirection;
    public float clicking;

    InputMaster controls;
    Rigidbody2D rb;

    private void Awake()
    {
        script = this;

        rb = GetComponent<Rigidbody2D>();
        controls = new InputMaster();

        //controls.Player.Shoot.performed += ctx => raycastShoot();
        controls.Player.Shoot.performed += ctx => clicking = ctx.ReadValue<float>();
        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Movement.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Player.Movement.canceled += ctx => move = Vector2.zero;
        controls.Player.Aim.performed += ctx => aimDirection = ctx.ReadValue<Vector2>();
        controls.Player.Aim.canceled += ctx => aimDirection = ctx.ReadValue<Vector2>();

    }

    void Start()
    {
        SetSideScroll();
    }


    void Update()
    {
        Aim();
        gunReload += Time.deltaTime;
        while(clicking >= 1 && gunReload > 0.1f)
        {
            raycastShoot();
            projectileShoot();
            gunReload = 0;
        }

        if (isTopDown)
            isTopDownTime += Time.deltaTime * transitionSpeed;
        else
            isTopDownTime -= Time.deltaTime * transitionSpeed;

        isTopDownTime = Mathf.Clamp(isTopDownTime, 0f, 1f);

        rb.gravityScale = Mathf.SmoothStep(gravityScale, 0, isTopDownTime);
        TDstopSpeedPercent = Mathf.SmoothStep(0.1f, 0.5f, isTopDownTime);

        if (Input.GetKeyDown(KeyCode.Space))
            FlipControlMode();
    }

    private void FixedUpdate()
    {
        if (isTopDown)
            TopDownMove();
        else
            SideScrollMove();
    }


    //Movement

    void TopDownMove()
    {
        if (move.x == 0)
            rb.velocity -= new Vector2(rb.velocity.x * TDstopSpeedPercent, 0);
        else if (rb.velocity.x < TDmaxMoveSpeed * move.x && move.x > 0)
            rb.AddForce(new Vector2(move.x * TDAcceleration, 0), ForceMode2D.Force);
        else if (rb.velocity.x > TDmaxMoveSpeed * move.x && move.x < 0)
            rb.AddForce(new Vector2(move.x * TDAcceleration, 0), ForceMode2D.Force);

        if (move.y == 0)
            rb.velocity -= new Vector2(0, rb.velocity.y * TDstopSpeedPercent);
        else if (rb.velocity.y < TDmaxMoveSpeed * move.y && move.y > 0)
            rb.AddForce(new Vector2(0, move.y * TDAcceleration), ForceMode2D.Force);
        else if (rb.velocity.y > TDmaxMoveSpeed * move.y && move.y < 0)
            rb.AddForce(new Vector2(0, move.y * TDAcceleration), ForceMode2D.Force);



    }

    void SideScrollMove()
    {
        if (move.x == 0)
            rb.velocity -= new Vector2(rb.velocity.x * stopSpeedPercent, 0);
        else if (rb.velocity.x < maxMoveSpeed * move.x && move.x > 0)
            rb.AddForce(new Vector2(move.x * Acceleration, 0), ForceMode2D.Force);
        else if (rb.velocity.x > maxMoveSpeed * move.x && move.x < 0)
            rb.AddForce(new Vector2(move.x * Acceleration, 0), ForceMode2D.Force);

        if (-rb.velocity.y > maxFallSpeed)
            rb.velocity -= new Vector2(0, maxFallSpeed + rb.velocity.y);

    }

    void Jump()
    {
        if(!isTopDown && IsGrounded())
        {
            rb.velocity -= new Vector2(0, rb.velocity.y);
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0, Vector2.down, 0.25f + Mathf.Epsilon);
        return hit.collider != null;
    }

    //Action
    Vector2 lookDirection;
    void Aim()
    {
        Vector2 mouseScreenPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lookDirection = (mouseScreenPosition - (Vector2)transform.position).normalized;

        if(usingController)
            lookDirection = aimDirection;

        Vector2 lerpAimDirection;

        lerpAimDirection = Vector3.Lerp(gunPivot.up, lookDirection, Time.deltaTime * aimSpeed);
        gunPivot.up = lerpAimDirection;
    }

    void projectileShoot()
    {
        GameObject tempBullet = Instantiate(Bullet, transform.position + (Vector3)lookDirection, Quaternion.identity);
        Rigidbody2D tempBulletRB = tempBullet.GetComponent<Rigidbody2D>();
        tempBulletRB.AddForce(lookDirection * shootPower, ForceMode2D.Impulse);
        if (isTopDown)
            tempBulletRB.gravityScale = 0;

        Destroy(tempBullet, 30);
    }

    void raycastShoot()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, gunPivot.up);
        GameObject tempTrail = Instantiate(BulletTrail, transform.position, Quaternion.identity);
        tempTrail.transform.up = lookDirection + Random.insideUnitCircle/50;
        Destroy(tempTrail, 0.3f);

    }

    //Mechanics

    void FlipControlMode()
    {
        if (isTopDown)
            SetSideScroll();
        else
            SetTopDown();
    }

    void SetTopDown()
    {
        isTopDown = true;
    }

    void SetSideScroll()
    {
        isTopDown = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "TopDown")
        {
            SetTopDown();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "TopDown")
        {
            SetSideScroll();
        }
    }




    //Input manager
    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
