using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class OVRInitializeEvent : UnityEvent<Transform, OVRSkeleton, OVRSkeleton> { }