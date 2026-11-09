using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputActionTEST : MonoBehaviour
{
    Vector2 moveInput;

    private void Update()
    {
        Vector3 direction = new Vector3(
            moveInput.x,
            0f,
            moveInput.y);

        transform.Translate(
            direction * Time.deltaTime * 5f);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
