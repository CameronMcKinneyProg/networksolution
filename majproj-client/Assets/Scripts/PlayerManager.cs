using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerInput
{
    bool forward;
    bool back;
    bool left;
    bool right;
    bool jump;
};

public struct PlayerState
{
    Vector3 position;
    Vector3 yVelocity;
};

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100f;
    public int itemCount = 0;
    public MeshRenderer model;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        model.enabled = true;
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }
}
