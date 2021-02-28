using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj_tornadoFlower : MonoBehaviour
{
    //If player is spinning, boost player speed
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            P_Movement PM = collision.gameObject.GetComponent<P_Movement>();
            Rigidbody2D PRB = collision.gameObject.GetComponent<Rigidbody2D>();

            if (PM.elementState == DynElement.AERO && PM.myState == PlayerState.DYN)
            {
                //Play the activate effect (later)

                //Boost player speed
                PRB.velocity = PRB.velocity.normalized * PM.VSpeedMax * 1.5f;
            }
        }
    }
}
