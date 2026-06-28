using System.Collections;
using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

     public float walkSpeed;
     public float sprintSpeed;
    public float slideSpeed;

    public float crouchSpeed;
    public float crouchYScale;
    public float startYScale;

    public float dsiredMoveSpeed;
    private float lastDsiredMoveSpeed;

    public float speedIncreaseMultiplayer;
    public float slopeIncreaseMultiplayer;
    public float wallrunSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("slope handling")]
    private RaycastHit slopeHit;
    private float maxSlopeAngle;
    private bool existingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

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

    public bool sliding;
    public bool wallrunning;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
             
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * moveSpeed * 20f, ForceMode.Force); 

            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f , ForceMode.Force);
            }
        }

       if(!wallrunning)  rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !existingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

    }

    private void Jump()
    {
        existingSlope = true;
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
        existingSlope = false;
    }

    private void StateHandler()
    {

        if (wallrunning)
        {
            state = MovementState.wallrunning;
            dsiredMoveSpeed = wallrunSpeed;
        }

        if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
                dsiredMoveSpeed = slideSpeed;

            else
                dsiredMoveSpeed = sprintSpeed;


        }

        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            dsiredMoveSpeed = sprintSpeed;
        }

        else if (grounded)
        {
            state = MovementState.walking;
            dsiredMoveSpeed = walkSpeed;

        }

        else
        {
            state = MovementState.air;
        }

        if (Mathf.Abs(dsiredMoveSpeed - lastDsiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothLerpMoveSpeed());
        }

        else
        {
            moveSpeed = dsiredMoveSpeed;
        }


        lastDsiredMoveSpeed = dsiredMoveSpeed;

        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            dsiredMoveSpeed = crouchSpeed;
        }
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
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
        float diffrence = Mathf.Abs(dsiredMoveSpeed - moveSpeed);
        float startVaue = moveSpeed;

        while (time < diffrence)
        {
            moveSpeed = Mathf.Lerp(startVaue, dsiredMoveSpeed, time / diffrence);
            yield return null;

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90);

                time += Time.deltaTime * speedIncreaseMultiplayer * slopeIncreaseMultiplayer * slopeAngleIncrease;
            }

            else
                time += Time.deltaTime * speedIncreaseMultiplayer;
        }

        moveSpeed = dsiredMoveSpeed;
    }
}