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

        ui.Setup(icon, name, amount);
        StartCoroutine(RemoveWhenDestroyed(itemId, ui));
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

    IEnumerator RemoveWhenDestroyed(string itemId, PickupToastItemUI ui)
    {
        while (ui != null) yield return null;
        if (active.ContainsKey(itemId)) active.Remove(itemId);
    }
}
