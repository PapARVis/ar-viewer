using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class WebAPIManager : MonoBehaviour
{
    public string baseUrl = "";
    public GameObject img;
    public GameObject mask;
    public SimpleCloudHandler simpleCloudHandler;
    public QRCodeAndVuforia qRCodeAndVuforia;

    string cache = "";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetNewImage(string id) {
        Debug.Log("Get message: " + id);
        if (cache == id) {
            Debug.Log("cached");
            qRCodeAndVuforia.StopQRCodeScanning();
            return;
        }
        StartCoroutine(FetchImage(id));
    }

    IEnumerator FetchImage(string id) {
        string url = string.Format("{0}/n?id={1}", baseUrl, id);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
            Debug.LogError(www.error);
            qRCodeAndVuforia.ClearMessage(); // clear message in order to rescan
        }
        else {
            qRCodeAndVuforia.StopQRCodeScanning();
            // retrieve results as binary data
            byte[] results = www.downloadHandler.data;
            var tex = new Texture2D(1, 1);
            tex.LoadImage(results);
            Debug.Log(tex.width + " " + tex.height);
            img.GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
            var transparentTex = CreateTransparentTex(tex.width, tex.height);
            mask.GetComponent<SpriteMask>().sprite = Sprite.Create(transparentTex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
            simpleCloudHandler.StartReco(id);
            cache = id;
        }
    }

    private Texture2D CreateTransparentTex(int width, int height) {
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

        Color fillColor = Color.clear;
        Color[] fillPixels = new Color[tex.width * tex.height];

        for (int i = 0; i < fillPixels.Length; i++) {
            fillPixels[i] = fillColor;
        }

        tex.SetPixels(fillPixels);

        tex.Apply();
        return tex;
    }
}
