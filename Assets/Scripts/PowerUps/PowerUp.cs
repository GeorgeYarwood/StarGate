using System;
using System.Collections;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] PowerUpType powerUpType;
    [SerializeField] AudioClip onCollectAudio;
    const float MOVE_SPEED = 0.35f;
    const float LIFE_TIME = 3.5f;

    void Start()
    {
        StartCoroutine(MoveToFloor());
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

        StartCoroutine(TimeToLive());
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if (Collision.TryGetComponent(out PlayerController _))
        {
            PowerUpManager.Instance.ApplyPowerUp(powerUpType);
            if (onCollectAudio)
            {
                AudioManager.Instance.PlayAudioClip(onCollectAudio);
            }
            Destroy(this.gameObject);
        }
    }
}
