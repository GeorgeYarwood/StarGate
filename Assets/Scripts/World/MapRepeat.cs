using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class MapRepeat : MonoBehaviour
{
    [SerializeField] Animator animator;

    const string ANIMATOR_TRIGGER = "MapRepeat";

    static MapRepeat instance;
    public static MapRepeat Instance
    {
        get { return instance; }
    }

    void OnEnable()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void TriggerAnimation()
    {
        animator.SetTrigger(ANIMATOR_TRIGGER);
    }
}
