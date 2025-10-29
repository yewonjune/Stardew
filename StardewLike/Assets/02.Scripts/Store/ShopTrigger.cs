using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public ShopController shopController;
    public ShopCatalog shopCatalog;

    void OnMouseDown()
    {
        shopController.OpenShop();
    }
}
