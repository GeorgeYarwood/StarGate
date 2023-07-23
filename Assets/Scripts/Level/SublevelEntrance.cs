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
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] BoxCollider2D boxCollider;
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

    public void ToggleSublevel(bool Setting)
    {
        spriteRenderer.enabled = Setting;
        boxCollider.enabled = Setting;
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        if(Collision.TryGetComponent(out PlayerShip _) && canReEnter)
        {
            GameController.Instance.DestroyAllProjectiles();
            StartCoroutine(BlockInteractionForTime()); //Stop us immediately going straight back in
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
