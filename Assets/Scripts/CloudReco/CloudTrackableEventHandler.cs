/*===============================================================================
Copyright (c) 2015-2018 PTC Inc. All Rights Reserved.
 
Copyright (c) 2010-2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using System;
using System.Collections;
using UnityEngine;
using Vuforia;

public class CloudTrackableEventHandler : DefaultTrackableEventHandler
{
    #region PRIVATE_MEMBERS
    CloudRecoBehaviour m_CloudRecoBehaviour;
    Transform childTransform;
    Transform maskTransform;
    Boolean isFirst = false;
    #endregion // PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS
    protected override void Start()
    {
        base.Start();

        m_CloudRecoBehaviour = FindObjectOfType<CloudRecoBehaviour>();

        // wait for qr code (cannot add here because it seems to crash vuforia)
        m_CloudRecoBehaviour.enabled = false;

        childTransform = gameObject.transform.GetChild(0); // NOTE: assume only one child
        maskTransform = childTransform.Find("Mask");
    }
    #endregion // MONOBEHAVIOUR_METHODS


    #region BUTTON_METHODS
    public void OnReset()
    {
        Debug.Log("<color=blue>OnReset()</color>");

        OnTrackingLost();
        TrackerManager.Instance.GetTracker<ObjectTracker>().GetTargetFinder<ImageTargetFinder>().ClearTrackables(false);       
    }
    #endregion BUTTON_METHODS


    #region PUBLIC_METHODS
    /// <summary>
    /// Method called from the CloudRecoEventHandler
    /// when a new target is created
    /// </summary>
    public void TargetCreated(TargetFinder.CloudRecoSearchResult targetSearchResult)
    {
        Debug.Log("TargetCreated");

        String metaData = targetSearchResult.MetaData;
        MetaDataClass metaJson = JsonUtility.FromJson<MetaDataClass>(metaData);
        Debug.Log(metaJson.ToString());

        Vector2 trackedCloudImageWH = GameObject.Find("ImageTarget").GetComponent<ImageTargetBehaviour>().GetSize();
        Debug.Log(trackedCloudImageWH.ToString());

        // default pixel to unit is 100 in unity
        float ratio = 100f / Math.Max(trackedCloudImageWH[0], trackedCloudImageWH[1]);
        childTransform.localScale = new Vector3(ratio, ratio);
        childTransform.localPosition = new Vector3(-trackedCloudImageWH[0] * ratio / 100f / 2 - metaJson.hv * ratio / 100f, 0, -(trackedCloudImageWH[1]) * ratio / 100f / 2 - ((metaJson.vv - trackedCloudImageWH[1] + metaJson.height) * ratio / 100f));

        Debug.Log(maskTransform.gameObject.GetComponent<SpriteMask>().sprite.texture.texelSize);
        // NOTE: hardcode -100 because qrcode always 100 unit and on the middle right
        maskTransform.localScale = new Vector3((trackedCloudImageWH[0] - 100) / maskTransform.gameObject.GetComponent<SpriteMask>().sprite.texture.width, trackedCloudImageWH[1] / maskTransform.gameObject.GetComponent<SpriteMask>().sprite.texture.height);
        maskTransform.localPosition = new Vector3(metaJson.hv / 100f, metaJson.vv / 100f, 0);

        isFirst = true;
    }
    #endregion // PUBLIC_METHODS


    #region PROTECTED_METHODS
    
    protected override void OnTrackingFound()
    {
        Debug.Log("<color=blue>OnTrackingFound()</color>");

        base.OnTrackingFound();

        if (isFirst) {
            StartCoroutine(FadeIn(5));
        }

        if (m_CloudRecoBehaviour)
        {
            // Changing CloudRecoBehaviour.CloudRecoEnabled to false will call TargetFinder.Stop()
            // and also call all registered ICloudRecoEventHandler.OnStateChanged() with false.
            m_CloudRecoBehaviour.CloudRecoEnabled = false;
        }
        isFirst = false;
    }

    protected override void OnTrackingLost()
    {
        Debug.Log("<color=blue>OnTrackingLost()</color>");

        base.OnTrackingLost();

        if (m_CloudRecoBehaviour)
        {
            // Changing CloudRecoBehaviour.CloudRecoEnabled to true will call TargetFinder.StartRecognition()
            // and also call all registered ICloudRecoEventHandler.OnStateChanged() with true.
            //m_CloudRecoBehaviour.CloudRecoEnabled = true;
        }
    }

    #endregion // PROTECTED_METHODS

    IEnumerator FadeIn(float aTime) {
        Debug.Log("FadeIn");
        float posX = maskTransform.localPosition.x;
        float posY = maskTransform.localPosition.y;
        float scaleX = maskTransform.localScale.x;
        float scaleY = maskTransform.localScale.y;
        for (float t = 0.0f; t <= 1.0f; t += Time.deltaTime / aTime) {
            maskTransform.localPosition = new Vector3(Mathf.Lerp(posX, 0, t), Mathf.Lerp(posY, 0, t));
            maskTransform.localScale = new Vector3(Mathf.Lerp(scaleX, 1, t), Mathf.Lerp(scaleY, 1, t));
            yield return null;
        }
    }
}

internal class MetaDataClass
{
    public float height;
    public float width;
    public string v;
    public string h;
    public float vv;
    public float hv;
    public override string ToString() {
        return height + " " + width + " " + v + " " + h + " " + vv + " " + hv;
    }
}