using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class InitializeEvent : UnityEvent<Transform, OVRSkeleton, OVRSkeleton> { }

public class OVRInitializer : MonoBehaviour
{
    public InitializeEvent OnInitialized = new InitializeEvent();
    
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