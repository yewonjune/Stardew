using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopTrigger : MonoBehaviour
{
    public ShopController shopController;
    public ShopCatalog shopCatalog;

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (shopController == null) return;
        if (shopController.IsOpen) return;

        shopController.OpenShop(shopCatalog);
    }
}
