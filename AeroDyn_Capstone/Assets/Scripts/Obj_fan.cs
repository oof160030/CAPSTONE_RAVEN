using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj_fan : MonoBehaviour
{
    //Contact with wind vane
    public GameObject wind;
    //If fan is active
    [SerializeField]
    private bool objActive = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!objActive)
            wind.SetActive(false);
    }

    public void activateObject()
    {
        objActive = true;
        wind.SetActive(true);
    }

    public void deactivateObject()
    {
        objActive = false;
        wind.SetActive(false);
    }
}
