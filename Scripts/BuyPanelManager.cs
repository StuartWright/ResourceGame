using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class BuyPanelManager : MonoBehaviour
{    
    public enum PanelTypes
    {
        None,
        Town,
        Storage,
        Industry
    }
    public RectTransform Current, PanelToClose, Town, Storage, Indusrty;
    private PanelTypes panelType;
    public PanelTypes PanelType
    {
        get { return panelType; }
        set
        {
           if(PanelToClose != null)
            {
                RectTransform TurnOff = PanelToClose;
                PanelToClose.DOMoveX(40, .3f);
                Tween t = PanelToClose.DOSizeDelta(new Vector3(0, 100, 0), .3f).OnComplete(() => { TurnOff.gameObject.SetActive(false);});
            }
            panelType = value;
            if (panelType == PanelTypes.None) return;
            switch (PanelType)
            {
                case PanelTypes.Town:
                    Current = Town;
                    break;
                case PanelTypes.Storage:
                    Current = Storage;
                    break;
                case PanelTypes.Industry:
                    Current = Indusrty;
                    break;
            }
            Current.gameObject.SetActive(true);
            Current.DOMoveX(900, .3f);
            //Current.DOSizeDelta(new Vector3(500, 100, 0), .3f);
            Current.DOSizeDelta(new Vector3(2223, 360, 0), .3f);
            PanelToClose = Current;
        }
    }

    public void TownClicked()
    {
        if(panelType != PanelTypes.Town)
            PanelType = PanelTypes.Town;
    }
    public void StorageClicked()
    {
        if (panelType != PanelTypes.Storage)
            PanelType = PanelTypes.Storage;
    }
    public void IndustryClicked()
    {
        if (panelType != PanelTypes.Industry)
            PanelType = PanelTypes.Industry;
    }
    public void Expand()
    {
        Current.DOMoveX(900, .3f);
        Current.DOSizeDelta(new Vector3(500, 50, 0), .3f);
    }
    public void ClosePanel()
    {
        PanelType = PanelTypes.None;
        PanelToClose = null;
        if (Current != null)
        {
            Current.DOMoveX(40, .3f);
            Tween t = Current.DOSizeDelta(new Vector3(0, 100, 0), .3f).OnComplete(() => { GameManager.Instance.BuyPanel.transform.DOMoveY(-100, .3f); Current.gameObject.SetActive(false); Current = null; });
        }
        else
        {
            //Current.DOMoveX(40, .5f);
            GameManager.Instance.BuyPanel.transform.DOMoveY(-100, .3f);
        }
        GameManager.Instance.Close();
        GameManager.Instance.CanInteract = true;
    }
    
    public void Open()
    {
        GameManager.Instance.BuyPanel.transform.DOMoveY(220, .3f);
        GameManager.Instance.CanInteract = false;
    }
}
