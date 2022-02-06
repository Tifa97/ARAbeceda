using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTracking : MonoBehaviour
{
    //[SerializeField]
    //private GameObject welcomePanel;

    //[SerializeField]
    //private Button dismissButton;

    [SerializeField]
    private Text imageTrackedText;

    [SerializeField]
    private GameObject[] arObjectsToPlace;

    [SerializeField]
    private Vector3 scaleFactor = new Vector3(0.1f, 0.1f, 0.1f);

    private ARTrackedImageManager m_TrackedImageManager;

    private ARAnchorManager m_anchorManager;

    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        //dismissButton.onClick.AddListener(Dismiss);
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        m_anchorManager = GetComponent<ARAnchorManager>();

        // setup all game objects in dictionary
        foreach (GameObject arObject in arObjectsToPlace)
        {
            GameObject newARObject = Instantiate(arObject, Vector3.zero, Quaternion.identity);
            newARObject.name = arObject.name;
            arObjects.Add(arObject.name, newARObject);
            arObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    //private void Dismiss() => welcomePanel.SetActive(false);

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            arObjects[trackedImage.name].SetActive(false);
        }
    }

    private void UpdateARImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // Display the name of the tracked image in the canvas
            imageTrackedText.text = trackedImage.referenceImage.name;

            // Assign and Place Game Object
            AssignGameObject(trackedImage);

            Debug.Log($"trackedImage.referenceImage.name: {trackedImage.referenceImage.name}");
        }
    }

    void AssignGameObject(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;
        Vector3 position = trackedImage.transform.position;

        if (arObjectsToPlace != null)
        {
            GameObject goARObject = arObjects[name];
            goARObject.transform.position = position;
            goARObject.transform.localScale = scaleFactor;
            goARObject.SetActive(true);
            
            foreach (GameObject go in arObjects.Values)
            {
                Debug.Log($"Go in arObjects.Values: {go.name}");
                if (go.name != name || trackedImage.trackingState == TrackingState.Limited || trackedImage.trackingState == TrackingState.None)
                {
                    go.SetActive(false);
                }
            }
        }
        else
        {
            _ShowAndroidToastMessage("VIDI ÐUKA");
        }
    }

    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
