using UnityEngine;

public class HandProvider : MonoBehaviour
{
    private OVRSkeleton _leftHand;
    private OVRSkeleton _rightHand;
    
    private OVRSkeleton _activeHand;

    public void SeedHands(Transform _, OVRSkeleton leftHandSkeleton, OVRSkeleton rightHandSkeleton)
    {
        // We don't care about headTransform in this case...
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
}
