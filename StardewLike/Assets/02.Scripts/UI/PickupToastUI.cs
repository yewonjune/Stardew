using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupToastUI : MonoBehaviour
{
    public Transform root;
    public PickupToastItemUI toastPrefab;

    public int maxVisible = 6;
    public bool newestOnTop = true;

    readonly Dictionary<string, PickupToastItemUI> active = new();

    public void Show(string itemId, Sprite icon, string name, int amount)
    {
        if (string.IsNullOrEmpty(itemId)) itemId = name;

        if(active.TryGetValue(itemId, out var existing) && existing != null)
        {
            existing.AddAmount(amount);
            if (newestOnTop) existing.transform.SetAsFirstSibling();
            return;
        }

        var ui = Instantiate(toastPrefab, root);
        if(newestOnTop) ui.transform.SetAsFirstSibling();

        active[itemId] = ui;
        ui.Setup(icon, name, amount);

        ui.onDestroyed = () =>
        {
            if (active.TryGetValue(itemId, out var cur) && cur == ui)
                active.Remove(itemId);
        };

        Trim();
    }
    
    void Trim()
    {
        while (root.childCount > maxVisible)
        {
            int idx = newestOnTop ? root.childCount - 1 : 0;
            var child = root.GetChild(idx);
            Destroy(child.gameObject);
        }
    }
}
