using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Obj_chainHook : MonoBehaviour
{
    //External access to the bubble prefab
    public GameObject bubblePrefab;
    //In_game reference to the current bubble (if still in existence)
    private GameObject myBubble = null;

    //
    private Rigidbody2D RB;

    //Vector positions (start and end)
    private Vector2 start;
    public Vector2 end;
    private Vector2 prior;

    //Position float
    public float interpolant;
    bool objActive;

    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [SerializeField]
    private UnityEvent HookRetracted = new UnityEvent();
    [SerializeField]
    private UnityEvent HookExtended = new UnityEvent();
    [SerializeField]
    private FloatEvent UpdateHookPosition = new FloatEvent();


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, end);
    }

    // Start is called before the first frame update
    void Start()
    {
        start = transform.position;
        end = new Vector2(start.x, end.y);
        RB = gameObject.GetComponent<Rigidbody2D>();
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
            myBubble = Instantiate(bubblePrefab, gameObject.transform);
            //Start gravity
            RB.gravityScale = -0.7f;
        }
        //When hit by other projectile, destroy bubble
        else if (PB && PB.bulletType != DynElement.HYDRO && myBubble != null)
        {
            myBubble.GetComponent<Obj_bubble>().Pop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Calculate current interpolant
        updateInterpolant();

        //If hook position changed, send ping through Unity Event
        if (RB.position != prior)
        {
            //Broadcast new float through unity engine
            UpdateHookPosition.Invoke(Mathf.Clamp(interpolant,0,1));

            //Clamp position based on position
            if(RB.position.y <= start.y || RB.position.y >= end.y)
            {
                RB.position = new Vector2(RB.position.x, RB.position.y <= start.y ? start.y : end.y);
                //Zero out momentum and gravity
                RB.velocity = Vector2.zero;
                RB.gravityScale = 0;
            }
            
        }

        //If hook position is maxed out, send ping through max event
        if(!objActive && interpolant >= 1)
        {
            objActive = true;
            HookExtended.Invoke();
        }

        //
        else if(objActive && interpolant <= 0)
        {
            objActive = false;
            HookRetracted.Invoke();
        }

        //If no bubble and gravity not down, set gravity to down
        if(myBubble == null)
        {
            if (RB.position.y != start.y)
                RB.gravityScale = 1;
            else
                RB.gravityScale = 0;
        }

        prior = RB.position;
    }

    private void updateInterpolant()
    {
        //calculate position float value
        Vector2 AB = end - start;
        Vector2 AV = RB.position - start;
        interpolant = Vector2.Dot(AV, AB) / Vector2.Dot(AB, AB);
    }
}
