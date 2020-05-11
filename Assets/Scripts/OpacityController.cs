using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpacityController : MonoBehaviour
{
    public Text text;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpacityValueChanged(float val) {
        Debug.Log(val);
        text.text = val.ToString("0.00");
        Color color = spriteRenderer.color;
        color.a = val;
        spriteRenderer.color = color;
    }
}
