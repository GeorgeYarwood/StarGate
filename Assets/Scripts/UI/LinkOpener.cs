using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkOpener : MonoBehaviour
{
    [SerializeField] string linkToOpen;
    
    public void OpenLink()
    {
        if(linkToOpen == string.Empty)
        {
            return;
        }
        Application.OpenURL(linkToOpen);
    }
}
