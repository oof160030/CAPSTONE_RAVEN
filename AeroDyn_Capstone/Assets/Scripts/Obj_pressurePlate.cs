using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Obj_pressurePlate : MonoBehaviour
{
    [SerializeField]
    private UnityEvent PlateActivated = new UnityEvent();
    [SerializeField]
    private UnityEvent PlateDeactivated = new UnityEvent();
    private BoxCollider2D trigger;
    [Tooltip("Object must be paired with an URN object in order to detect said urn")]
    public Collider2D urn;
    public LayerMask player;

    bool objActivated = false;

    private void Start()
    {
        trigger = gameObject.GetComponent<BoxCollider2D>();
    }

    //If the plate is touching an urn or the player, use activation event
    private void Update()
    {
        //Activate if inactive and touching something
        if(!objActivated && (trigger.IsTouchingLayers(player) || (urn != null && trigger.IsTouching(urn))))
        {
            objActivated = true;
            PlateActivated.Invoke();
        }
        else if (objActivated && !(trigger.IsTouchingLayers(player) || (urn != null && trigger.IsTouching(urn))))
        {
            objActivated = false;
            PlateDeactivated.Invoke();
        }
    }
}
