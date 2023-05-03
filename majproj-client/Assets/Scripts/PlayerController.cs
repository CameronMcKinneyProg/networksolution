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

    private List<PlayerMove> playerMoves;
    private float yVelocity;

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

        playerMoves = new List<PlayerMove>();
        yVelocity = 0f;
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

        // create new Move
        PlayerMove _move = new PlayerMove();

        // set move time
        _move.time = Time.realtimeSinceStartup;

        // set move inputs
        _move.input.forward = Input.GetKey(KeyCode.W);
        _move.input.back = Input.GetKey(KeyCode.S);
        _move.input.left = Input.GetKey(KeyCode.A);
        _move.input.right = Input.GetKey(KeyCode.D);
        _move.input.jump = Input.GetKey(KeyCode.Space);

        // set move state
        _move.state.position = transform.position;
        _move.state.yVelocity = yVelocity;
        _move.state.isGrounded = charController.isGrounded;

        // send RPCs to server
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(camTransform.forward); // RPC
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ClientSend.PlayerThrowItem(camTransform.forward); // RPC
        }
        ClientSend.PlayerMovement(_move);

        // store move in buffer
        playerMoves.Add(_move);
        //Debug.Log($"playerMoves count: {playerMoves.Count}");

        // predict outcome of move and render locally
        PredictMovement(ProcessInput(_move), _move.state.isGrounded, _move.input.jump);
    }

    private Vector2 ProcessInput(PlayerMove _move)
    {
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

        return _inputDirection;
    }

    private void PredictMovement(Vector2 _inputDirection, bool _isGrounded, bool _jump)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (_isGrounded)
        {
            yVelocity = 0f;
            if (_jump)
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        charController.Move(_moveDirection);
    }

    public void ComparePositionWithBufferedMove(Vector3 _correctPosition, float _timeOfMove)
    {
        for (int i = 0; i < playerMoves.Count; i++)
        {
            if (_timeOfMove == playerMoves[i].time)
            {
                PlayerMove _bufferedMove = playerMoves[i]; // capture compared move to check position

                playerMoves.RemoveRange(0, i + 1); // remove this and all older moves from buffer

                // compare positions
                Vector3 _difference = _correctPosition - _bufferedMove.state.position;

                float _distance = _difference.magnitude;

                if (_distance > 1.0f)
                {
                    Debug.Log($"Snap Correction, moves since last move processed by server: {i}, buffer moves to resimulate: {playerMoves.Count}");
                    transform.position = _correctPosition; // snap correction
                    ResimulateUnprocessedInputs();
                }
                else if (_distance > 0.1f)
                {
                    // exponentially smoothed moving average correction
                }

                break;
            }
        }
    }

    private void ResimulateUnprocessedInputs()
    {
        for (int i = 0; i < playerMoves.Count; i++)
        {
            PredictMovement(ProcessInput(playerMoves[i]), playerMoves[i].state.isGrounded, playerMoves[i].input.jump);
        }
    }
}
