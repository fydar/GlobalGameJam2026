using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StashingSceneManager
{
    private class SceneStash
    {
        public Scene scene;
        public GameObject[] rootGameObjects;
        public bool[] defaultActivities;
        public bool[] defaultIsHiddenInHierarchy;
    }

    private readonly Host host;
    private readonly List<SceneStash> stashes;
    private readonly List<int> scenesToStashWhenLoadedHandles;

    public StashingSceneManager(Host host)
    {
        stashes = new List<SceneStash>();
        scenesToStashWhenLoadedHandles = new List<int>();

        SceneManager.sceneLoaded += OnAfterSceneLoadedCallback;
        SceneManager.sceneUnloaded += TryForgetStashForScene;
        this.host = host;
    }

    public LoadSceneAsyncOperation UseBootSceneAsync()
    {
        int sceneCount = SceneManager.sceneCount;
        var entryScenes = new List<Scene>(sceneCount);
        Scene? bootScene = null;
        for (int i = sceneCount - 1; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneAt(i);
            entryScenes.Add(scene);
        }

        for (int i = entryScenes.Count - 1; i >= 0; i--)
        {
            var scene = entryScenes[i];
            if (!OwnershipUtilities.IsOwned(scene))
            {
                OwnershipUtilities.ClaimOwnership(host, scene);

                if (bootScene == null && scene.buildIndex == 0)
                {
                    bootScene = scene;
                }
                else
                {
                    StashScene(scene);
                }
            }
            else
            {
                entryScenes.RemoveAt(i);
            }
        }

        if (bootScene == null)
        {
            var operation = SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
            bootScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            OwnershipUtilities.ClaimOwnership(host, bootScene.Value);
        }

        if (bootScene.Value.isLoaded)
        {
            SceneManager.SetActiveScene(bootScene.Value);
#if UNITY_EDITOR
            EditorSceneManager.MoveSceneBefore(bootScene.Value, SceneManager.GetSceneAt(0));
#endif
            return LoadSceneAsyncOperation.FromResult(bootScene.Value);
        }
        else
        {
            var result = LoadSceneAsyncOperation.FromIncompleteResult(bootScene.Value, out var complete);
            SceneManager.sceneLoaded += BootSceneCheckCallback;
            return result;

            void BootSceneCheckCallback(Scene scene, LoadSceneMode loadSceneMode)
            {
                if (scene != bootScene)
                {
                    return;
                }

                SceneManager.sceneLoaded -= BootSceneCheckCallback;
                SceneManager.SetActiveScene(bootScene.Value);
#if UNITY_EDITOR
                EditorSceneManager.MoveSceneBefore(bootScene.Value, SceneManager.GetSceneAt(0));
#endif
                complete.Invoke();
            }
        }
    }

    public LoadSceneAsyncOperation LoadSceneAsync(int sceneBuildIndex)
    {
        foreach (var stash in stashes)
        {
            if (stash.scene.buildIndex == sceneBuildIndex)
            {
                UnstashScene(stash);
                return LoadSceneAsyncOperation.FromResult(stash.scene);
            }
        }

        var operation = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);

        var sceneToLoad = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        OwnershipUtilities.ClaimOwnership(host, sceneToLoad);

        return LoadSceneAsyncOperation.FromSceneLoad(sceneToLoad, operation);
    }

    public LoadSceneAsyncOperation LoadSceneAsync(string sceneName)
    {
        foreach (var stash in stashes)
        {
            if (stash.scene.name == sceneName)
            {
                UnstashScene(stash);
                return LoadSceneAsyncOperation.FromResult(stash.scene);
            }
        }

        var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        var sceneToLoad = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        OwnershipUtilities.ClaimOwnership(host, sceneToLoad);

        return LoadSceneAsyncOperation.FromSceneLoad(sceneToLoad, operation);
    }

    public AsyncOperation UnloadSceneAsync(Scene scene)
    {
        TryForgetStashForScene(scene);
        return SceneManager.UnloadSceneAsync(scene);
    }

    private void StashScene(Scene scene)
    {
        if (IsStashed(scene))
        {
            string sceneDisplayName = !string.IsNullOrEmpty(scene.name)
                ? scene.name
                : "Untitled";

            throw new InvalidOperationException($"Cannot stash scene \"{sceneDisplayName}\" as it is already stashed.");
        }

        if (!scene.isLoaded)
        {
            scenesToStashWhenLoadedHandles.Add(scene.handle);
        }
        else
        {
#if UNITY_EDITOR
            var lastScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            EditorSceneManager.MoveSceneAfter(scene, lastScene);
#endif

            var rootObjects = scene.GetRootGameObjects();

            bool[] defaultActivities = new bool[rootObjects.Length];
            bool[] defaultHideFlags = new bool[rootObjects.Length];
            for (int i = 0; i < rootObjects.Length; i++)
            {
                var rootObject = rootObjects[i];

                defaultActivities[i] = rootObject.activeSelf;
                rootObject.SetActive(false);

                defaultHideFlags[i] = rootObject.hideFlags.HasFlag(HideFlags.HideInHierarchy);
                rootObject.hideFlags |= HideFlags.HideInHierarchy;
            }

            stashes.Add(new SceneStash()
            {
                scene = scene,
                rootGameObjects = rootObjects,
                defaultActivities = defaultActivities,
                defaultIsHiddenInHierarchy = defaultHideFlags
            });
        }
    }

    private void UnstashScene(SceneStash stash)
    {
        for (int i = 0; i < stash.rootGameObjects.Length; i++)
        {
            var sceneObject = stash.rootGameObjects[i];

            sceneObject.SetActive(stash.defaultActivities[i]);

            if (!stash.defaultIsHiddenInHierarchy[i])
            {
                sceneObject.hideFlags &= ~HideFlags.HideInHierarchy;
            }
        }

        stashes.Remove(stash);
    }

    private void OnAfterSceneLoadedCallback(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scenesToStashWhenLoadedHandles.Contains(scene.handle))
        {
            scenesToStashWhenLoadedHandles.Remove(scene.handle);
            StashScene(scene);
        }
    }

    private void TryForgetStashForScene(Scene scene)
    {
        for (int i = stashes.Count - 1; i >= 0; i--)
        {
            var stash = stashes[i];
            if (stash.scene == scene)
            {
                stashes.RemoveAt(i);
            }
        }
    }

    private bool IsStashed(Scene scene)
    {
        foreach (var stash in stashes)
        {
            if (stash.scene == scene)
            {
                return true;
            }
        }
        return false;
    }
}
