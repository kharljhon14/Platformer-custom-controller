using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour
{
    #region public properties
    [Header("Player Properties")]
    public float walkSpeed = 10f;
    public float creepSpeed = 5f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float xWallJumpSpeed = 20f;
    public float yWallJumpSpeed = 10f;
    public float wallAmountSpeed = 8f;
    public float wallSlideSpeed = .1f;
    public float glideTime = 2f;
    public float glideDescentAmount = 2f;
    public float powerJumpSpeed = 40f;
    public float powerJumpWaitTime = 1.5f;
    public float dashSpeed = 20f;
    public float dashTime = .2f;
    public float dashCooldownTime = 2f;
    public float groundSlamSpeed = 60f;

    //Player abilities
    [Header("Player Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun;
    public bool canMultipleWallRun;
    public bool canWallSlide;
    public bool canGlide;
    public bool canGlideAfterWallContact;
    public bool canPowerJump;
    public bool canGroundDash;
    public bool canAirDash;
    public bool canGroundSlam;

    //Player states
    [Header("Player State")]
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isWallJumping;
    public bool isWallRunning;
    public bool isWallSliding;
    public bool isDucking;
    public bool isCreeping;
    public bool isGliding;
    public bool isPowerJumping;
    public bool isDashing;
    public bool isGroundSlamming;
    #endregion
    #region private properties
    //Input flags
    private bool _startJump;
    private bool _releaseJump;

    private Vector2 _input;
    private Vector2 _moveDirections;
    private CharacterController2D _characterController;

    private CapsuleCollider2D _capsuleCollider2D;
    private Vector2 _originalColliderSize;

    //Remove later when not needed
    private SpriteRenderer _spriteRenderer;

    private bool _ableToWallRun = true;

    private float _currentGlideTime;
    private bool _startGlide;

    private float _powerJumpTimer;

    private bool _facingRight;
    private float _dashTimer;
    #endregion

    private void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
        _capsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        _originalColliderSize = _capsuleCollider2D.size;
    }

    private void Update()
    {
        if (_dashTimer > 0f)
        {
            _dashTimer -= Time.deltaTime;
        }

        ProcessHorizotalMovement();

        if (_characterController.below) // On the ground
        {
            OnGround();
        }
        else // In the air
        {
            OnAir();
        }
        _characterController.Move(_moveDirections * Time.deltaTime);
    }

    private void ProcessHorizotalMovement()
    {

        if (isWallJumping == false)
        {
            _moveDirections.x = _input.x;

            if (_moveDirections.x < 0f)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                _facingRight = false;
            }
            else if (_moveDirections.x > 0f)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                _facingRight = true;
            }

            if (isDashing == true)
            {
                if (_facingRight == true)
                {
                    _moveDirections.x = dashSpeed;
                }
                else
                {
                    _moveDirections.x = -dashSpeed;
                }
                _moveDirections.y = 0f;
            }
            else
            {
                _moveDirections.x *= walkSpeed;
            }
        }
    }

    private void OnGround()
    {
        //Clear any down movement when grounded
        _moveDirections.y = 0f;

        ClearAirAbilityFlags();
        Jump();
        DuckingAndCreeping();
    }

    private void DuckingAndCreeping()
    {
        //Ducking and creeping
        if (_input.y < 0f)
        {
            if (isDucking == false && isCreeping == false)
            {
                _capsuleCollider2D.size = new Vector2(_capsuleCollider2D.size.x, _capsuleCollider2D.size.y / 2);
                transform.position = new Vector2(transform.position.x, transform.position.y - (_originalColliderSize.y / 4));
                _spriteRenderer.sprite = Resources.Load<Sprite>("_Crouch");
                isDucking = true;
            }

            _powerJumpTimer += Time.deltaTime;
        }
        else
        {
            if (isDucking == true || isCreeping == true)
            {
                RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size, //Change to transform.localscale
                    CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2, _characterController.layerMask);

                if (!hitCeiling.collider)
                {
                    _capsuleCollider2D.size = _originalColliderSize;
                    transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y / 4));
                    _spriteRenderer.sprite = Resources.Load<Sprite>("_idle");
                    isCreeping = false;
                    isDucking = false;
                }
            }

            _powerJumpTimer = 0f;
        }

        if (isDucking == true && _moveDirections.x != 0)
        {
            isCreeping = true;
            _powerJumpTimer = 0f;
        }
        else
        {
            isCreeping = false;
        }
    }

    private void Jump()
    {
        //Jumping
        if (_startJump == true)
        {
            _startJump = false;

            if (canPowerJump == true && isDucking == true && _characterController.groundType != GroundType.OneWayPlatform && (_powerJumpTimer > powerJumpWaitTime))
            {
                _moveDirections.y = powerJumpSpeed;
                StartCoroutine("PowerJumpWaiter");
            }
            else
            {
                _moveDirections.y = jumpSpeed;
            }

            isJumping = true;
            _characterController.DisableGroundCheck();
            _ableToWallRun = true;
        }
    }

    private void ClearAirAbilityFlags()
    {
        isJumping = false;
        isDoubleJumping = false;
        isTripleJumping = false;
        isWallJumping = false;
        isWallSliding = false;
        _currentGlideTime = glideTime;
        isGroundSlamming = false;
    }

    private void OnAir()
    {
        ClearGroundAbilityFlags();
        AirJump();
        WallRunning();

        GravityCalculations();
    }

    private void WallRunning()
    {
        //Wall Running
        if (canWallRun == true && (_characterController.left == true || _characterController.right == true))
        {
            if (_input.y > 0f && _ableToWallRun == true)
            {
                _moveDirections.y = wallAmountSpeed;

                if (_characterController.left == true)
                {
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else if (_characterController.right == true)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }

                StartCoroutine("WallRunWaiter");
            }
        }
        else
        {
            if (canMultipleWallRun == true)
            {
                StopCoroutine("WallRunWaiter");
                _ableToWallRun = true;
                isWallRunning = false;
            }
        }

        //Can glide after wall contact
        if ((_characterController.left == true || _characterController.right == true) && canWallRun == true)
        {
            if (canGlideAfterWallContact == true)
            {
                _currentGlideTime = glideTime;
            }
            else
            {
                _currentGlideTime = 0f;
            }
        }
    }

    private void AirJump()
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

                if (canJumpAfterWallJump == true)
                {
                    isDoubleJumping = false;
                    isTripleJumping = false;
                }

            }
            _startJump = false;
        }
    }

    private void ClearGroundAbilityFlags()
    {
        if ((isDucking == true || isCreeping == true) && _moveDirections.y > 0f)
        {
            StartCoroutine("ClearDuckingState");
        }

        //Clear jump timer
        _powerJumpTimer = 0f;
    }

    private void GravityCalculations()
    {
        //Detects if something above the player
        if (_moveDirections.y > 0f && _characterController.above)
        {
            _moveDirections.y = 0f;
        }

        //Apply wallslide adjustmet
        if (canWallSlide && (_characterController.left == true || _characterController.right == true))
        {
            if (_characterController.hitGroundThisFrame == true)
            {
                _moveDirections.y = 0f;
            }
            if (_moveDirections.y <= 0f)
            {
                _moveDirections.y -= (gravity * wallSlideSpeed * Time.deltaTime);
                isWallSliding = true;
            }
            else
            {
                _moveDirections.y -= gravity * Time.deltaTime;
            }

        }
        else if (canGlide == true && _input.y > 0f && _moveDirections.y < 0.1f) //Glide adjustment
        {
            if (_currentGlideTime > 0f)
            {
                isGliding = true;

                if (_startGlide == true)
                {
                    _moveDirections.y = 0f;
                    _startGlide = false;
                }

                _moveDirections.y -= glideDescentAmount * Time.deltaTime;
                _currentGlideTime -= Time.deltaTime;
            }
            else
            {
                isGliding = false;
                _moveDirections.y -= gravity * Time.deltaTime;
            }
        }
        //else if (canGroundSlam == true && isPowerJumping == false && _input.y < 0f && _moveDirections.y < 0f)//Ground slam
        //{
        //    _moveDirections.y = -groundSlamSpeed;
        //}
        else if (isGroundSlamming == true && isPowerJumping == false && _moveDirections.y < 0f)
        {
            _moveDirections.y = -groundSlamSpeed;
        }
        else if (isDashing == false) //Regular gravity
        {
            _moveDirections.y -= gravity * Time.deltaTime;
        }
    }

    #region Input methods
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

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && _dashTimer <= 0f)
        {
            if ((canAirDash == true && _characterController.below == false) || (canGroundDash == true && _characterController.below == true))
            {
                StartCoroutine("Dash");
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && _input.y < 0f)
        {
            if (canGroundSlam == true)
            {
                isGroundSlamming = true;
            }
        }
    }
    #endregion

    #region coroutines
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

        if (isWallJumping == false)
        {
            _ableToWallRun = false;
        }
    }

    private IEnumerator ClearDuckingState()
    {
        yield return new WaitForSeconds(0.05f);

        RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider2D.bounds.center, _capsuleCollider2D.size, //Change to transform.localscale
            CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2, _characterController.layerMask);

        if (!hitCeiling.collider)
        {
            _capsuleCollider2D.size = _originalColliderSize;
            //transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y / 4));
            _spriteRenderer.sprite = Resources.Load<Sprite>("_idle");
            isCreeping = false;
            isDucking = false;
        }
    }

    private IEnumerator PowerJumpWaiter()
    {
        isPowerJumping = true;
        yield return new WaitForSeconds(0.5f); // tweak 
        isPowerJumping = false;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        _dashTimer = dashCooldownTime;
    }
    #endregion
}
