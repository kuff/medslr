using System.Collections;
using UnityEngine;

public class OVRInitializer : MonoBehaviour
{
    public OVRInitializeEvent OnInitialized = new OVRInitializeEvent();
    
    [SerializeField] private Transform headTransform;
    [SerializeField] private OVRSkeleton leftHandSkeleton, rightHandSkeleton;

    protected IEnumerator Start()
    {
        // Busy-wait until both hands are initialized
        while (!leftHandSkeleton.IsInitialized && !rightHandSkeleton.IsInitialized)
            yield return null;
        
        // Invoke initialization event
        OnInitialized.Invoke(headTransform, leftHandSkeleton, rightHandSkeleton);
        Debug.Log("OVR initialization complete");
    }
}