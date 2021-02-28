using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Obj_turbine : MonoBehaviour
{
    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [SerializeField]
    private UnityEvent TurbineActivated = new UnityEvent();
    [SerializeField]
    private UnityEvent TurbineDeactivated = new UnityEvent();
    [SerializeField]
    private FloatEvent UpdateTurbinePosition = new FloatEvent();

    public float acc; //Current rate of spin change
    public float accRate; //Change in acceleration per second
    public float maxAcc; //Maximum change in spin per second
    public float currentSpin; //Current spin
    private float priorSpin;
    public float maxSpin; //Max spin value

    private SpriteRenderer SR;

    bool objActivated = false;

    private void Start()
    {
        SR = gameObject.GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        //Update fan acceleration
        acc = Mathf.Clamp(acc -= accRate * Time.deltaTime, -maxAcc, maxAcc);
        if (acc > 0)
            activeEffect();
        else
            deactiveEffect();

        //Update fan spin based on acceleration
        currentSpin = Mathf.Clamp(currentSpin += acc * Time.deltaTime,0,maxSpin);
        if(currentSpin != priorSpin)
        {
            UpdateTurbinePosition.Invoke(currentSpin / maxSpin);
        }

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

        priorSpin = currentSpin;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("P_Projectile"))
        {
            //Destroy the projectile
            Destroy(collision.gameObject);

            //Play the activate effect (later)
            activeEffect();

            //Set the fan rate to the max possible
            acc = maxAcc;
        }
    }

    private void activeEffect()
    {
        SR.color = Color.red;
    }

    private void deactiveEffect()
    {
        SR.color = Color.cyan;
    }
}
