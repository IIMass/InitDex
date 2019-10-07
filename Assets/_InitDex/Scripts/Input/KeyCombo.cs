using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCombo : MonoBehaviour
{
    public string[] registeredButtons;
    private int currentArrayIndex;

    public float comboEndTime;
    private float lastComboPressTime;

    public KeyCombo(string[] buttons)
    {
        registeredButtons = buttons;
    }

    bool ComboCheck()
    {
        if (Time.time > lastComboPressTime + comboEndTime)
        {
            currentArrayIndex = 0;
            return true;
        }
        else
            return false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            lastComboPressTime = Time.time;
    }
}
