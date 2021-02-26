using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class obj_crystal : MonoBehaviour
{
    /*
    [Serializable]
    public class StringEvent : UnityEvent <string> { }
    */

    [SerializeField]
    private UnityEvent CrystalActivated = new UnityEvent();

    bool objActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("P_Projectile"))
        {
            //Destroy the projectile
            //Play the activate effect (later)
            activeEffect();
            if(!objActivated)
            {
                //Set active state
                objActivated = true;
                //Ping connected objects with unity event
                CrystalActivated.Invoke();
            }
        }
    }

    private void activeEffect()
    {
        SpriteRenderer SR = GetComponent<SpriteRenderer>();
        SR.color = Color.red;
    }
}
