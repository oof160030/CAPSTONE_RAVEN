using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Control : MonoBehaviour
{
    //Object transform for the camera to follow
    public GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = target.transform.position;
        transform.position = new Vector3(pos.x,pos.y,-10);
    }

    public void changeTarget(GameObject newTarg)
    {
        target = newTarg;
    }

    public void test()
    {
        Debug.Log("Activated");
    }
}
