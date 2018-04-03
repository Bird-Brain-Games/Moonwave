﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using System;
using UnityEngine;

public enum BUTTONS
{
    BLANK = 0x000,
    DPAD_UP = 0x0001,
    DPAD_DOWN = 0x0002,
    DPAD_LEFT = 0x0004,
    DPAD_RIGHT = 0x0008,
    START = 0x0010,
    BACK = 0x0020,
    LEFT_THUMB = 0x0040,
    RIGHT_THUMB = 0x0080,
    LEFT_SHOULDER = 0x0100,
    RIGHT_SHOULDER = 0x0200,
    A = 0x1000,
    B = 0x2000,
    X = 0x4000,
    Y = 0x8000
}

public enum TRIGGERS
{
    RIGHT,
    LEFT
}

public enum BUTTON_DETECTION
{
    GET_BUTTON,
    GET_BUTTON_DOWN,
    GET_BUTTON_UP
}

public enum ANALOGSTICKS
{
    RIGHT_X,
    RIGHT_Y,
    LEFT_X,
    LEFT_Y
}

[System.Serializable]
public struct Button
{
    public BUTTONS key;
    public BUTTONS altKey;

}

[System.Serializable]
public struct Trigger
{
    //The sensitivity we need to be above to return true
    [Range(0.0f, 1.0f)]
    public float sensitivity;
    public TRIGGERS trigger;
}

[System.Serializable]
public struct Trigger_Button
{
    //The sensitivity we need to be above to return true
    public Trigger trigger;
    public bool currentState;
    public bool lastState;
    public Button button;
}

[System.Serializable]
public struct Aim
{
    public ANALOGSTICKS aimX;
    public ANALOGSTICKS aimY;
}

[System.Serializable]
public struct Move
{
    public ANALOGSTICKS moveX;
    public ANALOGSTICKS moveY;
}

public class Controls : MonoBehaviour
{
    //dont modify these two regions unless you know what you are doing
    //The plugin imports
    #region Imports
    [DllImport("Controller_Input")]
    static extern bool CreateController(int cID);
    [DllImport("Controller_Input")]
    static extern void DeleteController(int cID);
    [DllImport("Controller_Input")]
    //This must be called at the start of each update function
    static extern bool ControllerUpdate(int cID);
    [DllImport("Controller_Input")]
    //returns true if the buttons is pressed
    static extern bool GetButton(int cID, int button);
    [DllImport("Controller_Input")]
    //returns true the frame the button is released
    static extern bool GetButtonUp(int cID, int button);
    [DllImport("Controller_Input")]
    //returns true the frame the button is pressed
    static extern bool GetButtonDown(int cID, int button);
    [DllImport("Controller_Input")]
    //returns the right thumbsticks x axis
    static extern float GetRightX(int cID);
    [DllImport("Controller_Input")]
    //returns the right thumbsticks y axis
    static extern float GetRightY(int cID);
    [DllImport("Controller_Input")]
    //returns the left thumbsticks x axis
    static extern float GetLeftX(int cID);
    [DllImport("Controller_Input")]
    //returns the left thumbsticks y axis
    static extern float GetLeftY(int cID);
    [DllImport("Controller_Input")]
    static extern float GetRightTrigger(int cID);
    [DllImport("Controller_Input")]
    static extern float GetLeftTrigger(int cID);
    [DllImport("Controller_Input")]
    //sets the controllers rumble 0 is off, 1 is full Rumble
    static extern void SetRumble(int cID, float leftRumble, float rightRumble);

    #endregion
    //The unity wrappers for the plugin imports
    #region Import Wrapper

