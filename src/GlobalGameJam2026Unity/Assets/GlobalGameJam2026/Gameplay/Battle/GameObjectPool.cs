using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
internal class GameObjectPool<T>
  where T : Component
{
    [SerializeField] private T _template;
    [SerializeField] private bool _reuseTemplate = true;

    private List<T> _active = new();
    private List<T> _inactive = new();
    private bool _isInitialised;

    public IEnumerable<T> Active
    {
        get
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                yield return _active[i];
            }
        }
    }

    private bool IsTemplateInScene => _template.gameObject.scene.buildIndex != -1;

    public void Initialise(Transform parent = null)
    {
        if (_isInitialised) return;

        if (IsTemplateInScene)
        {
            _template.gameObject.SetActive(false);
            _template.gameObject.hideFlags = HideFlags.HideInHierarchy;

            if (_reuseTemplate)
            {
                _inactive.Add(_template);
            }
        }

        _isInitialised = true;
    }

    public void Prewarm(int amount, Transform parent = null)
    {
        if (!_isInitialised)
        {
            Initialise(parent);
        }

        for (int i = 0; i < amount; i++)
        {
            ExpandPool(parent);
        }
    }

    public T Grab(Transform parent = null)
    {
        if (!_isInitialised)
        {
            Initialise(parent);
        }

        if (_inactive.Count == 0)
        {
            ExpandPool(parent);
        }

        int popIndex = _inactive.Count - 1;
        var item = _inactive[popIndex];

        _inactive.RemoveAt(popIndex);
        _active.Add(item);

        item.gameObject.SetActive(true);
        item.gameObject.hideFlags = HideFlags.None;

        if (item.transform.parent != parent)
        {
            item.transform.SetParent(parent);
        }
        else
        {
            item.transform.SetAsLastSibling();
        }

        return item;
    }

    public void ReturnAll()
    {
        if (!_isInitialised)
        {
            Initialise(null);
        }

        foreach (var item in _active)
        {
            item.gameObject.SetActive(false);
            item.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
        _inactive.AddRange(_active);
        _active.Clear();
    }

    public void Return(T item)
    {
        if (!TryReturn(item))
        {
            if (_inactive.Contains(item))
            {
                Debug.LogError($"A GameObject is being returned to a pool whilst it's already returned.");
            }
            else
            {
                Debug.LogError($"A GameObject is being returned to a pool that it doesn't belong to.");
            }
            return;
        }
    }

    public bool TryReturn(T item)
    {
        if (!_isInitialised)
        {
            Initialise(null);
        }

        bool removed = _active.Remove(item);
        if (removed)
        {
            _inactive.Add(item);
            item.gameObject.SetActive(false);
            item.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
        return removed;
    }

    private T ExpandPool(Transform parent)
    {
        var clone = UnityEngine.Object.Instantiate(_template.gameObject, parent);
        var cloneComponent = clone.GetComponent<T>();

        _inactive.Add(cloneComponent);

        return cloneComponent;
    }
}