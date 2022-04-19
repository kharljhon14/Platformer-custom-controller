using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Player properties
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;

    //Input flags
    private bool _startJump;
    private bool _releaseJump;

    //Player states
    public bool isJumping;

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
            if (_startJump)
            {
                _startJump = false;
                _moveDirections.y = jumpSpeed;
                isJumping = true;
            }
        }

        else // In the air
        {
            if (_releaseJump)
            {
                _releaseJump = false;
                
                if(_moveDirections.y > 0)
                {
                    _moveDirections.y *= .5f;
                }
            }

            _moveDirections.y -= gravity * Time.deltaTime;
        }

        _characterController.Move(_moveDirections * Time.deltaTime);
    }

    //Input Methods
    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _startJump = true;
        }
        
        else if (context.canceled)
        {
            _releaseJump = true;
        }
    }
}
