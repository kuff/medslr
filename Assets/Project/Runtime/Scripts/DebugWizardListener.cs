using UnityEngine;

public class DebugWizardListener : MonoBehaviour
{
    public void OnWizardEvent(bool wasSuccess)
    {
        Debug.Log("Wizard event: " + wasSuccess);
    }
}
