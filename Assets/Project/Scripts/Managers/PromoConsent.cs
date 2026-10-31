using System.Collections;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

// Google's consent flow (UMP). It runs before the ads SDK starts: a player in a region that needs
// consent (EEA, UK, Switzerland) sees Google's form the first time, everyone else passes straight
// through. The ads SDK only starts once CanRequestAds says the player's answer allows it.
//
// UMP callbacks are not promised to arrive on the main thread, so they only set flags here and the
// coroutine that started them reads those flags on the main thread.
public class PromoConsent
{
    // The update call is a network request. Past this the game goes on with last launch's answer, so a
    // dead connection never holds the ads (or the player) on the splash.
    const float UpdateTimeoutSeconds = 10f;

    volatile bool callbackDone;
    volatile string callbackError;

    /// <summary>True when the stored consent answer lets the game request ads.</summary>
    public bool CanRequestAds => ConsentInformation.CanRequestAds();

    /// <summary>True when the player must be offered a way to change their answer (the Options row).</summary>
    public bool PrivacyOptionsRequired =>
        ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

    /// <summary>
    /// Refreshes the consent status and shows Google's form if this player still has to answer it.
    /// Never fails the game: any error is logged and the last stored answer decides.
    /// </summary>
    public IEnumerator Gather()
    {
        callbackDone = false;
        callbackError = null;

        ConsentInformation.Update(new ConsentRequestParameters(), error =>
        {
            callbackError = error?.Message;
            callbackDone = true;
        });

        float waited = 0f;
        while (!callbackDone && waited < UpdateTimeoutSeconds)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!callbackDone)
        {
            Debug.LogWarning("[Consent] Status update timed out; using the stored answer.");
            yield break;
        }

        if (callbackError != null)
        {
            Debug.LogWarning("[Consent] Status update failed: " + callbackError + "; using the stored answer.");
            yield break;
        }

        // The form waits for the player, so there is no timeout here.
        callbackDone = false;
        ConsentForm.LoadAndShowConsentFormIfRequired(error =>
        {
            callbackError = error?.Message;
            callbackDone = true;
        });

        while (!callbackDone)
        {
            yield return null;
        }

        if (callbackError != null)
        {
            Debug.LogWarning("[Consent] Form failed: " + callbackError);
        }

        Debug.Log("[Consent] Done. canRequestAds=" + CanRequestAds + " privacyOptions=" + PrivacyOptionsRequired);
    }

    /// <summary>Opens Google's privacy options form so the player can change their consent answer.</summary>
    public void ShowPrivacyOptions()
    {
        ConsentForm.ShowPrivacyOptionsForm(error =>
        {
            if (error != null)
            {
                Debug.LogWarning("[Consent] Privacy options form failed: " + error.Message);
            }
        });
    }
}
