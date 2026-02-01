using UnityEngine;

public static class BootstrapperEntrypoint
{
    private static Host host;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Main()
    {
        var hostGameObject = new GameObject("Game");
        // host = hostGameObject.AddComponent<Host>();
        Object.DontDestroyOnLoad(hostGameObject);

#if UNITY_EDITOR
        Application.quitting += ApplicationQuitting;
#endif

        // host.Initialize();
    }

#if UNITY_EDITOR
    private static void ApplicationQuitting()
    {
        if (host != null && host.gameObject != null)
        {
            Object.DestroyImmediate(host.gameObject);
        }
    }
#endif
}