    //returns whether the button is pressed
    bool GetButton(BUTTONS button)
    {
        return GetButton(playerNumber, (int)button);
    }
    //returns true when the button is released
    bool GetButtonUp(BUTTONS button)
    {
        return GetButtonUp(playerNumber, (int)button);
    }
    //returns true when the button is first pressed
    bool GetButtonDown(BUTTONS button)
    {
        return GetButtonDown(playerNumber, (int)button);
    }
    //returns a value between -1 to 1 for the given analogue imput
    float getAnalogInput(ANALOGSTICKS stick)
    {
        switch (stick)
        {
            case ANALOGSTICKS.LEFT_X:
                return GetLeftX(playerNumber);
            case ANALOGSTICKS.LEFT_Y:
                return GetLeftY(playerNumber);
            case ANALOGSTICKS.RIGHT_X:
                return GetRightX(playerNumber);
            case ANALOGSTICKS.RIGHT_Y:
                return GetRightY(playerNumber);

        }
        Debug.Log("error invalid analog stick");
        return 0.0f;
    }
    //returns a value between 0 and 1 for the right trigger
    public float GetRightTrigger()
    {
        return GetRightTrigger(playerNumber);
    }
    //returns a value between 0 and 1 for the left trigger
    public float GetLeftTrigger()
    {
        return GetLeftTrigger(playerNumber);
    }
    public float GetTrigger(TRIGGERS triggers)
    {
        if (triggers == TRIGGERS.LEFT)
        {
            return GetLeftTrigger();
        }
        else
        {
            return GetRightTrigger();
        }
    }
    //sets the rumble of the controller. NOTE to self could overload the function to have a duration version.
    public void SetRumble(float leftRumble, float rightRumble)
    {
        SetRumble(playerNumber, leftRumble, rightRumble);
    }

    

