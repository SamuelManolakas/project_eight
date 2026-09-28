using UnityEngine;

public static class Smoothing
{
    /// <summary>
    /// Frame-rate independent replacement for <c>rate * Time.deltaTime</c> as the t of a Lerp/Slerp.
    /// "rate * deltaTime" smooths faster on slow machines (and snaps once it passes 1);
    /// this gives the same result per second at any frame rate.
    /// </summary>
    public static float Factor(float rate) => 1f - Mathf.Exp(-rate * Time.deltaTime);
}
