using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Player properties
    [Header("Player Properties")]
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float xWallJumpSpeed = 20f;
    public float yWallJumpSpeed = 10f;
    public float wallAmountSpeed = 8f;

    //Player abilities
    [Header("Player Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun;
    public bool canMultipleWallRun;

    //Input flags
    private bool _startJump;
    private bool _releaseJump;

    //Player states
    [Header("Player State")]
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isWallJumping;
    public bool isWallRunning;

    private Vector2 _input;
    private Vector2 _moveDirections;
    private CharacterController2D _characterController;

    private bool _ableToWallRun = true;

    private void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
    }

    private void Update()
    {
        if (isWallJumping == false)
        {
            _moveDirections.x = _input.x;
            _moveDirections.x *= walkSpeed;

            if (_moveDirections.x < 0f)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else if (_moveDirections.x > 0f)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }


        if (_characterController.below) // On the ground
        {
            _moveDirections.y = 0f;

            //Clear flags when in air abilities
            isJumping = false;
            isDoubleJumping = false;
            isTripleJumping = false;
            isWallJumping = false;

            if (_startJump == true)
            {
                _startJump = false;
                _moveDirections.y = jumpSpeed;
                isJumping = true;
                _characterController.DisableGroundCheck();
                _ableToWallRun = true;
            }
        }
        else // In the air
        {
            if (_releaseJump == true)
            {
                _releaseJump = false;

                if (_moveDirections.y > 0)
                {
                    _moveDirections.y *= .5f;
                }
            }

            //Pressed jump button air
            if (_startJump == true)
            {
                //Triple Jump
                if (canTripleJump == true && (_characterController.left == false) && (_characterController.right == false))
                {
                    if (isDoubleJumping == true && isTripleJumping == false)
                    {
                        _moveDirections.y = doubleJumpSpeed;
                        isTripleJumping = true;
                    }
                }

                //Double Jump
                if (canDoubleJump == true && (_characterController.left == false) && (_characterController.right == false))
                {
                    if (isDoubleJumping == false)
                    {
                        _moveDirections.y = doubleJumpSpeed;
                        isDoubleJumping = true;
                    }
                }

                //Wall jump
                if (canWallJump == true && (_characterController.left == true) || (_characterController.right == true))
                {
                    if (_moveDirections.x <= 0f && _characterController.left == true)
                    {
                        _moveDirections.x = xWallJumpSpeed;
                        _moveDirections.y = yWallJumpSpeed;
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                    else if (_moveDirections.x >= 0f && _characterController.right == true)
                    {
                        _moveDirections.x = -xWallJumpSpeed;
                        _moveDirections.y = yWallJumpSpeed;
                        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    }
                    //isWallJumping = true;
                    StartCoroutine("WallJumpWaiter");

                    if(canJumpAfterWallJump == true)
                    {
                        isDoubleJumping = false;
                        isTripleJumping = false;
                    }

                }
                _startJump = false;
            }

            //Wall Running
            if (canWallRun == true && (_characterController.left == true || _characterController.right == true))
            {
                if(_input.y > 0f &&  _ableToWallRun == true)
                {
                    _moveDirections.y = wallAmountSpeed;

                    if(_characterController.left == true)
                    {
                        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    }
                    else if(_characterController.right == true)
                    {
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }

                    StartCoroutine("WallRunWaiter");
                }
            }
            else
            {
                if(canMultipleWallRun == true)
                {
                    StopCoroutine("WallRunWaiter");
                    _ableToWallRun = true;
                    isWallRunning = false;
                }
            }

            GravityCalculations();
        }
        _characterController.Move(_moveDirections * Time.deltaTime);
    }

    private void GravityCalculations()
    {

        if (_moveDirections.y > 0f && _characterController.above)
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

    //Coroutines
    private IEnumerator WallJumpWaiter()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(.4f);
        isWallJumping = false;
    }

    private IEnumerator WallRunWaiter()
    {
        isWallRunning = true;
        yield return new WaitForSeconds(.5f);
        isWallRunning = false;

        if(isWallJumping == false)
        {
            _ableToWallRun = false;
        }
    }
}
