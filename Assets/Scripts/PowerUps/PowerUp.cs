using System;
using System.Collections;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] PowerUpType powerUpType;
    const float MOVE_SPEED = 0.35f;
    const float LIFE_TIME = 3.5f;

    void Start()
    {
        StartCoroutine(MoveToFloor());
        StartCoroutine(TimeToLive());
    }

    IEnumerator TimeToLive()
    {
        yield return new WaitForSeconds(LIFE_TIME);
        Destroy(this.gameObject);
    }

    IEnumerator MoveToFloor()
    {
        while (transform.position.y > -GameController.GetMapBoundsYVal)
        {
            transform.position = new(transform.position.x,
                transform.position.y - MOVE_SPEED * Time.deltaTime);
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if (Collision.TryGetComponent(out PlayerShip _))
        {
            PowerUpManager.Instance.ApplyPowerUp(powerUpType);
            Destroy(this.gameObject);
        }
    }
}
