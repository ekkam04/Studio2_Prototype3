using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    Material mat;
    float distance;
    [Range(0, 0.5f)] public float speed = 0.2f;

    void Start()
    {
        mat = GetComponent<SpriteRenderer>().material;
    }

    void Update()
    {
        distance += speed * Time.deltaTime;
        mat.SetTextureOffset("_MainTex", new Vector2(distance, 0));
    }
}
