using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecificTypeUI : MonoBehaviour {
    
    [SerializeField] GameObject tileIcon;
    [SerializeField] Transform iconHolder;

    public void SetSpecific(List<MatchType> specificTypes) {
        gameObject.SetActive(true);
        if (iconHolder.childCount < specificTypes.Count) {
            GameObject go;
            for (int i = iconHolder.childCount; i < specificTypes.Count; ++i) {
                go = Instantiate(tileIcon);
                go.transform.SetParent(iconHolder);
            }
        }
        Image im;
        for(int i = 0; i < iconHolder.childCount; ++i) {
            if (specificTypes.Count <= i) {
                iconHolder.GetChild(i).gameObject.SetActive(false);
                continue;
            }
            iconHolder.GetChild(i).gameObject.SetActive(true);
            im = iconHolder.GetChild(i).GetComponent<Image>();
            if (specificTypes[i] == MatchType.None)
                im.enabled = false;
            else {
                im.enabled = true;
                im.color =  ConstantHolder.placeHolderGraphics[specificTypes[i]];
            }
        }
    }
}
