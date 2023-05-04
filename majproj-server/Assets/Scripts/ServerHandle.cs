using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed an incorrect client ID ({_clientIdCheck})!");
        }

        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void Ping(int _fromClient, Packet _packet)
    {
        //float _newClientTime = _packet.ReadFloat();
        //Server.clients[_fromClient].remoteTime = _newClientTime;

        Server.clients[_fromClient].SendPong();
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        // create new move
        PlayerMove _move = new PlayerMove();

        // move id
        _move.id = _packet.ReadLong();

        // move time
        _move.time = _packet.ReadFloat();

        // check if this move should be processed
        //if (_move.time < Server.clients[_fromClient].mostRecentRemoteTime)
        //{
        //    return; // old packet; discard
        //}
        if (_move.id < Server.clients[_fromClient].player.mostRecentMoveId)
        {
            return; // old packet; discard
        }

        // move inputs
        _move.input.forward = _packet.ReadBool();
        _move.input.back = _packet.ReadBool();
        _move.input.left = _packet.ReadBool();
        _move.input.right = _packet.ReadBool();
        _move.input.jump = _packet.ReadBool();

        // move state
        //_move.state.position = _packet.ReadVector3();
        //_move.state.yVelocity = _packet.ReadFloat();
        //_move.state.isGrounded = _packet.ReadBool();

        // rotation
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.ProcessInput(_move);
        Server.clients[_fromClient].player.SetInput(_rotation); // legacy
    }

    public static void PlayerShoot(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.Shoot(_shootDirection);
    }

    public static void PlayerThrowItem(int _fromClient, Packet _packet)
    {
        Vector3 _throwDir = _packet.ReadVector3();

        Server.clients[_fromClient].player.ThrowItem(_throwDir);
    }
}
