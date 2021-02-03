using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This Script handles player movement and reads controller inputs.
public class P_Movement : MonoBehaviour
{
    //Button management
    public bool jumpJustDown;
    public float jumpInput = 0;


    //External Object Components
    private Rigidbody2D RB;
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
    }

    // Update is called once per frame
    void Update()
    {
        //Read the Player Input
        ReadInput();
        //Update player values
        CheckContact();

        //Update player movement (based on inputs)
        MovePlayer();
        Jump();
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

    private void FixedUpdate()
    {
        //RB.MovePosition(RB.position + new Vector2(hInput,0) * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        RB.velocity = new Vector2(moveSpeed*2, RB.velocity.y);
    }

    //Script Reads Player Inputs
    private void ReadInput()
    {
        bool ZeroV = (jumpInput == 0);
        
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        jumpInput = Input.GetAxis("Jump");

        //If jump wasn't down prior, set it now
        jumpJustDown = (jumpInput > 0 && ZeroV);

        if (slide > 0)
        {
            slide -= Time.deltaTime;
            slide = Mathf.Clamp(slide, 0, 1);
        }

        if ((facingRight && hInput < 0)|| (!facingRight && hInput > 0))
            facingRight = !facingRight;
    }

    private void setButtonLast()
    {
        //If the button is pressed and just down is not true, set to true

        //Set back to false after time delay
    }
}
