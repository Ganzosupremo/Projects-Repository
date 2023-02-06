using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScreenCursor : MonoBehaviour
{
    public static ScreenCursor Instance { get { return instance; } }
    private static ScreenCursor instance;
    
    public Sprite defaultCrosshair;
    public float cursorSpeed;
    public InputAction rightJoystick;
    private static Image cursorImage;

    private void Awake()
    {
        instance = this; 
        cursorImage = GetComponent<Image>();
        cursorImage.sprite = defaultCrosshair;
        rightJoystick.Enable();
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Gamepad.current != null)
        {
            transform.position = cursorSpeed * Time.deltaTime * Gamepad.current.rightStick.ReadValue();
            //Debug.Log(Gamepad.current.rightStick.ReadValue());
        }
        else
        {
            transform.position = Mouse.current.position.ReadValue();
            //Debug.Log(Mouse.current.position.ReadValue());
        }
    }

    /// <summary>
    /// Changes the screen crosshair
    /// </summary>
    /// <param name="crosshair">The sprite to change the crosshair to</param>
    public void ChangeCrosshair(Sprite crosshair)
    {
        if (crosshair != null)
            cursorImage.sprite = crosshair;
        else
            cursorImage.sprite = defaultCrosshair;
    }
}
