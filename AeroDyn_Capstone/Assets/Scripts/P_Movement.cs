using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { IDLE, AERO, DYN, HIT, BLOCK, CHARGE };
public enum ControlMode { KEYBOARD, CONTROLLER };
public enum DynElement { AERO, HYDRO, LITHO, PYRO };
//This Script handles player movement and reads controller inputs.
public class P_Movement : MonoBehaviour
{
    //Button / Input management
    [Header("Player Button / Input settings")]
    public ControlMode mode;
    #region Button variables
    //Jump Button
    private bool jumpJustDown;
    private float jumpInput = 0;
    private int jumpCost = 1;
    //Aero Button
    private bool aeroJustDown, aeroJustUp;
    private float aeroInput = 0;
    private float aeroHold = 0;
    //Dyn Button
    private bool dynJustDown, dynJustUp;
    private float dynInput = 0;
    private float dynHold = 0;
    //Breath Button
    private bool breathJustDown, breathJustUp;
    private float breathInput = 0;
    private float breathHold = 0;
    //Block Button
    private bool blockJustUp;
    private float blockInput = 0;
    private float blockHold = 0;
    //Dyn Switch Buttons
    private bool dynNextDown, dynPrevDown;
    private float dynNextInput = 0;
    private float dynPrevInput = 0;
    //
    //LStick Axes
    private float hInput;
    private float vInput;
    #endregion

    //State Mechanics
    public PlayerState myState = PlayerState.IDLE;
    public DynElement elementState = DynElement.AERO;
    public int maxElement = 0;

    //External Object Components / references
    [Header("Object components / references")]
    public GameObject bullet; //Access bullets from an external list later
    public GameObject shield;
    public GameObject counter;
    private GameObject currentShield;
    private Rigidbody2D RB;
    private LineRenderer LR;

    //Player physics controls
    [Header("Player physics / Contact checks")]
    public float moveSpeed;
    public float moveAcc;
    public float jumpSpeed;
    public bool facingRight = true;
    public float baseGravity = 2;
    //Ground / contact check
    public float groundCheckDist = 1.0f;
    public float grounded = 0.5f; //Is the player on the ground, or just walked off the ground? Also used for jump buffer
    public float coyoteTime = 0.5f;
    public LayerMask GroundLayer;
    public float slide = 0;
    //Dyn controls
    public float VSpeedMax = 10;
    public float TornadoDynForce = 2.0f;
    public float dynTime = 0; //Multi-purpose variable for measuring dyn duration
    public float fireMult;
    public bool fireRecoil;
    public float fireMax;

    //Player resources
    [Header("Player stats / resources")]
    public float breath = 0;
    public float maxBreath;

    //Platform movement
    public Vector3 platMove;
    public Vector3 speed;

    //Health / Respawn system
    private Vector2 spawn;
    

