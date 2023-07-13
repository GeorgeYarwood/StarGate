using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] PowerUpType powerUpType;
    const float MOVE_SPEED = 0.35f;

    void Start()
    {
        StartCoroutine(MoveToFloor());
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