    // A coroutine to have a rumble over a duration with decreasing intensity [Graham]
    // Intensity between 0-1, duration in seconds
    public IEnumerator RumbleFor(float duration, float intensity)
    {
        if (duration <= 0f) yield break;

        float elapsed = 0f;
        while(elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t;     // For an exponential decline [Graham]
            float newIntensity = Mathf.Lerp(intensity, 0f, t);
            Debug.Log(newIntensity);
            SetRumble(newIntensity, newIntensity);

            //elapsed += 0.05f;
            //yield return new WaitForSeconds(0.05f);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        SetRumble(0f, 0f);
        yield return null;
    }
    #endregion

    //The playernumber is used for the controllerID
    public int playerNumber;

    //The controls for the player
    public Aim aimControls;
    public Move moveControls;
    public Trigger_Button shootLaser;
    public Button jump;
    public Trigger_Button boost;
    public Button shootShotgun;
    public Button start;

    // Select, selects colors, changeColors changes colors [jack]
    public Button select;
    public Button deselect;
    public Aim changeColors;

    //This region uses the above variables to fetch the controller state
    #region controls

    public bool GetStart(BUTTON_DETECTION detect = BUTTON_DETECTION.GET_BUTTON_DOWN)
    {
        return GetButtonStruct(start, detect);
    }

    //returns a vector2 of -1 to 1 values for aim analogue controls
    public Vector2 GetAim()
    {
        Vector2 result;
        result.x = getAnalogInput(aimControls.aimX);
        result.y = getAnalogInput(aimControls.aimY);
        return result;
    }
    //returns a vector2 of -1 to 1 values for move analogue controls
    public Vector2 GetMove()
    {
        Vector2 result;
        result.x = getAnalogInput(moveControls.moveX);
        result.y = getAnalogInput(moveControls.moveY);
        return result;
    }
    //returns true or false using the shotgun controls
    public bool GetShootShotgun(BUTTON_DETECTION detect = BUTTON_DETECTION.GET_BUTTON)
    {
        return GetButtonStruct(shootShotgun, detect);
    }
    //Return the pressure of trigger
    public float GetShootShotgunFloat()
    {
        return 0.0f;
    }
    //returns true or false using the jump controls.
    public bool GetJump(BUTTON_DETECTION detect = BUTTON_DETECTION.GET_BUTTON)
    {
        return GetButtonStruct(jump, detect);
    }
    //returns true or false using the select controls [jack]
    public bool GetSelect(BUTTON_DETECTION detect = BUTTON_DETECTION.GET_BUTTON_DOWN)
    {
        return GetButtonStruct(select, detect);
    }
    public bool GetDeselect(BUTTON_DETECTION detect = BUTTON_DETECTION.GET_BUTTON_DOWN)
    {
        return GetButtonStruct(deselect, detect);
    }
    //returns left or right using the select controls (left analog stick) [Jack]
    public int GetColorChange()
    {
        float result;
        result = getAnalogInput(changeColors.aimX);

        if(result > 0.300 && GetComponent<PlayerStats>().canChangeColour)
        {
            GetComponent<PlayerStats>().canChangeColour = false;
            return 1;
        }
        if (result < -0.300 && GetComponent<PlayerStats>().canChangeColour)
        {
            GetComponent<PlayerStats>().canChangeColour = false;
            return -1;
        }
        if (result >= -0.300 && result <= 0.300)
        {
            GetComponent<PlayerStats>().canChangeColour = true;
            return 0;
        }
        else
            return 0;
    }

    public bool GetShootLaser(BUTTON_DETECTION detect = BUTTON_DETECTION.GET_BUTTON)
    {
        //set last state equal the current state
        shootLaser.lastState = shootLaser.currentState;

        //find out the trigger state
        if (shootLaser.trigger.sensitivity < GetTrigger(shootLaser.trigger.trigger))
        {
            shootLaser.currentState = true;
        }
        else
        {
            shootLaser.currentState = false;
        }

        //if the button is being pressed return true
        if (GetButtonStruct(shootLaser.button, detect))
        {
            return true;
        }
        switch (detect)
        {
            case BUTTON_DETECTION.GET_BUTTON:
                return shootLaser.currentState;

            case BUTTON_DETECTION.GET_BUTTON_DOWN:
                if (shootLaser.currentState == true && shootLaser.lastState == false)
                    return true;
                else
                    return false;

            case BUTTON_DETECTION.GET_BUTTON_UP:
                if (shootLaser.currentState == false && shootLaser.lastState == true)
                    return true;
                else
                    return false;

            default:
                Debug.Log("error no button_detection selected");
                return false;

        }

    }

    //returns true or false using the boost controls.
    public bool GetBoost(BUTTON_DETECTION detect = BUTTON_DETECTION.GET_BUTTON)
    {
        //set last state equal the current state
        boost.lastState = boost.currentState;

        //find out the trigger state
        if (boost.trigger.sensitivity < GetTrigger(boost.trigger.trigger))
        {
            boost.currentState = true;
        }
        else
        {
            boost.currentState = false;
        }

        //if the button is being pressed return true
        if (GetButtonStruct(boost.button, detect))
        {
            return true;
        }
        switch (detect)
        {
            case BUTTON_DETECTION.GET_BUTTON:
                return boost.currentState;

            case BUTTON_DETECTION.GET_BUTTON_DOWN:
                if (boost.currentState == true && boost.lastState == false)
                    return true;
                else
                    return false;

            case BUTTON_DETECTION.GET_BUTTON_UP:
                if (boost.currentState == false && boost.lastState == true)
                    return true;
                else
                    return false;

            default:
                Debug.Log("error no button_detection selected");
                return false; 

        }

    }
    



    //Uses the Button struct to detimine what buttons to query the state of and in what manner
    bool GetButtonStruct(Button button, BUTTON_DETECTION detect)
    {
        //uses the detection enum to find what method of detection.
        switch (detect)
        {
            case BUTTON_DETECTION.GET_BUTTON:
                if (GetButton(button.key) || GetButton(button.altKey))
                    return true;
                else
                    return false;

            case BUTTON_DETECTION.GET_BUTTON_DOWN:
                if (GetButtonDown(button.key) || GetButtonDown(button.altKey))
                    return true;
                else
                    return false;

            case BUTTON_DETECTION.GET_BUTTON_UP:
                if (GetButtonUp(button.key) || GetButtonUp(button.altKey))
                    return true;
                else
                    return false;

            default:
                Debug.Log("error no button_detection selected");
                return false;

        }
    }

    #endregion
    //temp variables to differentiate players

    // Use this for initialization
    void Start()
    {
        playerNumber = GetComponent<PlayerStats>().m_PlayerID;
        Debug.Log("Player number: " + playerNumber);
        if (!CreateController(playerNumber))
        {
            Debug.Log("error this controller already exists");
        }
        if (ControllerUpdate(playerNumber) == false)
        {
            //gameObject.active = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ControllerUpdate(playerNumber);
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    private void OnApplicationQuit()
    {
        DeleteController(playerNumber);
    }


}
