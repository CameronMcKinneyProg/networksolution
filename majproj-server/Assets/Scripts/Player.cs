using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerInput
{
    public bool forward;
    public bool back;
    public bool left;
    public bool right;
    public bool jump;
};

public struct PlayerState
{
    public Vector3 position;
    public float yVelocity;
    public bool isGrounded;
};

public struct PlayerMove
{
    public long id;
    public float time;
    public PlayerState state;
    public PlayerInput input;
}

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float throwForce = 600f;
    public float shootDamage = 50f;
    public float shootRange = 25f;
    public float health;
    public float maxHealth = 100f;
    public int itemAmount = 0;
    public int maxItemAmount = 3;
    public long mostRecentMoveId = 0L;
    public float yVelocity = 0f;

    private bool[] inputs;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[5];
    }

    /*public void FixedUpdate()
    {
        if (health <= 0f)
        {
            return;
        }

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);
    }*/

    public void ProcessInput(PlayerMove _move)
    {
        if (health <= 0f)
        {
            return;
        }

        mostRecentMoveId = _move.id; // update stored remote move id

        //float _deltaTime = _move.time - Server.clients[id].mostRecentRemoteTime; // calculate time passed since stored remote time
        Server.clients[id].mostRecentRemoteTime = _move.time; // update stored remote time

        Vector2 _inputDirection = Vector2.zero;
        if (_move.input.forward)
        {
            _inputDirection.y += 1;
        }
        if (_move.input.back)
        {
            _inputDirection.y -= 1;
        }
        if (_move.input.left)
        {
            _inputDirection.x -= 1;
        }
        if (_move.input.right)
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection, _move.input.jump);
    }

    private void Move(Vector2 _inputDirection, bool _jump)
    {
        //float deltaOffset = _deltaTime / Time.fixedDeltaTime;

        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (_jump)
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(Quaternion _rotation)
    {
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _shootDirection)
    {
        if (health <= 0f)
        {
            return;
        }

        if (Physics.Raycast(shootOrigin.position, _shootDirection, out RaycastHit _hit, shootRange))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(shootDamage);
            }
            else if (_hit.collider.CompareTag("Enemy"))
            {
                _hit.collider.GetComponent<Enemy>().TakeDamage(shootDamage);
            }
        }
    }

    public void ThrowItem(Vector3 _viewDir)
    {
        if (health <= 0f)
        {
            return;
        }

        if (itemAmount > 0)
        {
            itemAmount--;
            NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialize(_viewDir, throwForce, id);
        }
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0f)
        {
            return;
        }

        health -= _damage;
        if (health <= 0f)
        {
            Die();
        }

        ServerSend.PlayerHealth(this);
    }

    private void Die()
    {
        health = 0f;
        controller.enabled = false;
        transform.position = new Vector3(0f, 30f, 0f);
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        transform.position = new Vector3(0f, 5f, 0f);
        ServerSend.PlayerRespawned(this);
    }

    public bool AttemptPickupItem()
    {
        if (itemAmount >= maxItemAmount)
        {
            return false;
        }

        itemAmount++;
        return true;
    }
}
