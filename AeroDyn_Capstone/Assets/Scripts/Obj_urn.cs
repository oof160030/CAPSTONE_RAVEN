using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj_urn : MonoBehaviour
{
    //External access to the bubble prefab
    public GameObject bubblePrefab;
    //In_game reference to the current bubble (if still in existence)
    private GameObject myBubble = null;

    private Rigidbody2D RB;

    private Vector2 start;
    [SerializeField]
    private bool startWithBubble = false;
    public float bubbleHeight;
    [SerializeField]
    private float bubbleSpeed;

    // Start is called before the first frame update
    void Start()
    {
        RB = gameObject.GetComponent<Rigidbody2D>();
        start = transform.position;

        if (startWithBubble)
            ReceiveBubble();
    }

    // Update is called once per frame
    void Update()
    {
        //Reset gravity if bubble is ever lost
        if (myBubble == null & RB.gravityScale != 1)
            RB.gravityScale = 1;
        //Move urn upwards if bubble is present
        else if (myBubble != null && RB.position.y < start.y+bubbleHeight)
        {
            RB.velocity = Vector2.up * bubbleSpeed;
        }
        //Stop bubble once max height is reached
        else if(myBubble != null && RB.velocity.magnitude != 0)
        {
            RB.position = start + Vector2.up * bubbleHeight;
            RB.velocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        P_Bullet PB = collision.gameObject.GetComponent<P_Bullet>();

        //When hit by blueprojectile, spawn bubble
        if (PB && PB.bulletType == DynElement.HYDRO && myBubble == null)
        {
            //Destroy the bullet
            Destroy(PB.gameObject);
            //Create a bubble as a child object
            ReceiveBubble();
        }
        //When hit by other projectile, destroy bubble
        else if (PB && PB.bulletType != DynElement.HYDRO && myBubble != null)
        {
            myBubble.GetComponent<Obj_bubble>().Pop();
        }
    }

    public void ReceiveBubble()
    {
        myBubble = Instantiate(bubblePrefab, gameObject.transform);
        RB.gravityScale = 0;
    }
}
