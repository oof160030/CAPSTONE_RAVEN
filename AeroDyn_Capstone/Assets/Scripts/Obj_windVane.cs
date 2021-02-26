using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Obj_windVane : MonoBehaviour
{
    [SerializeField]
    private UnityEvent VaneActivated = new UnityEvent();

    bool objActivated = false;

    //Activate if player is fast enough
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            P_Movement PRB = collision.gameObject.GetComponent<P_Movement>();

            if ((PRB.elementState == DynElement.AERO && PRB.myState == PlayerState.DYN) && !objActivated)
            {
                //Play the activate effect (later)
                activeEffect();

                //Set active state
                objActivated = true;
                //Ping connected objects with unity event
                VaneActivated.Invoke();
            }
        }
    }

    private void activeEffect()
    {
        SpriteRenderer SR = GetComponent<SpriteRenderer>();
        SR.color = Color.yellow;
    }
}
