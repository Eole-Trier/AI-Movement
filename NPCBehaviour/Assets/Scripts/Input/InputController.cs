using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public delegate void KeyEventHandler();
    public event KeyEventHandler OnUpArrowPressed;
    public event KeyEventHandler OnDownArrowPressed;
    public event KeyEventHandler OnRightArrowPressed;
    public event KeyEventHandler OnLeftArrowPressed;
    public event KeyEventHandler OnSpacePressed;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnUpArrowPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnDownArrowPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnLeftArrowPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnRightArrowPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnSpacePressed?.Invoke();
        }
    }
}
