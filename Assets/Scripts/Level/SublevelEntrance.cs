using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SublevelEntrance : MonoBehaviour
{
    bool isInSublevel = false;
    [SerializeField] ParticleSystem portalEnterVfx;
    [SerializeField] AudioClip portalEnterSfx;

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if(Collision.TryGetComponent(out PlayerShip _))
        {
            portalEnterVfx.Play();
            AudioManager.Instance.PlayAudioClip(portalEnterSfx);

            if (!isInSublevel)
            {
                GameController.Instance.FlyingStateInstance.LoadLevel
                    (GameController.AllLevels[GameController.CurrentLevel].SubLevel);
                isInSublevel = true;
                return;
            }

            if (isInSublevel)
            {
                GameController.Instance.FlyingStateInstance.LoadLevel
                    (GameController.AllLevels[GameController.CurrentLevel]);
                isInSublevel = false;
            }
        }
    }
}
