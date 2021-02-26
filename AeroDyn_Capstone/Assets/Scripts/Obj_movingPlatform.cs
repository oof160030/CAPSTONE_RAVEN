using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obj_movingPlatform : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject shell; //The parent object the platform and nodes are inside of
    public List<GameObject> nodes;
    private Rigidbody2D RB;

    public bool objActive = false;
    public float timeToMove;
    private float currentTime;
    private bool incrementing = true;
    private int targetNode = 0;
    private Vector2 targetPos, priorPos;

    private void OnDrawGizmos()
    {
        //Add node objects if a list element is empty
        for(int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] == null || (i>0 && nodes[i] == nodes[i-1]))
            {
                nodes[i] = new GameObject();
                nodes[i].transform.parent = shell.transform;
            }
                
        }

        //Remove node objects if they are not in the list
        foreach(Transform child in shell.transform)
        {
            if (!nodes.Contains(child.gameObject) && gameObject != child.gameObject)
                DestroyImmediate(child.gameObject);
        }
    }

    void Start()
    {
        RB = gameObject.GetComponent<Rigidbody2D>();
        RB.position = nodes[0].transform.position;
        targetPos = nodes[1].transform.position;
        priorPos = nodes[0].transform.position;
        targetNode = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentTime >= timeToMove)
        {
            currentTime = 0;
            //Change target position
            if (incrementing)
                targetNode++;
            else
                targetNode--;
            //Bound target position
            if(targetNode >= nodes.Count)
            {
                incrementing = false;
                targetNode = nodes.Count -2;
            }
            else if (targetNode < 0)
            {
                incrementing = true;
                targetNode = 1;
            }

            //Set the current and target positions
            targetPos = nodes[targetNode].transform.position;
            priorPos = nodes[targetNode + (incrementing? -1:1)].transform.position;
        }
    }

    private void FixedUpdate()
    {
        //Move platform if active and greater than 1
        if (objActive && nodes.Count > 1)
        {
            currentTime += Time.deltaTime;
            RB.MovePosition(Vector2.Lerp(priorPos, targetPos, (currentTime / timeToMove)));
        }
    }

    public void activateObject()
    {
        objActive = true;
    }
}