    //Draw debug raycasts
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down)* groundCheckDist);
    }

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        myState = PlayerState.IDLE;
        LR = GetComponent<LineRenderer>();
        LR.startWidth = 0.5f;
        LR.endWidth = 0.5f;

        RB.gravityScale = baseGravity;

        spawn = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Read the Player Input
        ReadInput();

        //Update Player status based on inputs
        CheckContact();
        UpdateState();

        //Choose corresponding response to inputs based on current state
        switch (myState)
        {
            case PlayerState.IDLE:
                MovePlayer();
                Jump();
                ChangeDyn();
                break;
            case PlayerState.AERO:
                Attack();
                break;
            case PlayerState.DYN:
                Dyn();
                break;
            case PlayerState.BLOCK:
                Block();
                break;
            case PlayerState.CHARGE:
                MovePlayer();
                ChangeDyn();
                if (grounded == coyoteTime)
                    Jump();
                break;
        }

        //Update breath based on status
        UpdateBreath();

        speed = RB.velocity;
    }

    private void FixedUpdate()
    {
        //Choose corresponding response to inputs based on current state
        switch (myState)
        {
            case PlayerState.IDLE:
                break;
            case PlayerState.AERO:
                break;
            case PlayerState.DYN:
                Dyn_Fixed();
                break;
            case PlayerState.BLOCK:
                break;
        }

        //Move with platform
        Vector3 saveVel = RB.velocity;
        RB.transform.position += platMove;
        platMove = Vector3.zero;
    }

    //Update player movement (based on inputs)
    public void MovePlayer()
    {
        //Get current velocity
        Vector2 currV = RB.velocity;

        #region //SET HORIZONTAL SPEED
        //Get target velocity
        Vector2 targetV = new Vector2 (hInput * moveSpeed,0);

        //Calculate change in velocity needed
        Vector2 VChange = targetV - currV;

        //Clamp the maximum velcotiy change
        VChange.x = Mathf.Clamp(VChange.x, -moveAcc, moveAcc);
        VChange.y = 0;

        /*Apply new velocity if:
         * Current speed is less than max movement speed
         * Player is holding in opposite direction of current movement
         * Player isn't holding all the way forward (less than half)
         * Player isn't sliding
        */
        if(slide > 0)
        { }
        else if (Mathf.Abs(currV.x) <= moveSpeed || Mathf.Sign(hInput) != Mathf.Sign(currV.x) || Mathf.Abs(hInput) < 0.4f)
            RB.AddForce(VChange * RB.mass, ForceMode2D.Impulse);
        #endregion
    }
    public void Jump()
    {
        //Jump if not holding down
        if(jumpJustDown && vInput >= -0.3)
        {
            //Jump if grounded
            if (grounded > 0)
            {
                grounded = -0.3f;
                RB.velocity = new Vector2(RB.velocity.x, 0);
                RB.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
            }
            //Air jump if not grounded
            else if (grounded == 0 && breath >= jumpCost)
            {
                breath -= jumpCost;
                jumpCost++;

                grounded = -0.3f;
                RB.velocity = new Vector2(RB.velocity.x, 0);
                RB.AddForce(Vector2.up * jumpSpeed * 0.75f, ForceMode2D.Impulse);
            }
        }
        //Slide or Dive if grounded
        else if(jumpJustDown && vInput < -0.3)
        {
            //Slide if grounded
            if (grounded > 0 && breath >= 0.5f)
            {
                grounded = -0.2f;
                breath -= 0.5f;
                if(facingRight)
                    RB.AddForce(Vector2.right * jumpSpeed, ForceMode2D.Impulse);
                else
                    RB.AddForce(Vector2.left * jumpSpeed, ForceMode2D.Impulse);
                slide = 0.5f;
            }
            //Dive if not grounded
            else if (grounded == 0)
            {
                grounded = -0.5f;
                RB.velocity = new Vector2(RB.velocity.x/0.5f, 0);
                RB.AddForce(Vector2.down * jumpSpeed, ForceMode2D.Impulse);
            }
        }
    }

    //Use Player attacks / abilities
    public void Attack()
    {
        //Fire straight ahead if button was just pressed
        if(aeroJustDown)
        {
            if(breath >= 0.5f)
            {
                GameObject b = Instantiate(bullet, RB.position, Quaternion.identity);
                P_Bullet PB = b.GetComponent<P_Bullet>();
                /*
                if (facingRight)
                    b.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 5, ForceMode2D.Impulse);
                else
                    b.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 5, ForceMode2D.Impulse);
                */
                PB.BulletStart(elementState, (facingRight ? Vector2.right : Vector2.left));
                Destroy(b, 5.0f);
                breath -= 0.5f;
            }
        }
        //Let Player aim if aero button has been held for long enough (0.25 seconds)
        else if(aeroHold > 0.25)
        {
            Vector2 aimDir = new Vector2();
            //Calculate direction (based on Joystick for controller, or mouse position for keyboard)
            switch (mode)
            {
                case ControlMode.CONTROLLER:
                    aimDir = new Vector2(hInput, vInput);
                    break;
                case ControlMode.KEYBOARD:
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    aimDir = mousePos - RB.position;
                    break;
            }
            aimDir = (aimDir.normalized * 2.0f);

            //Draw a Line in the desired direction
            Debug.DrawRay(RB.position, aimDir, Color.blue);
            LR.enabled = true;
            LR.SetPosition(0, RB.position);
            LR.SetPosition(1, RB.position + aimDir);

            //If the button was just released, fire in current direction
            if (aeroJustUp)
            {
                if(breath >= 0.5f)
                {
                    GameObject b = Instantiate(bullet, RB.position, Quaternion.identity);
                    P_Bullet PB = b.GetComponent<P_Bullet>();
                    PB.BulletStart(elementState, aimDir.normalized);

                    Destroy(b, 5.0f);

                    breath -= 0.5f;
                }
                LR.enabled = false;
            }
        }
    }

    //Player can switch their dyn ability 
    public void ChangeDyn()
    {
        //If player presses one of the dyn buttons, can switch to new state
        if (dynNextDown)
        {
            int value = (int)elementState;
            value = (value + 1) % 4;
            elementState = (DynElement)value;
        }
        if (dynPrevDown)
        {
            int value = (int)elementState;
            value = (value - 1) % 4;
            if (value == -1)
                value = 3;
            elementState = (DynElement)value;
        }
    }

    //Dyn Abilities called in update (for instantaneuos / impulse motion)
    //Also calls MovePlayer if the player can move horizontally during motion
    public void Dyn()
    {
        //Select next action based on dyn mode
        switch(elementState)
        {
            //Allow player to move while rising
            case DynElement.AERO:
                MovePlayer();
                break;
            //Burst upwards when releasing dyn, and also allow player to move
            case DynElement.HYDRO:
                MovePlayer();
                if (dynJustUp)
                {
                    RB.velocity = new Vector2(RB.velocity.x, 0);
                    RB.AddForce(Vector2.up * jumpSpeed * 0.75f, ForceMode2D.Impulse);
                }
                break;
            //Hold in place, then launch the player in the desired direction
            case DynElement.LITHO:
                //Once button is pressed for first time, set time to 60 and lock gravity / position
                if (dynTime > 0)
                    dynTime -= Time.deltaTime;

                if(dynJustDown && dynTime == 0 && breath >= 2)
                {
                    dynTime = 1;
                    RB.velocity = Vector2.zero;
                    RB.constraints = RigidbodyConstraints2D.FreezeAll;
                    RB.gravityScale = 0;
                    breath -= 2;
                }
                //Once time reaches 30, unlock constraints and launch player
                else if(dynTime < 0.5f &&  dynTime > 0 && RB.constraints == RigidbodyConstraints2D.FreezeAll)
                {
                    RB.constraints = RigidbodyConstraints2D.FreezeRotation;
                    Vector2 aimDir = new Vector2(hInput, vInput);
                    aimDir = aimDir.normalized;
                    RB.AddForce(aimDir * 20, ForceMode2D.Impulse);
                }
                else if(dynTime <= 0)
                {
                    RB.gravityScale = baseGravity;
                    dynTime = 0;
                    LR.enabled = false;
                }

                if(dynTime > 0)
                {
                    Vector2 aimDir = new Vector2(hInput, vInput);
                    aimDir = aimDir.normalized;
                    //Draw a Line in the desired direction
                    Debug.DrawRay(RB.position, aimDir, Color.blue);
                    LR.enabled = true;
                    LR.SetPosition(0, RB.position);
                    LR.SetPosition(1, RB.position + aimDir);
                }
                break;
            case DynElement.PYRO:
                //While holding down, allow the player to aim and slow gravity
                Vector2 dir2 = new Vector2(hInput, vInput);
                if (dynInput > 0 && dynTime <= breath && !fireRecoil)
                {
                    dynTime += Time.deltaTime * 2;
                    dynTime = Mathf.Clamp(dynTime, 0, 5);
                    
                    RB.gravityScale = (RB.velocity.y > 0 ? 1:0.5f);

                    //Draw a Line in the desired direction
                    Vector2 dir3 = dir2.normalized * dynTime;
                    Debug.DrawRay(RB.position, dir3, Color.red);
                    LR.enabled = true;
                    LR.SetPosition(0, RB.position);
                    LR.SetPosition(1, RB.position + dir3);
                }
                //after releasing, launch the player in the opposite direction
                else if (dynJustUp && dynTime <= breath && dynTime > 1 && !fireRecoil)
                {
                    dir2 = dir2.normalized;
                    int fire = (int)dynTime;

                    //Calculate travel speed
                    Vector2 motion = RB.velocity;
                    float fireSpeed = motion.magnitude + fire * fireMult;

                    if (motion.magnitude >= fireMax)
                        fireSpeed = motion.magnitude;
                    else
                        fireSpeed = Mathf.Clamp(fireSpeed,(motion.magnitude < fire*fireMult ? fire * fireMult :motion.magnitude),fireMax);

                    //Set new motion
                    RB.velocity = new Vector2(-dir2.x, -dir2.y) * fireSpeed;
                    //RB.AddForce(-dir2 * fire * fireMult, ForceMode2D.Impulse);

                    breath -= fire;
                    //(Also spawn a fireball later
                    RB.gravityScale = baseGravity;
                    dynTime = 0;
                    LR.enabled = false;
                    fireRecoil = true;
                    Invoke("SetIdle", 0.5f);
                }
                //Emergency cancel - reset states
                else
                {
                    RB.gravityScale = baseGravity;
                    dynTime = 0;
                    LR.enabled = false;
                }
                break;
        }
    }

    //Dyn abilities called in fixed update (for non-impulse motion)
    public void Dyn_Fixed()
    {
        //Select next action based on dyn mode
        switch (elementState)
        {
            //If in air mode, accelerate up (capped by max rise speed)
            case DynElement.AERO:
                if(breath > 0)
                {
                    if(RB.velocity.y < VSpeedMax)
                        RB.AddForce(Vector2.up * TornadoDynForce, ForceMode2D.Force);
                    breath -= Time.deltaTime / 1.2f;
                }
                break;
            //If in water mode, accelerate up (capped by max fall speed)
            case DynElement.HYDRO:
                if (breath > 0)
                {
                    if (RB.velocity.y < -VSpeedMax/10)
                        RB.AddForce(Vector2.up * TornadoDynForce, ForceMode2D.Force);
                    breath -= Time.deltaTime / 1.2f;
                }
                break;
            case DynElement.LITHO:
                break;
            case DynElement.PYRO:
                break;
        }
    }
    public void Block()
    {
        //If player is holding block, summon the shield in front of the player
        if(blockInput > 0)
        {
            if(currentShield == null)
                currentShield = Instantiate(shield, RB.position, Quaternion.identity);
            if (facingRight)
                currentShield.transform.position = RB.position + (Vector2.right);
            else
                currentShield.transform.position = RB.position + (Vector2.left);
        }
        //Drop shield when releasing button (and counter if held long enough
        if(blockJustUp && currentShield != null)
        {
            Destroy(currentShield);
            currentShield = null;
            Vector2 counterPos = RB.position + (facingRight ? Vector2.right : Vector2.left)*1.5f;
            if (blockHold > 1)
                Destroy(Instantiate(counter, counterPos, Quaternion.identity),0.5f);
        }
        
    }
    

    //Update player inputs and states / resources
    private void ReadInput()
    {
        bool ZeroJ = (jumpInput == 0);
        bool ZeroA = (aeroInput == 0);
        bool ZeroD = (dynInput == 0);
        bool ZeroBr = (breathInput == 0);
        bool ZeroBl = (blockInput == 0);
        bool ZeroDN = (dynNextInput == 0);
        bool ZeroDP = (dynPrevInput == 0);

        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        jumpInput = Input.GetAxis("Jump");
        aeroInput = Input.GetAxis("Aero");
        dynInput = Input.GetAxis("Dyn");
        breathInput = Input.GetAxis("Breath");
        blockInput = Input.GetAxis("Block");
        dynNextInput = Input.GetAxis("DynNext");
        dynPrevInput = Input.GetAxis("DynPrev");

        #region //Indicate if specific buttons (Jump, Aero, Breath) were just pressed down or up
        jumpJustDown = (jumpInput > 0 && ZeroJ);

        aeroJustDown = (aeroInput > 0 && ZeroA);
        aeroJustUp = (aeroInput == 0 && !ZeroA);
        if (aeroInput > 0 || aeroJustUp)
            aeroHold += Time.deltaTime;
        else
            aeroHold = 0;
        Mathf.Clamp(aeroHold, 0, 5.0f);

        dynJustDown = (dynInput > 0 && ZeroD);
        dynJustUp = (dynInput == 0 && !ZeroD);
        if (dynInput > 0 || dynJustUp)
            dynHold += Time.deltaTime;
        else
            dynHold = 0;
        Mathf.Clamp(dynHold, 0, 5.0f);

        breathJustDown = (breathInput > 0 && ZeroBr);
        breathJustUp = (breathInput == 0 && !ZeroBr);
        if (breathInput > 0 || breathJustUp)
            breathHold += Time.deltaTime;
        else
            breathHold = 0;
        Mathf.Clamp(breathHold, 0, 5.0f);

        blockJustUp = (blockInput == 0 && !ZeroBl);
        if (blockInput > 0 || blockJustUp)
            blockHold += Time.deltaTime;
        else
            blockHold = 0;
        Mathf.Clamp(blockHold, 0, 5.0f);

        dynNextDown = (dynNextInput > 0 && ZeroDN);
        dynPrevDown = (dynPrevInput > 0 && ZeroDP);
        #endregion

        //Reduce slide state (set in Jump) if player is sliding
        if (slide > 0)
        {
            slide -= Time.deltaTime;
            slide = Mathf.Clamp(slide, 0, 1);
        }

        //Change which way the player is facing
        if ((facingRight && hInput < 0)|| (!facingRight && hInput > 0))
            facingRight = !facingRight;
    }
    public void CheckContact()
    {
        //If a raycast touches the ground, set grounded to max value
        Vector2 position = RB.position;
        RaycastHit2D hitL = Physics2D.Raycast(position, Vector2.down, groundCheckDist, GroundLayer);

        if (grounded < 0)
        {
            grounded += Time.deltaTime;
            grounded = Mathf.Clamp(grounded, -1, 0);
        }
        else if (hitL.collider != null)
            grounded = coyoteTime;
        //Else, decrement grounded value
        else
        {
            grounded -= Time.deltaTime;
            grounded = Mathf.Clamp(grounded, 0, coyoteTime);
        }
    }
    public void UpdateState()
    {
        switch (myState)
        {
            case PlayerState.IDLE:
                if (aeroInput > 0)
                    myState = PlayerState.AERO;
                else if (dynInput > 0 && breath > 0)
                    myState = PlayerState.DYN;
                else if (blockInput > 0)
                    myState = PlayerState.BLOCK;
                else if (breathInput > 0)
                    myState = PlayerState.CHARGE;
                break;
            case PlayerState.AERO:
                if (aeroInput == 0 && !aeroJustUp)
                    myState = PlayerState.IDLE;
                break;
            case PlayerState.DYN:
                if (elementState == DynElement.LITHO && dynTime <= 0)
                {
                    dynTime = 0;
                    myState = PlayerState.IDLE;
                }
                else if (elementState == DynElement.PYRO && !fireRecoil && dynInput == 0 && !dynJustUp)
                {
                    dynTime = 0;
                    myState = PlayerState.IDLE;
                }
                else if (elementState != DynElement.LITHO && elementState != DynElement.PYRO && dynInput == 0 && !dynJustUp)
                    myState = PlayerState.IDLE;
                break;
            case PlayerState.BLOCK:
                if (blockJustUp)
                    Invoke("SetIdle", 0.5f);
                break;
            case PlayerState.CHARGE:
                if (breathInput == 0)
                    myState = PlayerState.IDLE;
                else if (aeroInput > 0)
                    myState = PlayerState.AERO;
                else if (dynInput > 0 && breath > 0)
                    myState = PlayerState.DYN;
                else if (blockInput > 0)
                    myState = PlayerState.BLOCK;
                    break;
        }
    }

    public void SetIdle()
    {
        myState = PlayerState.IDLE;
        fireRecoil = false;
    }

    //Basic respawn system
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Hazard"))
        {
            breath = maxBreath;
            RB.velocity = Vector2.zero;
            RB.transform.position = spawn;
        }
    }

    public void UpdateBreath()
    {
        //If on ground, allow player to update power
        if (grounded == coyoteTime)
        {
            //Gain breath through charge
            if (breathInput > 0)
                breath += (Time.deltaTime * 2.0f);
            else
                breath += (Time.deltaTime);

            if (jumpCost != 1)
                jumpCost = 1;
        }
        //If not grounded, can charge slowly in the air
        else if (breathInput > 0)
            breath += (Time.deltaTime * 0.75f);

        breath = Mathf.Clamp(breath, 0, maxBreath);
    }

    public void GainBreath(float gains)
    {
        breath = Mathf.Clamp(breath += gains, 0, maxBreath);
    }
}
