using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState
{
    // public Combatant[] playerTeam;
}

public class Host : MonoBehaviour
{
    private StashingSceneManager stashingSceneManager;
    private LoadSceneAsyncOperation bootLoad;

    public void Initialize()
    {
        stashingSceneManager = new StashingSceneManager(this);
        bootLoad = stashingSceneManager.UseBootSceneAsync();
    }

    void Start()
    {
        StartCoroutine(GameFlow());
    }

    IEnumerator GameFlow()
    {
        yield return bootLoad;

        // var overworldLoad = stashingSceneManager.LoadSceneAsync(1);
        // yield return overworldLoad;
        // 
        // var overworld = GetRootComponent<Overworld>(overworldLoad.Scene);
        // SceneManager.SetActiveScene(overworldLoad.Scene);
        // 
        // yield return overworld.Run();


        var battleLoad = stashingSceneManager.LoadSceneAsync(2);

        var battle = GetRootComponent<BattleController>(battleLoad.Scene);
        SceneManager.SetActiveScene(battleLoad.Scene);

        yield return battle.RunBattle();
    }

    private T GetRootComponent<T>(Scene scene)
    {
        var rootObjects = scene.GetRootGameObjects();
        foreach (var rootObject in rootObjects)
        {
            if (rootObject.TryGetComponent<T>(out var component))
            {
                return component;
            }
        }
        throw new InvalidOperationException($"Unable to find root object of type {typeof(T).Name}");
    }
}
