using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class playerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 10f;
    public float slideSpeed = 15f;
    public float crouchSpeed = 3.5f;
    public float wallrunSpeed = 12f;

    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [Header("Speed Multipliers")]
    public float speedIncreaseMultiplier = 1.5f;
    public float slopeIncreaseMultiplier = 2.5f;
    public float airMultiplier = 0.4f;

    [Header("Jumping & Drag")]
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float groundDrag = 5f;
    private bool readyToJump = true;

    [Header("Crouching")]
    public float crouchYScale = 0.5f;
    private float startYScale;
    private Vector3 crouchScale;
    private Vector3 playerScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    private bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;

    // Movement States
    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        air,
        crouching,
        sliding
    }

    [HideInInspector] public bool sliding;
    [HideInInspector] public bool wallrunning;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // ????? ???? ????? ?????? ???? ???? ?????
        playerScale = transform.localScale;
        startYScale = playerScale.y;
        crouchScale = new Vector3(playerScale.x, crouchYScale, playerScale.z);
    }

    private void Update()
    {
        // ????? ???? ???? ?? ???? (Ground Check)
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // ?????? ?????? (Drag)
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // ???? ???
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // ???? ?????
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = crouchScale;
            // ?? ???? ?????? ?? ??? ????? ???? ?????? ??????? ?? ???? ????? ?????
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // ???? ??? ?? ???? ?????
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = playerScale;
        }
    }

    private void StateHandler()
    {
        // 1. ???? ???????????
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        // 2. ???? ????? (Slide)
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }
        // 3. ???? ????? (Crouch)
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        // 4. ???? ????? ???? (Sprint)
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // 5. ???? ??? ???? ????
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // 6. ???? ???? ?? ???
        else
        {
            state = MovementState.air;
        }

        // ????? ??? ????? ???? ??????? ???? (Lerp)
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // ?????? ??? ???? ?? ???? ??? ????? ? ????? ???????
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // ???) ???? ??? ??? ???????
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            // ??????? ?? ??????? ??? ? ???????? ??? ??? ?? ??? ????
            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // ?) ???? ??? ???? ???
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // ?) ???? ?? ???
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // ??????? ???? ????? ??? ??? ???? ??????? ?? ?? ????? ?????? ?? ?????
        if (!wallrunning)
            rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // ????? ???? ???? ??? ???
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        // ????? ???? ???? ??? ???? ?? ??? (??? ?? ??????? X ? Z)
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // ??? ???? ???? Y ???? ????? ???? ??? ????? ????? ????
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private IEnumerator SmoothLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        // ??????? ?? ??? ?? ??? ???????? ?? ???? ????? ???? ???????
        if (difference < 0.01f)
        {
            moveSpeed = desiredMoveSpeed;
            yield break;
        }

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            yield return null;

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1f + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }
        }

        moveSpeed = desiredMoveSpeed;
    }
}
