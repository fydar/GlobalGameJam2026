using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OwnershipUtilities
{
    private static readonly Dictionary<int, Host> handleToOwner = new();

    static OwnershipUtilities()
    {
        SceneManager.sceneUnloaded += OnBeforeSceneUnloaded;
    }

    public static void ClaimOwnership(Host owner, Scene scene)
    {
        handleToOwner[scene.handle] = owner;
    }

    public static Host GetOwner(Scene scene)
    {
        if (handleToOwner.TryGetValue(scene.handle, out var sceneOwner))
        {
            return sceneOwner;
        }
        throw new KeyNotFoundException($"The given scene '{scene.name}' ({scene.handle}) does not have an owner.");
    }

    public static bool IsOwned(Scene scene)
    {
        return handleToOwner.ContainsKey(scene.handle);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    internal static void Main()
    {
        handleToOwner.Clear();
    }

    private static void OnBeforeSceneUnloaded(Scene scene)
    {
        handleToOwner.Remove(scene.handle);
    }
}
