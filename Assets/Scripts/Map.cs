using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] private List<GameObject> mapThemes;

    public void ActivateMapTheme(MapTheme theme)
    {
        for (int i = 0; i < mapThemes.Count; i++)
        {
            mapThemes[i].SetActive(false);
        }
        mapThemes[(int)theme].SetActive(false);
    }
}
