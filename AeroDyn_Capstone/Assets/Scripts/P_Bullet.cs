using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Bullet : MonoBehaviour
{
    //Track the bullet's type
    public DynElement bulletType;

    //Access to the bullet rigidbody
    private Rigidbody2D RB;

    //Bullet properties
    [SerializeField]
    private float fastSpeed, mediumSpeed, slowSpeed;

    // Start is called before the first frame update
    void Start()
    {
        if (!RB)
            RB = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate in direction of current movement
        RB.SetRotation(Mathf.Rad2Deg * Mathf.Atan2(RB.velocity.y,RB.velocity.x));
    }

    public void BulletStart(DynElement DE, Vector2 direction)
    {
        //Set the bullet's type and get it's rigidbody (if empty)
        bulletType = DE;
        if (!RB)
            RB = gameObject.GetComponent<Rigidbody2D>();
        SpriteRenderer SR = gameObject.GetComponent<SpriteRenderer>();

        //Based on type, change the bullet's speed and gravity
        switch (DE)
        {
            case DynElement.AERO:
                //Move at high speed, no gravity
                RB.AddForce(direction * fastSpeed, ForceMode2D.Impulse);
                SR.color = Color.green;
                break;
            case DynElement.HYDRO:
                //Move at moderate speed, upwards gravity, slight drag
                RB.AddForce(direction * mediumSpeed, ForceMode2D.Impulse);
                RB.gravityScale = -0.6f;
                RB.drag = 0.3f;
                SR.color = Color.blue;
                break;
            case DynElement.LITHO:
                //Move at moderate speed, downwards gravity
                RB.AddForce(direction * fastSpeed, ForceMode2D.Impulse);
                RB.gravityScale = 1;
                SR.color = Color.yellow;
                break;
            case DynElement.PYRO:
                //Move at low speed, no gravity
                RB.AddForce(direction * slowSpeed, ForceMode2D.Impulse);
                SR.color = Color.red;
                break;
        }
    }
}
