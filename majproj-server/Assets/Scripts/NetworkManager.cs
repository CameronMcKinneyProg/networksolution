using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject projectilePrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("NetworkManager instance already exists, destroying object!");
            Destroy(this);
        }
    }
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Constants.TICKS_PER_SEC;

        Server.Start(50, 29950);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<Player>();
    }

    public Enemy InstantiateEnemy(Vector3 _position)
    {
        return Instantiate(enemyPrefab, _position, Quaternion.identity).GetComponent<Enemy>();
    }

    public Projectile InstantiateProjectile(Transform _shootOrigin)
    {
        // multiply by 0.7f to avoid exploding on player that throws this projectile.
        // TODO: in Projectile.cs: check colliders against thrownByPlayer's collider before exploding.
        return Instantiate(projectilePrefab, _shootOrigin.position + _shootOrigin.forward * 0.7f, Quaternion.identity).GetComponent<Projectile>();
    }
}
