using GlobalTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = .2f;
    public LayerMask layerMask;

    public float slopeAngleLimit = 45f;
    public float downForceMultiplier = 1.2f;

    //flags
    public bool below;
    public bool left;
    public bool right;
    public bool above;

    public GroundType groundType;
    public WallType leftWallType;
    public WallType rightWallType;
    public GroundType ceilingType;

    public bool hitGroundThisFrame;
    public bool hitWallThisFrame;

    public float jumpPadAmount;
    public float jumpPadUpperLimit;

    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody2d;
    private CapsuleCollider2D _capsuleCollider2d;

    private Vector2[] _raycastPositions = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3];

    private bool _disableGroundCheck;

    private Vector2 _slopeNormal;
    private float _slopeAngle;

    private bool _inAirLastFrame;
    private bool _noSideCollisionLastFrame;

    private Transform _tempMovingPlatform;
    private Vector2 _movingPlatformVelocity;

    private void Awake()
    {
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _capsuleCollider2d = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        _inAirLastFrame = !below;

        _noSideCollisionLastFrame = (right == false && left == false);

        _lastPosition = _rigidbody2d.position;

        // slope adjustment
        if (_slopeAngle != 0 && below == true)
        {
            if ((_moveAmount.x > 0f && _slopeAngle > 0f) || (_moveAmount.x < 0f && _slopeAngle < 0f))
            {
                _moveAmount.y = -Math.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) * _moveAmount.x);
                _moveAmount.y *= downForceMultiplier;
            }
        }

        // Moving platform adjustment

        if(groundType == GroundType.MovingPlatform)
        {
            //Offset the player movement on the x with moving platform velocity
            _moveAmount.x += MovingPlatformAdjust().x;

            //If platform is moving down  offset the player's movement on the y
            if(MovingPlatformAdjust().y < 0f)
            {
                _moveAmount.y += MovingPlatformAdjust().y;
                _moveAmount.y *= downForceMultiplier;
            }   
        }

        _currentPosition = _lastPosition + _moveAmount;

        _rigidbody2d.MovePosition(_currentPosition);

        _moveAmount = Vector2.zero;

        if (!_disableGroundCheck)
        {
            CheckGrounded();
        }

        CheckOtherCollisions();

        if (below == true && _inAirLastFrame == true)
        {
            hitGroundThisFrame = true;
        }
        else
        {
            hitGroundThisFrame = false;
        }

        if((right == true || left == true) && _noSideCollisionLastFrame == true)
        {
            hitWallThisFrame = true;
        }
        else
        {
            hitWallThisFrame = false;
        }
    }

    //Can fire multiple times
    //Cumumalative
    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    //private void CheckGrounded()
    //{
    //    Vector2 raycastOrigin = _rigidbody2d.position - new Vector2(0, _capsuleCollider2d.size.y * .5f);

    //    _raycastPositions[0] = raycastOrigin + (Vector2.left * _capsuleCollider2d.size.x * .25f + Vector2.up * -.08f);
    //    _raycastPositions[1] = raycastOrigin + Vector2.up * -.13f;
    //    _raycastPositions[2] = raycastOrigin + (Vector2.right * _capsuleCollider2d.size.x * .25f + Vector2.up * -.08f);

    //    DrawDebugRays(Vector2.down, Color.green);

    //    int numberOfGroundHits = 0;

    //    for (int i = 0; i < _raycastPositions.Length; i++)
    //    {
    //        RaycastHit2D hit = Physics2D.Raycast(_raycastPositions[i], Vector2.down, raycastDistance, layerMask);

    //        if (hit.collider)
    //        {
    //            _raycastHits[i] = hit;
    //            numberOfGroundHits++;
    //        }

    //    }

    //    if (numberOfGroundHits > 0)
    //    {
    //        if (_raycastHits[1].collider)
    //        {
    //            groundType = DetermineGroundType(_raycastHits[1].collider);
    //            _slopeNormal = _raycastHits[1].normal;
    //            _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);
    //        }
    //        else
    //        {
    //            for (int i = 0; i < _raycastHits.Length; i++)
    //            {
    //                if (_raycastHits[i].collider)
    //                {
    //                    groundType = DetermineGroundType(_raycastHits[i].collider);
    //                    _slopeNormal = _raycastHits[i].normal;
    //                    _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);
    //                }
    //            }
    //        }

    //        // Check if slope is to steep
    //        if(_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
    //        {
    //            below = false;
    //        }
    //        else
    //        {
    //            below = true;

    //        }
    //    }
    //    else
    //    {
    //        groundType = GroundType.None;
    //        below = false;
    //    }

    //    Array.Clear(_raycastHits, 0, _raycastHits.Length);
    //}

    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider2d.bounds.center, _capsuleCollider2d.bounds.size, CapsuleDirection2D.Vertical,
            0f, Vector2.down, raycastDistance, layerMask);

        if (hit.collider)
        {
            groundType = DetermineGroundType(hit.collider);

            _slopeNormal = hit.normal;
            _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);

            if (_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }

            if(groundType == GroundType.JumpPad)
            {
                JumpPad jumpPad = hit.collider.GetComponent<JumpPad>();
                jumpPadAmount = jumpPad.jumpPadAmount;
                jumpPadUpperLimit = jumpPad.jumpPadUpperLimit;
            }
        }
        else
        {
            groundType = GroundType.None;
            below = false;

            if (_tempMovingPlatform)
            {
                _tempMovingPlatform = null;
            }
        }
    }

    private void CheckOtherCollisions()
    {
        //Check Left
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider2d.bounds.center, _capsuleCollider2d.size * .75f, 0f, Vector2.left,
            raycastDistance, layerMask);

        if (leftHit.collider)
        {
            leftWallType = DeterminWallType(leftHit.collider);
            left = true;
        }
        else
        {
            leftWallType = WallType.None;

            left = false;
        }

        //Check Right
        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider2d.bounds.center, _capsuleCollider2d.size * .75f, 0f, Vector2.right,
           raycastDistance, layerMask);

        if (rightHit.collider)
        {
            rightWallType = DeterminWallType(rightHit.collider);

            right = true;
        }
        else
        {
            rightWallType = WallType.None;
            right = false;
        }

        //Check Above
        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider2d.bounds.center, _capsuleCollider2d.bounds.size, CapsuleDirection2D.Vertical,
           0f, Vector2.up, raycastDistance, layerMask);

        if (aboveHit.collider)
        {
            ceilingType = DetermineGroundType(aboveHit.collider);
            above = true;
        }
        else
        {
            ceilingType = GroundType.None;
            above = false;
        }
    }

    private GroundType DetermineGroundType(Collider2D collider)
    {
        if (collider.GetComponent<GroundEffector>())
        {
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            
            if(groundType == GroundType.MovingPlatform)
            {
                if (!_tempMovingPlatform)
                {
                    _tempMovingPlatform = collider.transform;
                }
            }

            return groundEffector.groundType;
        }
        else
        {
            return GroundType.LevelGeometry;
        }
    }

    private WallType DeterminWallType(Collider2D collider)
    {
        if (collider.GetComponent<WallEffector>())
        {
            WallEffector wallEffector = collider.GetComponent<WallEffector>();
            return wallEffector.wallType;
        }
        else
        {
            return WallType.Normal;
        }
    }

    public void DisableGroundCheck()
    {
        below = false;
        _disableGroundCheck = true;
        StartCoroutine("EnableGroundCheck");
    }

    private IEnumerator EnableGroundCheck()
    {
        yield return new WaitForSeconds(.1f);
        _disableGroundCheck = false;
    }

    private Vector2 MovingPlatformAdjust()
    {
        if(_tempMovingPlatform && groundType == GroundType.MovingPlatform) 
        {
            _movingPlatformVelocity = _tempMovingPlatform.GetComponent<MovingPlatform>().difference;

            return _movingPlatformVelocity;
        }
        else
        {
            return Vector2.zero;
        }
    }

    private void DrawDebugRays(Vector2 direction, Color color)
    {
        for (int i = 0; i < _raycastPositions.Length; i++)
        {
            Debug.DrawRay(_raycastPositions[i], direction * raycastDistance, color);
        }
    }
}
