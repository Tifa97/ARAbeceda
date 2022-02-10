using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTracking : MonoBehaviour
{
    [SerializeField]
    private Button btnFinish;

    [SerializeField]
    private GameObject[] arObjectsToPlace;

    private ARTrackedImageManager m_TrackedImageManager;

    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>();

    float initialDistance;
    Vector3 initialScale;

    private TrackingState currentState;

    private GameObject spawnedObject;

    void Awake()
    {
        //dismissButton.onClick.AddListener(Dismiss);
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        // setup all game objects in dictionary
        foreach (GameObject arObject in arObjectsToPlace)
        {
            GameObject newARObject = Instantiate(arObject, Vector3.zero, Quaternion.identity);
            newARObject.name = arObject.name;
            arObjects.Add(arObject.name, newARObject);
            arObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.touchCount == 2)
        {
            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled ||
                touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
            {
                return;
            }

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                initialScale = spawnedObject.transform.localScale;
            }
            else
            {
                var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);

                if (Mathf.Approximately(initialDistance, 0))
                {
                    return;
                }

                var factor = currentDistance / initialDistance;
                spawnedObject.transform.localScale = initialScale * factor;
            }
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
            arObjects[trackedImage.referenceImage.name].SetActive(false);
        }
    }

    public void UpdateARImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != currentState)
        {
            currentState = trackedImage.trackingState;
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // Assign and Place Game Object
                AssignGameObject(trackedImage);

                if (btnFinish != null && !btnFinish.gameObject.activeInHierarchy)
                {
                    btnFinish.gameObject.SetActive(true);
                }
            }
        }
    }

    void AssignGameObject(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;
        Vector3 position = trackedImage.transform.position;
        Quaternion rotation = trackedImage.transform.rotation;

        if (arObjectsToPlace != null)
        {
            spawnedObject = arObjects[name];
            if (!spawnedObject.activeInHierarchy)
            {
                spawnedObject.transform.position = position;
                if (spawnedObject.gameObject.name != "K")
                {
                    spawnedObject.transform.rotation = Quaternion.Euler(rotation.x, rotation.y - 180, rotation.z);
                }
                spawnedObject.SetActive(true);
            }

            foreach (GameObject go in arObjects.Values)
            {
                if (go.name != name || trackedImage.trackingState == TrackingState.Limited || trackedImage.trackingState == TrackingState.None)
                {
                    go.SetActive(false);
                }
            }
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
