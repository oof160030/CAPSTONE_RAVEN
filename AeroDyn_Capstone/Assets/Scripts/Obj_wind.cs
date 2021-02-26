using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj_wind : MonoBehaviour
{
    //Force to push player by
    public float windForce;
    private LayerMask player;
    BoxCollider2D BC;

    Rigidbody2D PRB;

    // Start is called before the first frame update
    void Start()
    {
        player = LayerMask.GetMask("Player");
        BC = gameObject.GetComponent<BoxCollider2D>();
        PRB = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if(BC.IsTouchingLayers(player))
        {
            Vector2 dir = transform.up;
            PRB.AddForce(dir * windForce);
        }
    }

}
