using UnityEngine;

public class RenderStepHandStateProvider : MonoBehaviour, IHandStateProvider
{
    public void GetHandState(OVRPlugin.Hand hand, ref OVRPlugin.HandState hs)
    {
        OVRPlugin.GetHandState(OVRPlugin.Step.Render, hand,  ref hs);
    }
}
