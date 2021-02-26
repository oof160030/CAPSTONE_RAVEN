using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Obj_windGate : MonoBehaviour
{
    [SerializeField]
    private UnityEvent GateActivated = new UnityEvent();

    bool objActivated = false;
    public float activateSpeed;

    //Activate if player is fast enough
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D PRB = collision.gameObject.GetComponent<Rigidbody2D>();

            if(Mathf.Abs(PRB.velocity.y) >= activateSpeed && !objActivated)
            {
                //Play the activate effect (later)
                activeEffect();

                //Set active state
                objActivated = true;
                //Ping connected objects with unity event
                GateActivated.Invoke();
            }
        }
    }

    private void activeEffect()
    {
        SpriteRenderer SR = GetComponent<SpriteRenderer>();
        SR.color = Color.yellow;
    }
}
