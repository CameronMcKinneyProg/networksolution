using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class TCP
{
    public TcpClient socket;

    private readonly int id;
    private NetworkStream stream;
    private Packet receivedData;
    private byte[] receiveBuffer;
    private int dataBufferSize;

    public TCP(int _id, int _dataBufferSize)
    {
        id = _id;
        dataBufferSize = _dataBufferSize;
    }

    public void Connect(TcpClient _socket)
    {
        socket = _socket;
        socket.ReceiveBufferSize = dataBufferSize;
        socket.SendBufferSize = dataBufferSize;

        stream = socket.GetStream();

        receivedData = new Packet();

        receiveBuffer = new byte[dataBufferSize];

        stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

        ServerSend.Welcome(id, "Welcome to the server!");
    }

    public void SendData(Packet _packet)
    {
        try
        {
            if (socket != null)
            {
                stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to player {id} via TCP: {_ex}");
        }
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        try
        {
            int _byteLength = stream.EndRead(_result);
            if (_byteLength <= 0)
            {
                Server.clients[id].Disconnect();
                return;
            }

            byte[] _data = new byte[_byteLength];
            Array.Copy(receiveBuffer, _data, _byteLength);

            receivedData.Reset(HandleData(_data));
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error receiving TCP data: {_ex}");
            Server.clients[id].Disconnect();
        }
    }

    private bool HandleData(byte[] _data)
    {
        int _packetLength = 0;

        receivedData.SetBytes(_data);

        if (receivedData.UnreadLength() >= 4) // all packets start with a 4-byte int describing packet length
        {
            _packetLength = receivedData.ReadInt();
            if (_packetLength <= 0)
            {
                return true;
            }
        }

        while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) // receivedData contains at least 1 complete packet we can handle
        {
            byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet);
                }
            });

            _packetLength = 0;
            if (receivedData.UnreadLength() >= 4) // all packets start with a 4-byte int describing packet length
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }
        }

        if (_packetLength <= 1)
        {
            return true;
        }

        return false;
    }

    public void Disconnect()
    {
        socket.Close();
        stream = null;
        receivedData = null;
        receiveBuffer = null;
        socket = null;
    }
}
