using UnityEngine;

public class WizardController : MonoBehaviour
{
    public WizardSuccessEvent wizardSuccessEvent = new WizardSuccessEvent();
    public WizardFailureEvent wizardFailureEvent = new WizardFailureEvent();
    
    private void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            // Input succeeded
            wizardSuccessEvent.Invoke();
            HandLogger.LogWizardDecision(1);
        }
        else if (Input.GetKeyDown("d"))
        {
            // Input failed
            wizardFailureEvent.Invoke();
            HandLogger.LogWizardDecision(0);
        }
    }
}
