using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceEntrance : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            int entranceLayerMask = LayerMask.GetMask("Entrance");
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, entranceLayerMask);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    Debug.Log("¡˝ ¿‘¿Â");
                }
            }
        }
    }
}
