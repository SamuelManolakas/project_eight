using UnityEngine;

/// <summary>
/// Pre-hashed animator parameter names. Passing a string to SetFloat/SetBool hashes it on every
/// call; these are hashed once. Names must match the parameters in the animator controllers.
/// </summary>
public static class AnimatorParams
{
    public static readonly int Speed   = Animator.StringToHash("speed");
    public static readonly int StrafeX = Animator.StringToHash("strafeX");
    public static readonly int StrafeZ = Animator.StringToHash("strafeZ");
}
