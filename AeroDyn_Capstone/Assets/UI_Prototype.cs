using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Prototype : MonoBehaviour
{
    //External access to the TextMeshPro
    public TextMeshProUGUI BreathText;
    private P_Movement player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<P_Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        //Set text color based on element
        Color textCol;
        switch (player.elementState)
        {
            case DynElement.AERO:
                textCol = Color.green;
                break;
            case DynElement.HYDRO:
                textCol = Color.cyan;
                break;
            case DynElement.LITHO:
                textCol = Color.yellow;
                break;
            default:
                textCol = Color.red;
                break;
        }
        BreathText.color = textCol;

        //read the player's breath
        float BR = player.breath;
        BreathText.text = "BREATH: " + BR.ToString("0.00") + " / " + player.maxBreath.ToString("0");

        
    }
}
