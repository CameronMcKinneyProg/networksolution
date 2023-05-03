using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public Transform camTransform;
    public PlayerManager playerManager;
    public CharacterController charController;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;

    private float yVelocity = 0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("PlayerController instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {
        if (playerManager.health <= 0f) // only server can cause this
        {
            charController.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f);
            return;
        }
        else
        {
            charController.enabled = true;
        }

        // get input for this physics update
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space)
        };

        // send RPCs to server
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(camTransform.forward); // RPC
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ClientSend.PlayerThrowItem(camTransform.forward); // RPC
        }

        InputSnapshotMove _snapshot = SnapshotManager.instance.NewInputSnapshotMove(_inputs);
        ClientSend.PlayerMovement(_snapshot);

        // client prediction
        PredictMovement(ProcessInput(_inputs), _inputs[4]);
    }

    private Vector2 ProcessInput(bool[] _inputs)
    {
        Vector2 _inputDirection = Vector2.zero;
        if (_inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (_inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (_inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (_inputs[3])
        {
            _inputDirection.x += 1;
        }

        return _inputDirection;
    }

    private void PredictMovement(Vector2 _inputDirection, bool jump)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (charController.isGrounded)
        {
            yVelocity = 0f;
            if (jump)
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        charController.Move(_moveDirection);
    }
}
