using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveHandProvider : MonoBehaviour
{
    private OVRPlugin.Hand _activeHand;

    private void Start()
    {
        _activeHand = OVRPlugin.Hand.None;
    }

    public void SetActiveHand(OVRPlugin.Hand activeHand)
    {
        _activeHand = activeHand;
    }

    public OVRPlugin.Hand GetActiveHand()
    {
        return _activeHand;
    }
}
