using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = .2f;
    public LayerMask layerMask;

    //flags
    public bool below;


    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody2d;
    private CapsuleCollider2D _capsuleCollider2d;

    private Vector2[] _raycastPositions = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3];

    private void Awake()
    {
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _capsuleCollider2d = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        _lastPosition = _rigidbody2d.position;

        _currentPosition = _lastPosition + _moveAmount;

        _rigidbody2d.MovePosition(_currentPosition);

        _moveAmount = Vector2.zero;

        CheckGrounded();
    }

    //Can fire multiple times
    //Cumumalative
    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    private void CheckGrounded()
    {
        Vector2 raycastOrigin = _rigidbody2d.position - new Vector2(0, _capsuleCollider2d.size.y * .5f);

        _raycastPositions[0] = raycastOrigin + (Vector2.left * _capsuleCollider2d.size.x * .25f + Vector2.up * .02f);
        _raycastPositions[1] = raycastOrigin;
        _raycastPositions[2] = raycastOrigin + (Vector2.right * _capsuleCollider2d.size.x * .25f + Vector2.up * .02f);

        DrawDebugRays(Vector2.down, Color.green);

        int numberOfGroundHits = 0;

        for (int i = 0; i < _raycastPositions.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(_raycastPositions[i], Vector2.down, raycastDistance, layerMask);

            if (hit.collider)
            {
                _raycastHits[i] = hit;
                numberOfGroundHits++;
            }
        }

        if(numberOfGroundHits > 0)
        {
            below = true;
        }
        else
        {
            below = false;
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
