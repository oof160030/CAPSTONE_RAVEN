using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Obj_turbine : MonoBehaviour
{
    [SerializeField]
    private UnityEvent TurbineActivated = new UnityEvent();
    [SerializeField]
    private UnityEvent TurbineDeactivated = new UnityEvent();

    public float acc; //Current rate of spin change
    public float accRate; //Change in acceleration per second
    public float maxAcc; //Maximum change in spin per second
    public float currentSpin; //Current spin
    public float maxSpin; //Max spin value

    bool objActivated = false;

    private void FixedUpdate()
    {
        //Update fan acceleration
        acc = Mathf.Clamp(acc -= accRate * Time.deltaTime, -maxAcc, maxAcc);

        //Update fan spin based on acceleration
        currentSpin = Mathf.Clamp(currentSpin += acc * Time.deltaTime,0,maxSpin);

        //Activate fan when hits 0 or max
        if (objActivated && currentSpin == 0)
        {
            objActivated = false;
            TurbineDeactivated.Invoke();
        }
        else if (!objActivated && currentSpin == maxSpin)
        {
            objActivated = true;
            TurbineActivated.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("P_Projectile"))
        {
            //Destroy the projectile
            //Play the activate effect (later)
            activeEffect();

            //Set the fan rate to the max possible
            acc = maxAcc;
        }
    }

    private void activeEffect()
    {
        SpriteRenderer SR = GetComponent<SpriteRenderer>();
        SR.color = Color.red;
    }
}
