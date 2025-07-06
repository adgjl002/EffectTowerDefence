 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ObjectPoolKey { }

public interface IObjectSpawner
{
    GameObject Instantiate(string key, Vector3 pos, Quaternion rot);
}

public class ObjectPool : IObjectSpawner
{
    Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();
    Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    
    public void Regist(string key, int size)
    {
        GameObject prefab = null;
        if(!ResourceManager.Instance.TryGetPrefab(key, out prefab))
        {
            Debug.LogErrorFormat("ResourceManager :: Prefab({0}) not fount.", key);
            return;
        }
        
        Queue<GameObject> stack = new Queue<GameObject>();
        for (int i = 0; i < size; i++)
        {
            var g = GameObject.Instantiate(prefab) as GameObject;
            g.SetActive(false);
            stack.Enqueue(g);
        }
        prefabs[key] = prefab;
        pool[key] = stack;
    }

    public void Regist(string key, int size, Vector3 pos, Quaternion rot)
    {
        GameObject prefab = null;
        if (!ResourceManager.Instance.TryGetPrefab(key, out prefab))
        {
            Debug.LogErrorFormat("ResourceManager :: Prefab({0}) not fount.", key);
            return;
        }

        Queue<GameObject> stack = new Queue<GameObject>();
        for (int i = 0; i < size; i++)
        {
            var g = GameObject.Instantiate(prefab, pos, rot) as GameObject;
            g.SetActive(false);
            stack.Enqueue(g);
        }
        prefabs[key] = prefab;
        pool[key] = stack;
    }
    
    public void Release(string key)
    {
        var queue = pool[key];
        while(queue.Count != 0)
        {
            GameObject.Destroy(queue.Dequeue());
        }
        pool.Remove(key);
    }

    //public static void Initialize()
    //{
    //    instance = null;
    //}
    
    public GameObject Instantiate(string key, Vector3 pos, Quaternion rot)
    {
        if (!pool.ContainsKey(key)) return null;
        if (pool.Count == 0) return null;

        if(pool[key].Count == 0)
        {
            GameObject obj = GameObject.Instantiate(prefabs[key], pos, rot) as GameObject;
            return obj;
        }
        else
        {
            GameObject value = pool[key].Dequeue();
            value.transform.position = pos;
            value.transform.rotation = rot;

            value.SetActive(true);
            return value;
        }
    }

    public void Destroy(GameObject obj, string key)
    {
        obj.SetActive(false);
        
        if (pool.ContainsKey(key))
        {
            pool[key].Enqueue(obj);
        }
        else
        {
            GameObject.Destroy(obj);
        }
    }

}
