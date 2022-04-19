using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody2d;
    private CapsuleCollider2D _capsuleCollider2d;

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
    }

    //Can fire multiple times
    //Cumumalative
    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }
}
