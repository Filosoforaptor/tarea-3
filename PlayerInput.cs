using UnityEngine;
using System; // Ensured this is present

public class PlayerInput : MonoBehaviour
{
    private Vector3 inputDirection;
    private bool wasInputActive;

    public Vector3 Axis => inputDirection;

    public event Action OnInputStarted;
    public event Action OnInputStopped;
    public event Action OnFireInput; // New event for firing

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        inputDirection = new Vector3(inputX, inputY, 0f);

        bool isInputActive = (inputX != 0f || inputY != 0f);

        if (isInputActive && !wasInputActive)
        {
            OnInputStarted?.Invoke();
        }
        else if (!isInputActive && wasInputActive)
        {
            OnInputStopped?.Invoke();
        }

        wasInputActive = isInputActive;

        // Check for fire input
        if (Input.GetButtonDown("Fire1")) // "Fire1" is typically mapped to Left Ctrl, Left Mouse, or Gamepad A
        {
            Debug.LogWarning("Fire1 input detected!");
            OnFireInput?.Invoke();
        }
    }
}
