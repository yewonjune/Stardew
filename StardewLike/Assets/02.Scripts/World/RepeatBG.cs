using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatBG : MonoBehaviour
{
    [SerializeField][Range(0.1f, 200f)] float speed = 3f;

    float repeatDistance;

    // Start is called before the first frame update
    void Start()
    {
        repeatDistance = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if(transform.position.x <= -repeatDistance)
        {
            transform.position += Vector3.right * repeatDistance * 2;
        }
    }
}
