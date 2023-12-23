using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("moving and jumping properties")]
    public Rigidbody rb;
    private float x, z, inputmagnitude, speedmagnitude;
    public float speed;
    private float limitedspeed;
    public float groundvelmult;
    private float velocitymult;
    public float airmult;
    private Vector3 velocityChange;
    public Transform gc;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;
    public float jumpHeight;
    public float jumpgrace;
    public float jumpcooldown;
    public float jumpvelocityincreasemult;
    private bool readytojump;
    private float? lastgrounded;
    private float? jumppress;

    [Header("camera movement")]
    public Transform playerCam;
    public Transform orientation;
    private float xRotation;
    public float sensitivity = 50f;
    public float sensMultiplier = 1f;
    private float desiredX;
    public Animator camtilt;

    [Header("slopa movement")]
    public float slopemin;
    public float slopelimit;
    public float slopeslidespeed;
    public float antibumpforce;
    private RaycastHit slopehit;
    private Vector3 slopedir;

    [Header("wall jumping")]
    public float walljumpforce;
    public float walljumpdistance;
    private bool nearwall = false;
    private bool touchedgrass = false;

    [Header("rolling")]
    public float rollspeed;
    public float rolltime;
    public float cooldown;

    public Animator rollanim;

    private float cooldowntimer;
    private bool rollmoveblock = false;
    private float rolldirx, rolldirz;
    private CapsuleCollider capsule;
    [HideInInspector] public bool isRolling = false;

    [Header("current state")]
    public MovementState state;
    public enum MovementState
    {
        grounded,
        air,
        rolling,
        sliding
    }

    [Header("particles")]
    public GameObject jumpeffect;
    public GameObject trail;
    public ParticleSystem rollparticles;
    public float trailtime;
    private bool doOffset = true;
    private float offsetlasttime;
    
    [Header("other shit")]
    public PlayerInteraction playerinter;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        readytojump = true;
        cooldowntimer = cooldown;
        capsule = GetComponents<CapsuleCollider>()[1];
        StartCoroutine(InstantiateTrail());
        rollparticles.Stop();
    }

    void Update()
    {
        limitedspeed = Mathf.Clamp(limitedspeed, 0, speed * 2);
        
        speedmagnitude = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
        inputmagnitude = new Vector2(x, z).magnitude;

        // print(speedmagnitude);

        camtilt.SetFloat("horizontalspeed", speedmagnitude);
        camtilt.SetFloat("horizontal", Input.GetAxis("HorizontalTilt"));

        LookRot();
        WallJump();
        isGrounded = Physics.CheckSphere(gc.position, groundDistance, groundMask);

        if(isGrounded && readytojump)
        {
            readytojump = false;
            Invoke(nameof(ResetJump), jumpcooldown);
            lastgrounded = Time.time;
        }

        if(!isGrounded)
        {
            limitedspeed += Time.deltaTime * jumpvelocityincreasemult;
            velocitymult = groundvelmult * airmult;
            state = MovementState.air;
            if(Time.time - offsetlasttime > 0.5f)
                doOffset = true;
        }
        else
        {
            limitedspeed = speed;
            velocitymult = groundvelmult;
            touchedgrass = true;
        }

        if(isGrounded && !isRolling && !OnSteepSlope())
            state = MovementState.grounded;
        if(isRolling)
            state = MovementState.rolling;
        if(OnSteepSlope())
            state = MovementState.sliding;

        if(rollmoveblock == true)
        {
            x = rollspeed * rolldirx;
            z = rollspeed * rolldirz;
        }
        else
        {
            x = Input.GetAxis("Horizontal") * limitedspeed;
            z = Input.GetAxis("Vertical") * limitedspeed;
        }

        Vector3 move = orientation.transform.right * x + orientation.transform.forward * z;
        Vector3 newmove = new Vector3(move.x, rb.velocity.y, move.z);
        if(!OnSteepSlope())
            CalculateMovementVector(newmove);

        if(Input.GetButton("Jump"))
        {
            jumppress = Time.time;
        }

        if(Time.time - lastgrounded <= jumpgrace && Time.time - jumppress <= jumpgrace)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            Instantiate(jumpeffect, gc.position, Quaternion.identity);
            jumppress = null;
            lastgrounded = null;
        }

        if(OnSteepSlope())
            SlopeSliding();

        slopedir = Vector3.up - slopehit.normal * Vector3.Dot(Vector3.up, slopehit.normal);

        if(Input.GetKeyDown(KeyCode.E) && cooldowntimer == cooldown && inputmagnitude != 0)
        {
            StartCoroutine(Roll());
            cooldowntimer = 0;
        }

        if(cooldowntimer < cooldown)
        {
            cooldowntimer += Time.deltaTime;
        }

        if(cooldowntimer > cooldown)
        {
            cooldowntimer = cooldown;
        }
    }

    void CalculateMovementVector(Vector3 dir)
    {
        velocityChange = dir - rb.velocity;
        velocityChange = Vector3.ClampMagnitude(velocityChange, limitedspeed);
    }

    void FixedUpdate()
    {
        rb.AddForce(velocityChange * velocitymult, ForceMode.Force);

        if(isGrounded && OnSlope())
        {
            rb.AddForce(Vector3.Cross(slopedir, slopehit.transform.right).normalized * antibumpforce, ForceMode.Force);
            rb.AddForce(Vector3.down * 12.5f, ForceMode.Force);
            rb.useGravity = false;
        }
        else if(!playerinter.isaffectedbygravity)
        {
            rb.useGravity = true;
        }
    }

    void LookRot()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    void ResetJump()
    {
        readytojump = true;
    }

    bool OnSlope()
    {
        if(!isGrounded)
            return false;

        if(Physics.Raycast(transform.position, Vector3.down, out slopehit))
        {
            float slopeangle = Vector3.SignedAngle(slopehit.normal, Vector3.up, Vector3.up);
            if(slopeangle > slopemin)
                return true;
        }
        return false;
    }

    bool OnSteepSlope()
    {
        if(!isGrounded)
            return false;

        if(Physics.Raycast(transform.position, Vector3.down, out slopehit))
        {
            float slopeangle = Vector3.SignedAngle(slopehit.normal, Vector3.up, Vector3.up);
            if(slopeangle > slopelimit)
                return true;
        }
        return false;
    }

    void SlopeSliding()
    {
        float slidespeed = limitedspeed + slopeslidespeed + Time.deltaTime;

        Vector3 move = slopedir * -slidespeed;
        Vector3 newmove = new Vector3(move.x, rb.velocity.y - slopehit.point.y, move.z);
        CalculateMovementVector(newmove);
    }

    void OnTriggerStay(Collider other)
    {
        if(isGrounded == false && other.CompareTag("Ground"))
        {
            nearwall = true;
        }
        else
        {
            nearwall = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        nearwall = false;
    }

    bool CanWallJump()
    {
        return !Physics.Raycast(transform.position, Vector3.down, walljumpdistance);
    }

    void WallJump()
    {
        if(Input.GetButton("Jump") && nearwall && CanWallJump() && readytojump)
        {
            Vector3[] Directions = new Vector3[]
            {
                transform.forward,
                -transform.forward,
                transform.right,
                -transform.right
            };
 
            foreach(Vector3 direction in Directions)
            {
                if(Physics.Raycast(transform.position, direction, out RaycastHit hit, 1.5f) && touchedgrass)
                {
                    Instantiate(jumpeffect, gc.position, Quaternion.identity);
                    Vector3 dir = (orientation.transform.forward * Vector3.Dot(orientation.transform.forward, hit.normal)).normalized;
                    rb.AddForce(dir * walljumpforce, ForceMode.Impulse);
                    touchedgrass = false;
                    break;
                }
            }
        }
    }

    IEnumerator Roll()
    {
        rolldirx = Input.GetAxis("Horizontal");
        rolldirz = Input.GetAxis("Vertical");
        float starttime = Time.time;
        while(Time.time < starttime + rolltime)
        {
            isRolling = true;
            rollmoveblock = true;
            rollanim.SetBool("canroll", true);
            capsule.height = 1;
            capsule.center = Vector3.down / 2;
            Vector3 move = orientation.transform.right * x + orientation.transform.forward * z;
            Vector3 newmove = new Vector3(move.x, rb.velocity.y, move.z);
            CalculateMovementVector(newmove);
            yield return null;
            rollparticles.Play();
        }
        isRolling = false;
        rollmoveblock = false;
        rollanim.SetBool("canroll", false);
        rollparticles.Stop();
        capsule.height = 2;
        capsule.center = Vector3.zero;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Ground") && doOffset)
        {
            Instantiate(jumpeffect, gc.position, Quaternion.identity);
            doOffset = false;
            offsetlasttime = Time.time;
        }
    }

    IEnumerator InstantiateTrail()
    {
        if(isGrounded)
            Instantiate(trail, gc.position, Quaternion.identity);
        yield return new WaitForSeconds(trailtime);
        StartCoroutine(InstantiateTrail());
    }
}
