using Vuforia;
using UnityEngine;
using UnityEngine.UI;

public class SimpleCloudHandler : MonoBehaviour, IObjectRecoEventHandler
{
    public string targetName;

    public Text statusText;

    CloudRecoBehaviour m_CloudRecoBehaviour;
    bool m_IsScanning = false;
    string m_TargetMetadata = "";
    TargetFinder m_TargetFinder;
    ObjectTracker m_ObjectTracker;

    [Tooltip("Here you can set the ImageTargetBehaviour from the scene that will be used to " +
             "augment new cloud reco search results.")]
    public ImageTargetBehaviour m_ImageTargetBehaviour;

    // Use this for initialization
    void Start() {
        // register this event handler at the cloud reco behaviour
        m_CloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();

        if (m_CloudRecoBehaviour) {
            m_CloudRecoBehaviour.RegisterEventHandler(this);
        }
    }

    void Update() {
        statusText.text = m_IsScanning ? "AR scanning" : "AR not scanning";
    }

    public void OnInitError(TargetFinder.InitState initError) {
        Debug.Log("Cloud Reco init error " + initError.ToString());
    }
    public void OnInitialized() {
        Debug.Log("Cloud Reco initialized successfully.");

        m_ObjectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        m_TargetFinder = m_ObjectTracker.GetTargetFinder<ImageTargetFinder>();
    }

    public void OnInitialized(TargetFinder targetFinder) {
        Debug.Log("Cloud Reco initialized successfully.");

        m_ObjectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        m_TargetFinder = targetFinder;
    }

    public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult) {
        TargetFinder.CloudRecoSearchResult cloudRecoSearchResult =
            (TargetFinder.CloudRecoSearchResult)targetSearchResult;

        // This code demonstrates how to reuse an ImageTargetBehaviour for new search results
        // and modifying it according to the metadata. Depending on your application, it can
        // make more sense to duplicate the ImageTargetBehaviour using Instantiate() or to
        // create a new ImageTargetBehaviour for each new result. Vuforia will return a new
        // object with the right script automatically if you use:
        // TargetFinder.EnableTracking(TargetSearchResult result, string gameObjectName)

        // do something with the target metadata
        m_TargetMetadata = cloudRecoSearchResult.MetaData;

        // Check if the metadata isn't null
        if (m_TargetMetadata == null) {
            Debug.Log("Target metadata not available.");
        }
        else {
            Debug.Log("MetaData: " + m_TargetMetadata);
            Debug.Log("TargetName: " + cloudRecoSearchResult.TargetName);
            Debug.Log("Pointer: " + cloudRecoSearchResult.TargetSearchResultPtr);
            Debug.Log("TrackingRating: " + cloudRecoSearchResult.TrackingRating);
            Debug.Log("UniqueTargetId: " + cloudRecoSearchResult.UniqueTargetId);
        }

        if (cloudRecoSearchResult.TargetName == targetName) {
            // Changing CloudRecoBehaviour.CloudRecoEnabled to false will call TargetFinder.Stop()
            // and also call all registered ICloudRecoEventHandler.OnStateChanged() with false.
            m_CloudRecoBehaviour.CloudRecoEnabled = false;

            // Clear any existing trackables
            m_TargetFinder.ClearTrackables(false);

            // Enable the new result with the same ImageTargetBehaviour:
            m_TargetFinder.EnableTracking(cloudRecoSearchResult, m_ImageTargetBehaviour.gameObject);

            // Pass the TargetSearchResult to the Trackable Event Handler for processing
            m_ImageTargetBehaviour.gameObject.SendMessage("TargetCreated", cloudRecoSearchResult, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void OnStateChanged(bool scanning) {
        m_IsScanning = scanning;

        Debug.Log("State changed: " + scanning);

        // Changing CloudRecoBehaviour.CloudRecoEnabled to false will call:
        // 1. TargetFinder.Stop()
        // 2. All registered ICloudRecoEventHandler.OnStateChanged() with false.

        // Changing CloudRecoBehaviour.CloudRecoEnabled to true will call:
        // 1. TargetFinder.StartRecognition()
        // 2. All registered ICloudRecoEventHandler.OnStateChanged() with true.

        //if (scanning) {
        //    // clear all known trackables
        //    var tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        //    tracker.GetTargetFinder<ImageTargetFinder>().ClearTrackables(false);
        //}
    }

    public void OnUpdateError(TargetFinder.UpdateState updateError) {
        Debug.Log("Cloud Reco update error " + updateError.ToString());
    }

    public void StartReco(string name) {
        targetName = name;
        m_CloudRecoBehaviour.enabled = true;
        m_CloudRecoBehaviour.CloudRecoEnabled = true;
    }
}
