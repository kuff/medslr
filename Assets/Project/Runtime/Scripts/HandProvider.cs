using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HandProvider : MonoBehaviour
{
    private Transform _headTransform;
    private OVRSkeleton _leftHand;
    private OVRSkeleton _rightHand;
    
    private OVRSkeleton _activeHand;

    public void SeedHands(Transform headTransform, OVRSkeleton leftHandSkeleton, OVRSkeleton rightHandSkeleton)
    {
        _headTransform = headTransform;
        _leftHand = leftHandSkeleton;
        _rightHand = rightHandSkeleton;
#if UNITY_EDITOR
        Debug.Log($"HandProvider was seeded with hands; left: {_leftHand.name}, right: {_rightHand.name}");
#endif
    }

    public void UseHand(OVRHand.Hand hand)
    {
        _activeHand = hand switch
        {
            OVRHand.Hand.HandLeft => _leftHand,
            OVRHand.Hand.HandRight => _rightHand,
            _ => null
        };
    }

    public void UseRightHand()
    {
        UseHand(OVRHand.Hand.HandRight);
    }

    public void UseLeftHand()
    {
        UseHand(OVRHand.Hand.HandLeft);
    }

    public ref readonly OVRSkeleton GetActiveHand()
    {
        return ref _activeHand;
    }

    public bool isHandRaised(OVRHand.Hand hand)
    {
        //if (_headTransform == null) return false;

        var handSkeleton = hand switch
        {
            OVRHand.Hand.HandLeft => _leftHand,
            OVRHand.Hand.HandRight => _rightHand,
            _ => null
        };

        // Check position of hand
        var handTransform = handSkeleton.transform;
        var delta = handTransform.position - _headTransform.position;
        Debug.Log($"delta.normalized.z: {delta.normalized.z}");
        if (delta.normalized.z < 0.1f) return false;

        // Check position of fingers
        var totalError = 0f;
        for (var i = 0; i < handSkeleton.Bones.Count; i++)
        {
            var b = handSkeleton.Bones[i];

            totalError += Mathf.Abs(b.Transform.position.x - TargetVectors.gate[i, 0]);
            totalError += Mathf.Abs(b.Transform.position.y - TargetVectors.gate[i, 1]);
            totalError += Mathf.Abs(b.Transform.position.z - TargetVectors.gate[i, 2]);

            Debug.Log($"totalError: {totalError}");
            if (totalError > 1f) return false;
        }

        // Both tests have passed
        return true;
    }
}
