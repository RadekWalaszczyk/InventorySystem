using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    //Assingables
    [SerializeField] Transform playerCam;
    [SerializeField] Transform orientation;
    [SerializeField] Transform head;

    //Other
    private Rigidbody rb;
    private CapsuleCollider col;

    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;

    private float zRotation;
    [Header("Mouse")]
    [Tooltip("Mouse sensivity")]
    [Range(0.5f, 5f)]
    [SerializeField] float sensMultiplier = 1f;

    //Movement
    [Header("Movement")]
    [Tooltip("Acceleration")]
    [SerializeField] float moveSpeed = 4500;
    [Tooltip("Maximum speed")]
    [SerializeField] float maxSpeed = 20;
    [SerializeField] bool grounded;
    [SerializeField] LayerMask whatIsGround;

    private float threshold = 0.01f;
    [Tooltip("Direction change smoothness")]
    [SerializeField] float counterMovement = 0.175f;
    [SerializeField] float maxSlopeAngle = 35f;

    //Crouch & Slide
    private float crouchColHeight = 1f;
    private float colHeight; 
    private bool sliding = false, canSliding = true;
    private float headPosY;

    [Header("Crounching")]
    [SerializeField] float standingHeadHeight;
    [SerializeField] float crouchingHeadHeight;

    [Header("Slide")]
    [Tooltip("Slide force")]
    [SerializeField] float slideForce = 400;
    [Tooltip("Slide friction")]
    [SerializeField] float slideCounterMovement = 0.2f;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    private float jumpCoyoteTime = 0f;

    [Header("Jumping")]
    [Tooltip("Jump force")]
    [SerializeField] float jumpForce = 550f;

    [Header("Camera")]
    [Tooltip("Left-Right camera angle")]
    [SerializeField] float cameraRotZ = 10f;
    [Tooltip("Camera to angle smoothness")]
    [SerializeField] float cameraRotZInMultiplayer = 1f;
    [Tooltip("Camera back to normal smootness")]
    [SerializeField] float cameraRotZOutMultiplayer = 1f;

    //Input
    float x, y;
    bool jumping, throwing, crouching, disableOtherForces;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 dir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    void Start()
    {
        colHeight = col.height;
        headPosY = head.localPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void FixedUpdate()
    {
        Movement();
        Gravity();
        CheckIfGround();
    }

    public void UpdateMovement()
    {
        MyInput();
        Look();
        ChangePlayerHeadPos();
    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            jumping = true;
            jumpCoyoteTime = 0.15f;
        }

        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl) && !CheckIfSomethingAbove())
        {
            crouching = !crouching;

            if (crouching)
                StartCrouch();
            else
                StopCrouch();
        }
    }

    private void StartCrouch()
    {
        crouching = true;

        col.height = crouchColHeight;
        col.center = new Vector3(0f, 0.75f, 0f);
        headPosY = crouchingHeadHeight;
        if (canSliding && rb.velocity.magnitude > 0.5f && y > 0)
        {
            if (grounded && !CheckIfSomethingAbove())
            {
                rb.AddForce(dir * slideForce);
                sliding = true;
                canSliding = false;
                Invoke(nameof(LetSliding), 1f);
            }
        }
    }

    private void StopCrouch()
    {
        crouching = false;
        sliding = false;

        col.height = colHeight;
        col.center = new Vector3(0f, 1f, 0f);
        headPosY = standingHeadHeight;
    }

    private void Movement()
    {
        dir = Vector3.ProjectOnPlane(orientation.transform.forward, normalVector);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        jumpCoyoteTime -= Time.deltaTime;
        //If holding jump && ready to jump, then jump
        if (readyToJump && jumpCoyoteTime > 0 && grounded)
        {
            Jump();
            jumpCoyoteTime = 0f;
        }

        //Set max speed
        float maxSpeed = this.maxSpeed;

        // Start crouching when sliding ends
        if (rb.velocity.magnitude < 2f)
        {
            if (grounded && sliding)
                sliding = false;
        }

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (grounded && sliding) multiplierV = 0f;

        // Movement while crouching
        if (grounded && crouching)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
            maxSpeed = maxSpeed / 2f;
        }

        if (IsOnSlope())
        {
            multiplier = 1.5f;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        if (disableOtherForces) return;

        //Apply forces to move player
        rb.AddForce(dir * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
            StopCrouch();
            LetSliding();
        }
    }

    private void ChangePlayerHeadPos()
    {
        float currHeadPosY = Mathf.Lerp(head.localPosition.y, headPosY, Time.fixedDeltaTime);
        head.localPosition = new Vector3(head.localPosition.x, currHeadPosY, head.localPosition.z);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Gravity()
    {
        if (sliding && grounded) return;
        if (x == 0 && y == 0 && IsOnSlope() && grounded && rb.velocity.y <= 0) return;

        Vector3 rbVel = rb.velocity;
        rbVel.z = 0f;
        if (x == 0 && y == 0 && rbVel.magnitude < 0.1f && grounded) return;

        rb.AddForce(Vector3.down * Time.deltaTime * 2200f);
    }

    private bool CheckIfSomethingAbove()
    {
        return Physics.Raycast(transform.position + Vector3.up * 1.2f, transform.TransformDirection(Vector3.up), out var hit, 0.5f, LayerMask.GetMask("Default"));
    }

    private bool IsOnSlope()
    {
        return normalVector != Vector3.up;
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        //Rotate camera while moving horizontal
        Vector3 input = -new Vector3(x, 0 , y);
        zRotation += input.x * Time.fixedDeltaTime * cameraRotZInMultiplayer;
        zRotation = Mathf.Clamp(zRotation, -cameraRotZ, cameraRotZ);

        if (x == 0)
        {
            if (zRotation > 0)
                zRotation -= Time.fixedDeltaTime * cameraRotZOutMultiplayer;
            else if (zRotation < 0)
                zRotation += Time.fixedDeltaTime  * cameraRotZOutMultiplayer;
        }

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, zRotation);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        float speed = Vector3.Magnitude(rb.velocity);
        if (speed > maxSpeed)
        {
            float brakeSpeed = maxSpeed;  // calculate the speed decrease
            Vector3 brakeVelocity = rb.velocity.normalized * brakeSpeed;  // make the brake Vector3 value

            rb.AddForce(new Vector3(-brakeVelocity.x, 0f, -brakeVelocity.z));  // apply opposing brake force
        }

        if (!grounded || jumping || throwing) return;

        //Slow down sliding
        if (sliding)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * dir * Time.deltaTime * -mag.y * counterMovement);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    private void CheckIfGround()
    {
        var colliders = Physics.RaycastAll(transform.position + Vector3.up * 0.2f, Vector3.down, 1f, whatIsGround, QueryTriggerInteraction.Ignore).ToList();
        if (colliders.Count <= 0)
        {
            grounded = false;
            return;
        }

        //Iterate through every collision in a physics update
        for (int i = 0; i < colliders.Count; i++)
        {
            Vector3 normal = colliders[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                // Canceling throwing after hit the ground
                if (throwing && !grounded)
                    throwing = false;

                if (jumping)
                    jumping = false;

                grounded = true;
                normalVector = normal;
            }
        }
    }

    private void LetSliding()
    {
        canSliding = true;
    }
}