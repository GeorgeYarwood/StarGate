using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum Effects
{
    PULSE_CHROMATIC_ABERRATION,
    PULSE_BLOOM,
}

public class VFXManager : MonoBehaviour
{
    const float ANIMATION_SPEED = 0.025f;

    static VFXManager instance;
    public static VFXManager Instance
    {
        get { return instance; }
    }

    [SerializeField] Volume vfxVolume;

    public void PlayEffect(Effects ToPlay)
    {
        switch (ToPlay)
        {
            case Effects.PULSE_BLOOM:
               //Todo implement this if I ever need it
                break;
            case Effects.PULSE_CHROMATIC_ABERRATION:
                if (vfxVolume.profile.components.Find(x => x.GetType() == typeof(ChromaticAberration)))
                {
                    StartCoroutine(LerpValues(vfxVolume.profile.components.Find(x => x.GetType() == typeof(ChromaticAberration))));
                }
                break;
        }
    }

    IEnumerator LerpValues(VolumeComponent ToEdit)
    {
        float T = 0.0f;
        if (ToEdit as ChromaticAberration)
        {
            float Max = (ToEdit as ChromaticAberration).intensity.max;
            float Min = (ToEdit as ChromaticAberration).intensity.value;
            (ToEdit as ChromaticAberration).intensity.value = Min;
            while ((ToEdit as ChromaticAberration).intensity.value < Max)
            {
                (ToEdit as ChromaticAberration).intensity.value = Mathf.Lerp(Min, Max, T);
                T += ANIMATION_SPEED;
                yield return null;
            }
            T = 0.0f;
            while ((ToEdit as ChromaticAberration).intensity.value > Min)
            {
                (ToEdit as ChromaticAberration).intensity.value = Mathf.Lerp(Max, Min, T);
                T += ANIMATION_SPEED;
                yield return null;
            }
        }
    }

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


}
