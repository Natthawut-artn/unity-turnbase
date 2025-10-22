using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class UnmuteOnPlay
{
    static UnmuteOnPlay()
    {
        EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                AudioListener.pause = false;
                EditorUtility.audioMasterMute = false;
            }
        };
    }
}
