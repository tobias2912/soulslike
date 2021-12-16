using UnityEngine;
using UnityEngine.InputSystem;

public class InputEventReader : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool lightAttack;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    public void Move(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        jump = context.ReadValue<float>()>0.5f;
    }
    public void LightAttack(InputAction.CallbackContext context)
    {
        lightAttack = context.ReadValue<float>()>0.5f;
    }
    public void Sprint(InputAction.CallbackContext context)
    {
        System.Type valueType = context.valueType;
        float v = context.ReadValue<float>();
        sprint = v > 0.5f;
    }
    public void Look(InputAction.CallbackContext context){
        if(cursorInputForLook){
            look = context.ReadValue<Vector2>();
        }
    }
    



    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

}
