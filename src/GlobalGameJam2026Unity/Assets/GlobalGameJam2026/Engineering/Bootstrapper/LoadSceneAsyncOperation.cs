using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneAsyncOperation : CustomYieldInstruction
{
    private readonly AsyncOperation sceneLoadOperation;

    private Action onCompleted;

    public bool IsComplete { get; private set; }

    public float Progress
    {
        get
        {
            if (sceneLoadOperation != null)
            {
                return sceneLoadOperation.progress;
            }

            return 1.0f;
        }
    }

    public Scene Scene { get; }

    public override bool keepWaiting => !IsComplete;

    public event Action OnCompleted
    {
        add
        {
            if (IsComplete)
            {
                value.Invoke();
            }
            else
            {
                onCompleted += value;
            }
        }
        remove => onCompleted -= value;
    }

    private LoadSceneAsyncOperation(Scene scene)
    {
        Scene = scene;
    }

    private LoadSceneAsyncOperation(
        Scene scene,
        AsyncOperation sceneLoadOperation)
    {
        Scene = scene;
        this.sceneLoadOperation = sceneLoadOperation;

        if (sceneLoadOperation == null)
        {
            IsComplete = true;
        }
        else if (sceneLoadOperation.isDone)
        {
            IsComplete = true;
        }
        else
        {
            sceneLoadOperation.completed += SceneLoadOperation_completed;
        }
    }

    private void SceneLoadOperation_completed(
        AsyncOperation obj)
    {
        onCompleted?.Invoke();
        IsComplete = true;
    }

    public static LoadSceneAsyncOperation FromSceneLoad(
        Scene scene,
        AsyncOperation asyncOperation)
    {
        return new LoadSceneAsyncOperation(scene, asyncOperation);
    }

    public static LoadSceneAsyncOperation FromResult(
        Scene scene)
    {
        return new LoadSceneAsyncOperation(scene, null)
        {
            IsComplete = true,
        };
    }

    public static LoadSceneAsyncOperation FromIncompleteResult(
        Scene scene,
        out Action action)
    {
        var result = new LoadSceneAsyncOperation(scene);
        action = () =>
        {
            result.SceneLoadOperation_completed(null);
        };
        return result;
    }
}
