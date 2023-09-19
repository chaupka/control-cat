using System;
using System.Collections;
using System.Linq;
using DungeonGeneration;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public struct PlatformingExtras
{
    public bool hasAntiGravityApex;
    public float baseGravityScale;
    public float apexGravityScale;
    public bool hasEarlyFall;
    public bool hasJumpBuffering;

    [HideInInspector]
    public bool isJumpBuffering;
    public float jumpBufferCast;
    public bool hasCoyoteTime;
    public float maxCoyoteTime;

    [HideInInspector]
    public float coyoteTimer;

    [HideInInspector]
    public bool isCoyoteJumping;
    public bool hasClampFallingSpeed;
    public float maxFallingSpeed;
    public bool hasBumpedHeadCorrection;
    public float bumpHeadShift;
    public float bumpHeadTolerance;
    public float bumpVelocityCap;
    public bool hasCornerClip;

    [HideInInspector]
    public Vector2 baseSize;

    [HideInInspector]
    public Vector2 baseOffset;
    public float cornerClipFactor;
}

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody2D rb;

    [HideInInspector]
    public BoxCollider2D collider2d;
    ContactFilter2D contactFilter2D;

    [HideInInspector]
    public float movingRight = 0;

    [SerializeField]
    float moveSpeed = 10.0f;

    [SerializeField]
    float acceleration = 1f;

    [SerializeField]
    float deceleration = -1f;

    [SerializeField]
    float moveVelPower = 1.2f;

    [SerializeField]
    float walkFriction = 0.2f;

    [HideInInspector]
    public float lookingUp = 0;

    [HideInInspector]
    public bool isPressingJump;

    [SerializeField]
    float jumpForce = 15f;

    // [SerializeField] float jumpMaxHeight = 5f;
    // [SerializeField] float jumpMaxTimeToPeak = 1f;
    float gravity;
    float jumpVelocity;
    bool canJump = true;

    [SerializeField]
    float maxJumpTime = 0.5f;
    float jumpTimer;

    [SerializeField]
    float headBumpRay = 1f;

    [SerializeField]
    float dashTime = 0.2f;

    [SerializeField]
    float dashRecoveryTime = 1f;

    [SerializeField]
    float dashForce;
    bool canDash = true;
    bool isDashing;
    bool IsGrounded => CheckIsGrounded();

    bool IsFalling => rb.velocity.y < 0;
    bool IsBumpingHead => !IsGrounded && !IsFalling && CheckIsBumpingHead();
    Vector2 leftHeadBumpEdge;
    Vector2 rightHeadBumpEdge;
    private DungeonStateController dungeonState;

    [SerializeField]
    PlatformingExtras platforming;

    private bool CheckIsGrounded()
    {
        if (rb.IsTouching(contactFilter2D))
        {
            if (platforming.hasCoyoteTime)
            {
                platforming.coyoteTimer = platforming.maxCoyoteTime;
            }
            return true;
        }

        if (platforming.hasCoyoteTime)
        {
            platforming.coyoteTimer -= Time.deltaTime;
        }
        if (platforming.hasCornerClip)
        {
            ClipCorner();
        }
        GameStateController.instance.cameraController.OffsetCamera(false);
        return false;
    }

    private bool CheckIsBumpingHead()
    {
        leftHeadBumpEdge = new Vector2(collider2d.bounds.min.x, collider2d.bounds.max.y);
        rightHeadBumpEdge = collider2d.bounds.max;
        return Physics2D
                .Raycast(
                    leftHeadBumpEdge + Vector2.right * platforming.bumpHeadTolerance,
                    Vector2.up,
                    headBumpRay,
                    LayerTag.terrainLayer
                )
                .collider != null
            || Physics2D
                .Raycast(
                    rightHeadBumpEdge - Vector2.right * platforming.bumpHeadTolerance,
                    Vector2.up,
                    headBumpRay,
                    LayerTag.terrainLayer
                )
                .collider != null;
    }

    private void ClipCorner()
    {
        if (!IsFalling)
        {
            collider2d.size = new Vector2(
                platforming.baseSize.x,
                platforming.baseSize.y - 0.01f * platforming.cornerClipFactor
            );
            collider2d.offset = new Vector2(
                platforming.baseOffset.x,
                platforming.baseOffset.y + 0.005f * platforming.cornerClipFactor
            );
        }
        else
        {
            collider2d.size = platforming.baseSize;
            collider2d.offset = platforming.baseOffset;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<BoxCollider2D>();
        jumpTimer = maxJumpTime;
        platforming.baseSize = collider2d.size;
        platforming.baseOffset = collider2d.offset;
        contactFilter2D.SetNormalAngle(89, 91);
        contactFilter2D.useNormalAngle = true;
        contactFilter2D.SetLayerMask(LayerTag.terrainLayer);
        contactFilter2D.useLayerMask = true;
        dungeonState = GameObject.Find("DungeonState").GetComponent<DungeonStateController>();
        StartCoroutine(CheckIsInRoom());
    }

    private IEnumerator CheckIsInRoom()
    {
        while (1 < 2)
        {
            yield return new WaitForSeconds(3);
            dungeonState.playerRoom = dungeonState.rooms.FirstOrDefault(
                r => r.tilePositions.Contains(Vector2Int.RoundToInt(transform.position))
            );
        }
    }

    #region update

    private void Update()
    {
        if (isDashing)
        {
            return;
        }
        // animation...
        Jump();
        DescendAfterHeadBump();
        if (platforming.hasAntiGravityApex)
        {
            AntiGravityApex();
        }
        if (platforming.hasClampFallingSpeed)
        {
            ClampFallingSpeed();
        }
    }

    private void Jump()
    {
        if (
            (platforming.isJumpBuffering && IsGrounded)
            || (isPressingJump && (canJump || platforming.isCoyoteJumping))
        )
        {
            platforming.isJumpBuffering = false;
            platforming.isCoyoteJumping = false;
            if (jumpTimer > 0)
            {
                // gravity = Physics2D.gravity.y * platforming.baseGravityScale;
                // rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                // jumpVelocity = (-0.5f * Physics2D.gravity.y * platforming.baseGravityScale * Mathf.Pow(jumpMaxTimeToPeak, 2) + jumpMaxHeight) / (jumpMaxTimeToPeak + maxJumpTime);
                // jumpVelocity = maxJumpTime * gravity + Mathf.Sqrt(Mathf.Pow(maxJumpTime * gravity, 2) - 2 * gravity * jumpMaxHeight);
                // rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimer -= Time.deltaTime;
                canJump = true;
            }
            else
            {
                jumpTimer = maxJumpTime;
                canJump = false;
            }
        }
    }

    private void DescendAfterHeadBump()
    {
        if (IsBumpingHead)
        {
            canJump = !isPressingJump && canJump;
            EarlyFall();
        }
        else if (platforming.hasBumpedHeadCorrection && !IsGrounded && !IsFalling)
        {
            BumpHeadCorrection();
        }
    }

    private void EarlyFall()
    {
        if (!IsFalling)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
    }

    private void BumpHeadCorrection()
    {
        var hitLeft = Physics2D.Raycast(
            leftHeadBumpEdge,
            Vector2.up,
            headBumpRay,
            LayerTag.terrainLayer
        );
        var hitRight = Physics2D.Raycast(
            rightHeadBumpEdge,
            Vector2.up,
            headBumpRay,
            LayerTag.terrainLayer
        );
        NudgePlayerWhenBumping(hitLeft, hitRight, false);
        NudgePlayerWhenBumping(hitRight, hitLeft, true);
    }

    private void NudgePlayerWhenBumping(RaycastHit2D hit, RaycastHit2D noHit, bool right)
    {
        if (
            rb.velocity.y > platforming.bumpVelocityCap * jumpForce
            && hit.collider != null
            && noHit.collider == null
        )
        {
            Tilemap tilemap;
            Vector3 tilePosition;
            float nudgeDistance;
            tilemap = hit.transform.GetComponent<Tilemap>();
            tilePosition = tilemap.WorldToCell(hit.point);
            nudgeDistance = right
                ? tilePosition.x - collider2d.bounds.max.x
                : tilePosition.x + tilemap.cellSize.x - collider2d.bounds.min.x;
            // rb.MovePosition(new Vector2(rb.position.x + nudgeDistance + Mathf.Sign(nudgeDistance) * platforming.bumpHeadShift, rb.position.y));
            rb.position = new Vector2(
                rb.position.x
                    + nudgeDistance
                    + Mathf.Sign(nudgeDistance) * platforming.bumpHeadShift,
                rb.position.y
            );
        }
    }

    private void AntiGravityApex()
    {
        rb.gravityScale = IsFalling ? platforming.apexGravityScale : platforming.baseGravityScale;
    }

    private void ClampFallingSpeed()
    {
        rb.velocity = new Vector2(
            rb.velocity.x,
            Mathf.Max(rb.velocity.y, platforming.maxFallingSpeed)
        );
    }
    #endregion

    #region fixed update
    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        Move();
        Friction();
    }

    private void Move()
    {
        float targetSpeed = movingRight * moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float walking =
            Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, moveVelPower) * Mathf.Sign(speedDiff);
        rb.AddForce(walking * Vector2.right);
    }

    private void Friction()
    {
        if (IsGrounded && Mathf.Abs(movingRight) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(walkFriction));
            amount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -amount);
        }
    }
    #endregion

    #region events
    public void OnJumpPerformed()
    {
        isPressingJump = true;
        if (platforming.hasJumpBuffering)
        {
            JumpBuffering();
        }
        if (platforming.hasCoyoteTime)
        {
            CoyoteJump();
        }
        canJump = IsGrounded && canJump;
    }

    public void OnJumpCancelled()
    {
        isPressingJump = false;
        canJump = true;
        jumpTimer = maxJumpTime;
        if (platforming.hasEarlyFall)
        {
            EarlyFall();
        }
    }

    public void OnLookingUpOrDownPerformed(float inputLookingUp)
    {
        lookingUp = inputLookingUp;
        if (
            Mathf.Abs(lookingUp) > 0.1f
            && IsGrounded
            && Mathf.Abs(movingRight) < 0.1f
            && Mathf.Abs(rb.velocity.y) < 0.01f
        )
        {
            GameStateController.instance.cameraController.OffsetCamera(true, lookingUp);
        }
    }

    public void OnLookingUpOrDownCancelled()
    {
        lookingUp = 0;
        GameStateController.instance.cameraController.OffsetCamera(false);
        // reset look animation bool
    }

    public void OnDashStarted()
    {
        if (canDash)
        {
            StartCoroutine(Dash());
        }
    }

    public void OnInteractPerformed()
    {
        // check if any gameobject exists

        // check if is on specific position in tilemap
        dungeonState.Interact(transform);
    }

    public void OnCopyPerformed()
    {
        dungeonState.CopyPlatform(Vector2Int.RoundToInt(transform.position));
    }
    #endregion

    private void JumpBuffering()
    {
        if (!IsGrounded && canJump && IsFalling)
        {
            RaycastHit2D hit = Physics2D.BoxCast(
                transform.position,
                collider2d.size,
                0,
                Vector2.down,
                platforming.jumpBufferCast,
                LayerTag.terrainLayer
            );
            if (hit.collider != null)
            {
                platforming.isJumpBuffering = true;
            }
        }
    }

    private void CoyoteJump()
    {
        if (IsFalling && platforming.coyoteTimer > 0)
        {
            platforming.isCoyoteJumping = true;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        var dashDirection = new Vector3(movingRight, lookingUp).normalized;
        dashDirection = dashDirection.magnitude != 1 ? Vector2.right : dashDirection;
        rb.velocity = dashDirection.normalized * dashForce;
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = originalGravity;
        isDashing = false;
        var timer = Time.time;
        yield return new WaitUntil(() =>
        {
            return Time.time - timer > dashRecoveryTime && IsGrounded;
        });
        canDash = true;
    }
}
