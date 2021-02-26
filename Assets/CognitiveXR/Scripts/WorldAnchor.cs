using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAnchor : MonoBehaviour
{
    private bool IsVisible = true;
    public List<GameObject> ObjectsToHide;
    

    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;

        foreach (GameObject gameObject in ObjectsToHide)
        {
            gameObject.SetActive(IsVisible);
        }
    }
}
