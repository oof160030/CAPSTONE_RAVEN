using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj_bubble : MonoBehaviour
{
    public float bubbleForce = 10;
    public float bubbleBreath = 2;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            P_Movement PM = collision.gameObject.GetComponent<P_Movement>();
            Rigidbody2D PRB = collision.gameObject.GetComponent<Rigidbody2D>();

            //Propel the object upwards
            PRB.velocity = new Vector2(PRB.velocity.x, 0);
            PRB.AddForce(Vector2.up * bubbleForce, ForceMode2D.Impulse);

            //Restore breath
            PM.GainBreath(bubbleBreath);

            Pop();
        }
    }

    public void Pop()
    {
        //Play animation

        //Remove hitbox
        gameObject.GetComponent<CircleCollider2D>().enabled = false;

        //Destroy the bubble (after the 1/2 second animation)
        Destroy(gameObject, 0.5f);
    }
}
