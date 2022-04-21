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

        if(_moveDirections.x < 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else if(_moveDirections.x > 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        }

        if (_characterController.below)
        {
            _moveDirections.y = 0f;
            isJumping = false;

            if (_startJump)
            {
                _startJump = false;
                _moveDirections.y = jumpSpeed;
                isJumping = true;
                _characterController.DisableGroundCheck();
            }
        }
        else // In the air
        {
            if (_releaseJump)
            {
                _releaseJump = false;

                if (_moveDirections.y > 0)
                {
                    _moveDirections.y *= .5f;
                }
            }

            GravityCalculations();
        }

        _characterController.Move(_moveDirections * Time.deltaTime);
    }

    private void GravityCalculations()
    {

        if(_moveDirections.y > 0f && _characterController.above)
        {
            _moveDirections.y = 0f;
        }

        _moveDirections.y -= gravity * Time.deltaTime;

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
            _releaseJump = false;

        }
        else if (context.canceled)
        {
            _releaseJump = true;
            _startJump = false;
        }
    }
}
