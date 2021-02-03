using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { IDLE, AERO, DYN, HIT };
public enum ControlMode { KEYBOARD, CONTROLLER};
//This Script handles player movement and reads controller inputs.
public class P_Movement : MonoBehaviour
{
    //Button management
    public ControlMode mode;
    public bool jumpJustDown;
    public float jumpInput = 0;

    public bool aeroJustDown, aeroJustUp;
    public float aeroInput = 0;
    public float aeroHold = 0;

    //State Mechanics
    public PlayerState myState = PlayerState.IDLE;

    //External Object Components
    private Rigidbody2D RB;
    public GameObject bullet;
    private LineRenderer LR;

    public float moveSpeed;
    public float moveAcc;
    public float hInput;
    public float vInput;
    public float jumpSpeed;
    public bool facingRight = true;

    //Ground / contact check
    public float groundCheckDist = 1.0f;
    //
    public float grounded = 0.5f;
    public float coyoteTime = 0.5f;
    public LayerMask GroundLayer;
    public float slide = 0;

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
    }

    // Update is called once per frame
    void Update()
    {
        //Read the Player Input
        ReadInput();
        //Update player values
        CheckContact();
        UpdateState();

        //Update player movement (based on inputs) if current state is idle
        if (myState == PlayerState.IDLE)
        {
            MovePlayer();
            Jump();
        }
        //If player is in attack state, use attack methods
        else if (myState == PlayerState.AERO)
            Attack();
    }

    //Moves player relative to time passed
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
            else if (grounded == 0)
            {
                grounded = -0.3f;
                RB.velocity = new Vector2(RB.velocity.x, 0);
                RB.AddForce(Vector2.up * jumpSpeed * 0.75f, ForceMode2D.Impulse);
            }
        }
        //Slide or Dive if grounded
        else if(jumpJustDown && vInput < -0.3)
        {
            //Slide if grounded
            if (grounded > 0)
            {
                grounded = -0.5f;
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

    //Allow player to use attacks
    public void Attack()
    {
        //Fire straight ahead if button was just pressed
        if(aeroJustDown)
        {
            GameObject b = Instantiate(bullet, RB.position, Quaternion.identity);
            if (facingRight)
                b.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 5, ForceMode2D.Impulse);
            else
                b.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 5, ForceMode2D.Impulse);
            Destroy(b, 5.0f);
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
                GameObject b = Instantiate(bullet, RB.position, Quaternion.identity);
                b.GetComponent<Rigidbody2D>().AddForce(aimDir * 5, ForceMode2D.Impulse);
                Destroy(b, 5.0f);
                LR.enabled = false;
            }
        }
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
        //If player is not in aero / dyn state, and the player is holding aero, set state
        if (myState == PlayerState.IDLE && (aeroJustDown || aeroInput > 0))
            myState = PlayerState.AERO;
        //If plyer is not idling, and also not pressing any attack buttons
        else if (myState != PlayerState.IDLE && aeroInput == 0 && !aeroJustUp)
            myState = PlayerState.IDLE;
    }

    //Script Reads Player Inputs
    private void ReadInput()
    {
        bool ZeroJ = (jumpInput == 0);
        bool ZeroA = (aeroInput == 0);
        
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        jumpInput = Input.GetAxis("Jump");
        aeroInput = Input.GetAxis("Aero");

        //Indicate if specific buttons were just pressed down or up
        jumpJustDown = (jumpInput > 0 && ZeroJ);

        aeroJustDown = (aeroInput > 0 && ZeroA);
        aeroJustUp = (aeroInput == 0 && !ZeroA);
        if (aeroInput > 0 || aeroJustUp)
        {
            aeroHold += Time.deltaTime;
        }
        else
            aeroHold = 0;
        Mathf.Clamp(aeroHold, 0, 5.0f);

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

    private void setButtonLast()
    {
        //If the button is pressed and just down is not true, set to true

        //Set back to false after time delay
    }
}
