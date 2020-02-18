using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //STATE
    bool isGrounded;
    bool isJumping;
    bool isBlocking;
    bool isDashing;
    bool isFalling;
    bool isAttacking;
    bool isAttackEffective;
    bool isWallSliding;


    //MOVEMENT
    [Header("Movement")]
    [Tooltip("Horizontal movement in units/s")]
    public float maxSpeed;
    public float wallSlideSpeed;

    Rigidbody2D rb2d;
    Vector2 moveInput;

    //JUMP
    [Header("Jump")]
    [Tooltip("Vertical movement in units/s while jumping")]
    public float flatJumpVelocity;
    [Tooltip("Maximum time that upwards force will be applied")]
    public float maxJumpTimeFlat;
    [Tooltip("On jump button release, vertical speed will be multiplied by this amount.")]
    public float jumpVelocityCut;
    public float minYVelocity = -10;

    int jumpCount = 0;
    Collider2D col2d;
    const float GROUND_CHECK_DIST = 0.1f;
    float jumpTime = 0;
    float fallTime;

    //DASH
    [Header("Dash")]
    public float dashSpeed;
    public float dashDuration;
    float timeDashing;
    int dashCount;

    //INTERACTION
    [Header("Interaction")]
    public float interactRange = 1;
    public float interactExitRange = 3;
    Collider2D[] colliderResults = new Collider2D[1];

    //LEVEL INFO
    public Vector3 currentCheckpoint;

    //ANIMATION
    [HideInInspector] public bool facingRight = true;
    Animator anim;

    //ATTACK
    [Header("Attack")]
    [Tooltip("Number of different attack animations in animator")]
    public int numAttackAnims;
    [Tooltip("After this number of seconds without attacking the attack animation number will reset to 0")]
    public float attackAnimResetDelay;
    public PlayerAttack[] attackPrefabs;
    PlayerAttack attackObj;
    int attackAnim;
    float timeSinceAttack;

    //RECOIL
    [Header("Recoil")]
    public float attackRecoilVelocity;
    public float recoilDecay;
    public float bounceVelocity;
    public float getHitRecoil;
    public float getHitBounce;
    float recoilVal;
    bool recoilRight;

    //BLOCK
    [Header("Block")]
    public float perfectBlockDuration = 0.5f;
    float blockTime;

    //HOOK
    [Header("Hook")]
    public GameObject hookPrefab;
    public float hookForce;
    public float hookRange;
    GameObject activeHook;
    Transform hookTarget;
    Vector3 hookOffset;

    void Awake()
    {
        col2d = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        facingRight = true;
        currentCheckpoint = transform.position;
    }

    void Update()
    {
        CheckIfGrounded();
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
        if (!isJumping && (isGrounded))
        {
            jumpCount = 1;
            dashCount = 1;
        }
        if (Input.GetButtonDown("Jump") && (jumpCount > 0||isWallSliding) && !isJumping && !isDashing)
        {
            //AttackEnd();
            isJumping = true;
            jumpCount -= 1;
        }
        /*if (Input.GetButtonDown("Dash") && dashCount > 0 && !isDashing)
        {
            if (isJumping)
            {
                StopJump();
            }
            isDashing = true;
            anim.SetBool("Dashing", true);
        }*/
        if (Input.GetButton("Block"))
        {
            blockTime += Time.deltaTime;
            isBlocking = true;
        }
        else
        {
            isBlocking = false;
            blockTime = 0;
        }
        timeSinceAttack += Time.deltaTime;
        if (timeSinceAttack > attackAnimResetDelay)
        {
            attackAnim = 0;
        }
        if (Input.GetButtonDown("Attack") && !isWallSliding)
        {
            Attack();
        }

        /*if (interact != null && Vector2.Distance(interact.transform.position, transform.position) > interactExitRange)
        {
            interact.OnLeaveRange();
            //Debug.Log (Vector3.Distance (interact.transform.position, transform.position) + "left range");
            interact = null;
        }
        if (interact == null)
        {
            foreach (Collider2D c in Physics2D.OverlapCircleAll(transform.position, interactRange))
            {
                Interactable i = c.GetComponent<Interactable>();
                if (i != null)
                {
                    interact = i;
                    break;
                }
            }
        }
        if (Input.GetButtonDown("Interact") && interact != null)
        {
            interact.OnInteract();
        }*/

    }

    void FixedUpdate()
    {
        
        float h = moveInput.x;
        
        //HORIZONTAL MOVEMENT (regular)
        if (!isDashing && recoilVal <= 0)
        {
            if (Mathf.Abs(h) > 0)
            {
                float speed = Mathf.Sign(h) * maxSpeed;
                rb2d.velocity = new Vector2(speed, rb2d.velocity.y);
            }
            else
            {
                rb2d.velocity = new Vector2(0, rb2d.velocity.y);
            }
        }

        //HORIZONTAL MOVEMENT (recoil)
        if (recoilVal > 0) {
            recoilVal -= Time.deltaTime * recoilDecay;
            rb2d.velocity = new Vector2(recoilVal * (recoilRight ? 1 : -1), rb2d.velocity.y);
            //rb2d.velocity = new Vector2(recoilX * attackRecoilX * (recoilTimeLeft / attackRecoilDuration), rb2d.velocity.y);
        }

        //WALL SLIDE
        bool wallRight = WallOnRight();
        bool wallLeft = WallOnLeft();
        if (!isJumping && !isGrounded && ((wallLeft && h < 0) || (wallRight && h > 0)))
        {
            if (!isWallSliding)
            {
                CancelAttack();
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
                //StopJump();
                isWallSliding = true;
            }
            /*if (h < 0 && !facingRight)
                Flip();
            else if (h > 0 && facingRight)
                Flip();*/
        }
        else {
            isWallSliding = false;
        }

        if (isWallSliding && rb2d.velocity.y < wallSlideSpeed) {
            rb2d.velocity = new Vector2(rb2d.velocity.x, wallSlideSpeed);
        }

        //JUMPING
        if (isJumping)
        {
            JumpVariableFlat();
        }

        if (isDashing)
        {
            Dash();
        }

        //FALLING
        isFalling = !isWallSliding && !isGrounded && !isJumping && rb2d.velocity.y < 0;
        if (isFalling)
        {
            fallTime += Time.fixedDeltaTime;
        }
        else
        {
            fallTime = 0;
        }

        //ANIMATION
        if (recoilVal <= 0)// && !isWallSliding)
        {
            if (h > 0 && !facingRight)
                Flip();
            else if (h < 0 && facingRight)
                Flip();
        }

        if (rb2d.velocity.y < minYVelocity) {
            rb2d.velocity = Vector2.Lerp(rb2d.velocity, new Vector2(rb2d.velocity.x,minYVelocity), 0.5f * Time.fixedDeltaTime);
        }
        anim.SetBool("Grounded", isGrounded);
        anim.SetFloat("Speed", Mathf.Abs(h));
        anim.SetFloat("FallTime", fallTime);
        anim.SetBool("Jumping", isJumping);
        anim.SetBool("Falling", isFalling);
        anim.SetBool("WallSliding", isWallSliding);
    }

    void Dash()
    {
        if (timeDashing > dashDuration)
        {
            rb2d.gravityScale = 1;
            timeDashing = 0;
            isDashing = false;
            anim.SetBool("Dashing", false);
            rb2d.velocity = Vector2.zero;
            dashCount -= 1;
        }
        else
        {
            rb2d.gravityScale = 0;
            rb2d.AddForce(dashSpeed * (facingRight ? Vector2.right : Vector2.left) * 10);
            Vector2 vel = rb2d.velocity;
            vel.y = 0;
            vel.x = Mathf.Clamp(vel.x, -dashSpeed, dashSpeed);
            rb2d.velocity = vel;
            timeDashing += Time.fixedDeltaTime;
        }

    }

    void JumpVariableFlat()
    {
        //Released jump button: end jump and cut velocity
        if (!Input.GetButton("Jump") && jumpTime > maxJumpTimeFlat * 0.1f)
        {
            Vector2 vel = rb2d.velocity;
            vel.y *= jumpVelocityCut;
            rb2d.velocity = vel;
            StopJump();
            return;
        }
        //Holding jump button: continue to rise
        else if (jumpTime < maxJumpTimeFlat)
        {
            Vector2 vel = rb2d.velocity;
            vel.y = flatJumpVelocity;
            rb2d.velocity = vel;
            jumpTime += Time.fixedDeltaTime;
        }
        //Hit ceiling
        if (rb2d.velocity.y < 0)
        {
            StopJump();
        }
    }

    void StopJump()
    {
        jumpTime = 0;
        isJumping = false;

    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {

        AttackBox box = col.GetComponent<AttackBox>();
        if (box != null && !isAttackEffective)
        {

            if (isBlocking && blockTime < perfectBlockDuration)
            {
                OnPerfectBlockAttack();
            }
            else if (isBlocking)
            {
                OnBlockAttack();
            }
            else
            {
                ReceiveDamage(col.transform.position);
            }
        }

        if (col.gameObject.CompareTag("Hazard"))
        {
            Die();
        }
    }

    void OnPerfectBlockAttack()
    {
        Debug.Log("perfect block");
    }

    void OnBlockAttack()
    {
        Debug.Log("regular block");
    }

    void ReceiveDamage(Vector3 source)
    {
        Debug.Log("take damage");
        recoilVal = getHitRecoil;
        rb2d.velocity = new Vector2(rb2d.velocity.x, getHitBounce);
        //-recoil

        //-lose control for duration
    }

    void Die()
    {
        transform.position = currentCheckpoint;
        rb2d.velocity = Vector2.zero;
    }

    void CheckIfGrounded()
    {
        int mask = 1 << LayerMask.NameToLayer("Ground");
        Vector2 left = new Vector3(col2d.bounds.min.x+0.05f, col2d.bounds.center.y);
        Vector2 center = new Vector3(col2d.bounds.center.x, col2d.bounds.center.y);
        Vector2 right = new Vector3(col2d.bounds.max.x-0.05f, col2d.bounds.center.y);
        Debug.DrawLine(left, left + Vector2.down * (col2d.bounds.extents.y + GROUND_CHECK_DIST), Color.red, 0.1f);
        Debug.DrawLine(center, center + Vector2.down * (col2d.bounds.extents.y + GROUND_CHECK_DIST), Color.red, 0.1f);
        Debug.DrawLine(right, right + Vector2.down * (col2d.bounds.extents.y + GROUND_CHECK_DIST), Color.red, 0.1f);


        isGrounded = Physics2D.Raycast(left, Vector2.down, col2d.bounds.extents.y + GROUND_CHECK_DIST, mask) ||
            Physics2D.Raycast(center, Vector2.down, col2d.bounds.extents.y + GROUND_CHECK_DIST, mask) ||
            Physics2D.Raycast(right, Vector2.down, col2d.bounds.extents.y + GROUND_CHECK_DIST, mask);
    }

    bool WallOnRight() {
        int mask = 1 << LayerMask.NameToLayer("Ground");
        Vector2 top = new Vector2(col2d.bounds.center.x,col2d.bounds.center.y);
        Vector2 bot = new Vector2(col2d.bounds.center.x, col2d.bounds.min.y);

        return Physics2D.Raycast(top, Vector2.right, col2d.bounds.extents.x + GROUND_CHECK_DIST, mask) ||
            Physics2D.Raycast(bot, Vector2.right, col2d.bounds.extents.x + GROUND_CHECK_DIST, mask);
    }

    bool WallOnLeft()
    {
        int mask = 1 << LayerMask.NameToLayer("Ground");
        Vector2 top = new Vector2(col2d.bounds.center.x, col2d.bounds.center.y);
        Vector2 bot = new Vector2(col2d.bounds.center.x, col2d.bounds.min.y);

        return Physics2D.Raycast(top, Vector2.left, col2d.bounds.extents.x + GROUND_CHECK_DIST, mask) ||
            Physics2D.Raycast(bot, Vector2.left, col2d.bounds.extents.x + GROUND_CHECK_DIST, mask);
    }

    void Attack()
    {
        if (isAttacking)
        {
            return;
        }
        timeSinceAttack = 0;
        isAttacking = true;
        isAttackEffective = false;
        anim.SetInteger("AttackNum", attackAnim);
        anim.SetBool("Attacking", true);
        attackAnim = (attackAnim + 1) % numAttackAnims;
    }

    void AttackEffectBegin()
    {
        isAttackEffective = true;
        attackObj = Instantiate(attackPrefabs[attackAnim], transform);
        attackObj.controller = this;
    }

    void AttackEffectEnd()
    {
        if (attackObj!=null)
        {
            Destroy(attackObj.gameObject);
        }
        isAttackEffective = false;
    }

    void AttackEnd()
    {
        isAttacking = false;
        anim.SetBool("Attacking", false);
    }

    void CancelAttack() {
        isAttacking = false;
        isAttackEffective = false;
        anim.SetBool("Attacking", false);
        if (attackObj!=null)
        {
            Destroy(attackObj.gameObject);
        }
    }

    public void OnHitEnemy(Enemy e) {
        recoilRight = !facingRight;// ? -1 : 1;//e.transform.position.x > transform.position.x ? 1 : -1;
        
        if (!isGrounded) {
            rb2d.velocity = new Vector2(rb2d.velocity.x, bounceVelocity);
        }
        else if (recoilVal <= 0)
        {
            recoilVal = attackRecoilVelocity;
        }

    }
}




