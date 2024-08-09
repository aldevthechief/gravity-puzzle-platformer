using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    [Header("moving and jumping properties")]
    public Rigidbody rb;
    private bool forcesapplied = true;
    private float x, z, inputmagnitude, speedmagnitude;
    public float speed;
    private float limitedspeed;
    public float groundvelmult;
    private float velocitymult;
    public float airmult;
    private Vector3 velocityChange;
    private Vector3 gravitymove;
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
    public Transform globalPlayerCam;
    public Transform localPlayerCam;
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
    private float currentslidespeed;

    [Header("wall jumping")]
    public bool iswalljumpallowed = true;
    public float walljumpforce;
    public float walljumpdistance;
    private bool nearwall = false;
    private bool haswalljumped = false;

    [Header("rolling")]
    public float rollspeed;
    public float rolltime;
    public float cooldown;

    public Animator rollanim;

    private float cooldowntimer;
    private bool rollmoveblock = false;
    private CapsuleCollider capsule;
    [HideInInspector] public bool isRolling = false;
    private float xrollinput, yrollinput;

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
    private bool canspawnparticles = true;
    private float lastparticlespawn;
    
    [Header("gravity manipulation")]
    public float gravityrotationtime = 0.75f;
    private GravityController gravcontroller;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        gravcontroller = GetComponent<GravityController>();

        readytojump = true;
        cooldowntimer = cooldown;
        capsule = GetComponents<CapsuleCollider>()[1];
        StartCoroutine(InstantiateTrail());
        rollparticles.Stop();
    }

    void Update()
    {
        limitedspeed = Mathf.Clamp(limitedspeed, 0, speed * 2);
        
        // speedmagnitude = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
        inputmagnitude = new Vector2(x, z).magnitude;

        // print(speedmagnitude);

        camtilt.SetFloat("horizontal", Input.GetAxis("HorizontalTilt"));

        LookRot();
        if(iswalljumpallowed) WallJump();

        isGrounded = Physics.CheckBox(gc.position, groundDistance * Vector3.one, Quaternion.identity, groundMask);

        if(isGrounded)
        {
            limitedspeed = speed;
            velocitymult = groundvelmult;
            haswalljumped = false;

            if(readytojump)
                lastgrounded = Time.time;

            if(!isRolling && !OnSteepSlope())
                state = MovementState.grounded;
        }
        else
        {
            limitedspeed += Time.fixedDeltaTime * jumpvelocityincreasemult;
            velocitymult = groundvelmult * airmult;
            state = MovementState.air;
            if(Time.time - lastparticlespawn > 0.5f)
                canspawnparticles = true;
        }

        if(isRolling)
            state = MovementState.rolling;
        if(OnSteepSlope())
            state = MovementState.sliding;

        if(rollmoveblock)
        {
            x = xrollinput * rollspeed;
            z = yrollinput * rollspeed;
        }
        else
        {
            x = Input.GetAxis("Horizontal") * limitedspeed;
            z = Input.GetAxis("Vertical") * limitedspeed;
        }

        if(OnSlope() && isGrounded)
        {
            Vector3 move = Vector3.ClampMagnitude(orientation.right * x + orientation.forward * z, limitedspeed);
            gravitymove = Vector3.ProjectOnPlane(move, slopehit.normal) + Vector3.Project(rb.velocity, slopehit.normal);
            rb.useGravity = false;
        }
        else
        {
            Vector3 move = Vector3.ClampMagnitude(orientation.right * x + orientation.forward * z, limitedspeed);
            gravitymove = move + Vector3.Project(rb.velocity, gravcontroller.groundup);

            if(!gravcontroller.isaffectedbygravity)
                rb.useGravity = true;
        }

        if(OnSteepSlope()) 
        {
            slopedir = Vector3.Cross(slopehit.normal, Vector3.Cross(slopehit.normal, gravcontroller.groundup)).normalized;
            SlopeSliding();
        }
        else currentslidespeed = slopeslidespeed;

        if(Input.GetButton("Jump"))
        {
            jumppress = Time.time;
        }

        if(Time.time - lastgrounded <= jumpgrace && Time.time - jumppress <= jumpgrace)
        {
            rb.AddForce(gravcontroller.groundup * jumpHeight, ForceMode.Impulse);
            Instantiate(jumpeffect, gc.position, Quaternion.identity);

            readytojump = false;
            Invoke(nameof(ResetJump), jumpcooldown);

            jumppress = null;
            lastgrounded = null;
        }

        if(Input.GetKeyDown(KeyCode.E) && cooldowntimer == cooldown && inputmagnitude != 0)
        {
            StartCoroutine(Roll());
            cooldowntimer = 0;
        }

        if(cooldowntimer < cooldown)
            cooldowntimer += Time.deltaTime;
        else
            cooldowntimer = cooldown;
    }

    Vector3 CalculateMovementVector(Vector3 dir) {return dir - rb.velocity;}

    public IEnumerator BlockForces(float forceblocktime)
    {
        forcesapplied = false;
        yield return new WaitForSeconds(forceblocktime);
        forcesapplied = true;
    }

    void FixedUpdate()
    {   
        if(forcesapplied) rb.AddForce(CalculateMovementVector(gravitymove) * velocitymult, ForceMode.Force);
        if(OnSteepSlope()) currentslidespeed += 0.1f;
    }

    void LookRot()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        Vector3 rot = localPlayerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        localPlayerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    void ResetJump()
    {
        readytojump = true;
    }

    bool OnSlope()
    {
        if(!isGrounded)
            return false;

        if(Physics.Raycast(transform.position, -gravcontroller.groundup, out slopehit))
        {
            float slopeangle = Vector3.Angle(slopehit.normal, gravcontroller.groundup);
            if(slopeangle > slopemin && slopeangle < slopelimit)
                return true;
        }
        return false;
    }

    bool OnSteepSlope()
    {
        if(!isGrounded)
            return false;

        if(Physics.Raycast(transform.position, -gravcontroller.groundup, out slopehit))
        {
            float slopeangle = Vector3.Angle(slopehit.normal, gravcontroller.groundup);
            if(slopeangle >= slopelimit)
                return true;
        }
        return false;
    }

    void SlopeSliding()
    {
        Vector3 slopemovedir = slopedir * currentslidespeed;
        gravitymove = Vector3.ProjectOnPlane(slopemovedir, slopehit.normal) + Vector3.Project(rb.velocity, gravcontroller.groundup);
    }

    void OnTriggerStay(Collider other)
    {
        nearwall = !isGrounded && other.CompareTag("Ground");
    }

    void OnTriggerExit(Collider other)
    {
        nearwall = !other.CompareTag("Ground");
    }

    bool CanWallJump()
    {
        return !Physics.Raycast(transform.position, -transform.up, walljumpdistance);
    }

    void WallJump()
    {
        if(Input.GetButton("Jump") && nearwall && CanWallJump() && readytojump && !haswalljumped)
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
                if(Physics.Raycast(transform.position, direction, out RaycastHit hit, 1.5f))
                {
                    Instantiate(jumpeffect, gc.position, Quaternion.identity);
                    Vector3 dir = (orientation.forward * Vector3.Dot(orientation.forward, hit.normal)).normalized;
                    rb.AddForce(dir * walljumpforce, ForceMode.Impulse);
                    haswalljumped = true;
                    break;
                }
            }
        }
    }

    IEnumerator Roll()
    {
        xrollinput = Input.GetAxis("Horizontal");
        yrollinput = Input.GetAxis("Vertical");
        float starttime = Time.time;
        while(Time.time < starttime + rolltime)
        {
            isRolling = true;
            rollmoveblock = true;
            rollanim.SetBool("canroll", true);
            capsule.height = 1;
            capsule.center = Vector3.down / 2;
            Vector3 move = Vector3.ClampMagnitude(orientation.right * x + orientation.forward * z, rollspeed);
            gravitymove = move + Vector3.Project(rb.velocity, gravcontroller.groundup);
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
        if(other.gameObject.CompareTag("Ground") && canspawnparticles)
        {
            Instantiate(jumpeffect, gc.position, Quaternion.identity);
            canspawnparticles = false;
            lastparticlespawn = Time.time;
        }
    }

    IEnumerator InstantiateTrail()
    {
        if(isGrounded)
            Instantiate(trail, gc.position, Quaternion.identity);
        yield return new WaitForSeconds(trailtime);
        StartCoroutine(InstantiateTrail());
    }

    public void NormalRotation()
    {
        Quaternion rotdir = Quaternion.FromToRotation(transform.up, gravcontroller.groundup);
        rb.freezeRotation = false;
        rb.DORotate((rotdir * rb.rotation).eulerAngles, gravityrotationtime);
        globalPlayerCam.DORotate((rotdir * globalPlayerCam.rotation).eulerAngles, gravityrotationtime);
        rb.freezeRotation = true;
    }
}
