using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj_bubbleFlower : MonoBehaviour
{
    //External access to the bubble prefab
    public GameObject bubblePrefab;

    //In_game reference to the current bubble (if still in existence)
    private GameObject myBubble = null;

    //Time to generate new bubble
    public float maxRegenTime;

    //Time spent regenerating
    private float regenTime = 0;

    // Update is called once per frame
    void Update()
    {
        if(myBubble == null && regenTime < maxRegenTime)
        {
            //Add to regen time
            regenTime += Time.deltaTime;

            if (regenTime >= maxRegenTime)
            {
                myBubble = Instantiate(bubblePrefab, transform.position + transform.up, Quaternion.identity);
                regenTime = 0;
            }  
        }
    }
}
