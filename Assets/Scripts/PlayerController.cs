using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float gravity = 20f;

    private Vector2 _input;
    private Vector2 _moveDirections;
    private CharacterController2D _characterController;

    private void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
    }

    private void Update()
    {
        _moveDirections.x = _input.x;
        _moveDirections.x *= walkSpeed;

        if (_characterController.below)
        {

        }
        else
        {
            _moveDirections.y -= gravity * Time.deltaTime;
        }

        _characterController.Move(_moveDirections * Time.deltaTime);
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }
}
