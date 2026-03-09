using UnityEngine;

public abstract class GameMode : MonoBehaviour
{
    // Base class for game modes
    public abstract void Initialize();
    public abstract void OnMatchStart();
    public abstract void OnMatchEnd();
}