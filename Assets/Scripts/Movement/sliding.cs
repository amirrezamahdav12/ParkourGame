using UnityEngine;

public class sliding : MonoBehaviour
{
    [Header("refrences")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovment pm;

    [Header("sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;


    public float slideXScale;
    public float startYScale;

    [Header("input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;


    private bool issliding;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovment>();

        startYScale = playerObj.localScale.y;

    }

    private void startSlide()
    {
        pm.sliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideXScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }


        if (slideTimer <= 0)
        {
            stopSlide();
        }
    }

    private void stopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);

    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (verticalInput != 0 || verticalInput != 0))
            startSlide();

        if (Input.GetKeyUp(slideKey) && pm.sliding)
        {
            stopSlide();
        }


    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }
}