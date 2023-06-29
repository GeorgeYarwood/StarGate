using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SublevelEntrance : MonoBehaviour
{
    bool isInSublevel = false;
    public bool IsInSublevel
    {
        get { return isInSublevel; }
        set { isInSublevel = value; }
    }
    [SerializeField] ParticleSystem portalEnterVfx;
    [SerializeField] AudioClip portalEnterSfx;

    //Just one entrance for all levels at the moment
    static SublevelEntrance instance;
    static public SublevelEntrance Instance
    {
        get { return instance;}
    }

    bool canReEnter = true;
    const float RE_ENTRY_TIMER = 1.0f;

    void Start()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if(Collision.TryGetComponent(out PlayerShip _))
        {
            StartCoroutine(BlockInteractionForTime()); //Stop up immediately going straight back in
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

    IEnumerator BlockInteractionForTime()
    {
        canReEnter = false;
        yield return new WaitForSeconds(RE_ENTRY_TIMER);
        canReEnter = true;
    }
}
