using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;
    private static int debugLastExplodedId = 0;

    public int id;
    public Rigidbody rigidBody;
    public int thrownByPlayer;
    public Vector3 initialForce;
    public float explosionRadius = 1.75f;
    public float explosionDamage = 75f;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this, thrownByPlayer);

        rigidBody.AddForce(initialForce);
        StartCoroutine(ExplodeAfterSeconds(10f));
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"#{id}: {collision.transform.name} collision called Explode().");
        Explode();
    }

    public void Initialize(Vector3 _initialMovementDir, float _initialForceStrength, int _thrownByPlayer)
    {
        initialForce = _initialMovementDir * _initialForceStrength;
        thrownByPlayer = _thrownByPlayer;
    }

    private void Explode()
    {
        if (debugLastExplodedId == id)
        {
            //Time.timeScale = 0f; //debug
            Debug.LogWarning($"Explode() has already been called for projectile #{id}.");
            return;
        }

        ServerSend.ProjectileExploded(this);

        Collider[] _colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider _collider in _colliders)
        {
            if (_collider.CompareTag("Player"))
            {
                _collider.GetComponent<Player>().TakeDamage(explosionDamage);
            }
            else if (_collider.CompareTag("Enemy"))
            {
                _collider.GetComponent<Enemy>().TakeDamage(explosionDamage);
            }
        }

        debugLastExplodedId = id;
        projectiles.Remove(id);
        Destroy(gameObject);
        Debug.Log($"#{id} destroyed.");
    }

    private IEnumerator ExplodeAfterSeconds(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);
        Debug.Log($"#{id}: ExplodeAfterSeconds() coroutine called Explode().");
        Explode();
    }
}
