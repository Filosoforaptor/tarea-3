using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    private static PoolManager _instance;
    public static PoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("PoolManager is NULL.");
            }
            return _instance;
        }
    }

    [SerializeField]
    private Transform _container;
    [SerializeField]
    private int _initialPoolSize = 10; // Default initial pool size

    private Dictionary<GameObject, List<GameObject>> pools;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            pools = new Dictionary<GameObject, List<GameObject>>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreatePool(GameObject prefab, int size)
    {
        if (pools.ContainsKey(prefab))
        {
            Debug.LogWarning($"Pool for {prefab.name} already exists.");
            return;
        }

        List<GameObject> objectList = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.name = $"{prefab.name}_{i}";
            obj.transform.SetParent(_container);
            obj.SetActive(false);
            objectList.Add(obj);
        }
        pools.Add(prefab, objectList);
        Debug.Log($"Pool created for {prefab.name} with {size} objects.");
    }

    public void CreatePool(GameObject prefab)
    {
        CreatePool(prefab, _initialPoolSize);
    }

    public GameObject GetObject(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            Debug.LogError($"No pool found for {prefab.name}.");
            return null;
        }

        List<GameObject> objectList = pools[prefab];
        for (int i = 0; i < objectList.Count; i++)
        {
            if (!objectList[i].activeInHierarchy)
            {
                objectList[i].SetActive(true);
                return objectList[i];
            }
        }

        // If no inactive objects are found, expand the pool or return null
        // For now, let's log a warning and return null.
        // Optionally, you can expand the pool here:
        // GameObject obj = Instantiate(prefab);
        // obj.name = $"{prefab.name}_{objectList.Count}";
        // obj.transform.SetParent(_container);
        // objectList.Add(obj);
        // pools[prefab] = objectList; // Update the list in the dictionary
        // obj.SetActive(true);
        // return obj;

        Debug.LogWarning($"No inactive objects available in pool for {prefab.name}. Consider expanding the pool.");
        return null;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        // Optional: Reset object state here if needed
        // e.g., obj.transform.position = Vector3.zero;
        // obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public bool IsPoolCreated(GameObject prefabKey)
    {
        if (prefabKey == null) return false;
        return pools.ContainsKey(prefabKey);
    }
}
