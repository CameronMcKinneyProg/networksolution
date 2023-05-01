using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class UDP
{
    public UdpClient socket;
    public IPEndPoint endPoint;

    public UDP()
    {
        endPoint = new IPEndPoint(IPAddress.Parse(Client.instance.hostIp), Client.instance.hostPort);
    }

    public void Connect(int _localPort)
    {
        socket = new UdpClient(_localPort);

        socket.Connect(endPoint);
        socket.BeginReceive(ReceiveCallback, null);

        using (Packet _packet = new Packet())
        {
            SendData(_packet);
        }
    }

    public void SendData(Packet _packet)
    {
        try
        {
            _packet.InsertInt(Client.instance.myId);
            if (socket != null)
            {
                socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.LogError($"Error sending data to server via UDP: {_ex}");
        }
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        try
        {
            byte[] _data = socket.EndReceive(_result, ref endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            if (_data.Length < 4)
            {
                Client.instance.Disconnect();
                return;
            }

            HandleData(_data);
        }
        catch
        {
            Disconnect();
        }
    }

    private void HandleData(byte[] _data)
    {
        using (Packet _packet = new Packet(_data))
        {
            int _packetLength = _packet.ReadInt();
            _data = _packet.ReadBytes(_packetLength);
        }

        ThreadManager.ExecuteOnMainThread(() =>
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetId = _packet.ReadInt();
                Client.packetHandlers[_packetId](_packet);
            }
        });
    }

    private void Disconnect()
    {
        Client.instance.Disconnect();

        endPoint = null;
        socket = null;
    }
}
