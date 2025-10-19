using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceExit : MonoBehaviour
{
    public Transform player;

    public Vector3 playerOutdoorPosition;

    [SerializeField] string targetCamKey = "Farm";

    // Start is called before the first frame update
    void Start()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            ExitPlace();
    }

    void ExitPlace()
    {
        if (CameraManager.Instance == null || player == null)
        {
            return;
        }

        var rb = player.GetComponent<Rigidbody2D>();
        var col = player.GetComponent<Collider2D>();
        var mover = player.GetComponent<PlayerMovement>();

        if (mover) mover.enabled = false;
        if (rb) { rb.velocity = Vector2.zero; rb.isKinematic = true; }
        if (col) col.enabled = false;

        FadeManager.Instance.FadeOutIn(() =>
        {
            CameraManager.Instance.SwitchTo(targetCamKey);
            player.position = playerOutdoorPosition;

            StartCoroutine(RestoreNextFrame(rb, col, mover));
        });
    }
    IEnumerator RestoreNextFrame(Rigidbody2D rb, Collider2D col, PlayerMovement mover)
    {
        yield return null;

        if (rb) rb.isKinematic = false;
        if (col) col.enabled = true;
        if (mover) mover.enabled = true;
    }
}
