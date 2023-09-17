using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleTouchUI : MonoBehaviour
{
    [SerializeField] GameObject touchUi;
    void Start()
    {
        if (touchUi)
        {
#if FOR_MOBILE
            touchUi.SetActive(true);
#else
            touchUi.SetActive(false);
#endif
        }
    }
}
