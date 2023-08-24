using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigationItem : MonoBehaviour
{
    [SerializeField] Image highlightImage;
    [SerializeField] Selectable toInteractWith;
    public Selectable ToInteractWith
    {
        get { return toInteractWith; }
    }

    public void SetHighlight(bool Setting)
    {
        highlightImage.gameObject.SetActive(Setting);
    }
}
