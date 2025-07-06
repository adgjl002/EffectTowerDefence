using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMaster {

    public static SpawnMaster Instance { get { return AppManager.Instance.spawnMaster; } }

    private ObjectPool objectPool;

    public void Initialize()
    {
        objectPool = new ObjectPool();

        var keyCollection = ResourceManager.Instance.GetPrefabKeyCollection();
        foreach (var key in keyCollection)
        {
            objectPool.Regist(key, 0);
        }
    }

    public void Release()
    {
        if(objectPool != null)
        {
            objectPool = null;
        }
    }

    public static bool TrySpawnMonoBehaviour<T>(string key, Vector3 pos, Quaternion rot, out T obj) where T : MonoBehaviour
    {
        var gobj = Instance.objectPool.Instantiate(key, pos, rot);
        if (gobj == null)
        {
            obj = null;
            return false;
        }

        obj = gobj.GetComponent<T>();
        return (obj != null);
    }
    
    public static bool TrySpawnObject<T>(string key, Vector3 pos, Quaternion rot, out T obj) where T : Object
    {
        var gobj = Instance.objectPool.Instantiate(key, pos, rot);
        if (gobj == null)
        {
            obj = null;
            return false;
        }

        obj = gobj.GetComponent<T>();
        return (obj != null);
    }

    public static bool TrySpawnUI<T>(string key, Transform parentTf, out T obj) where T : UIBase
    {
        var gobj = Instance.objectPool.Instantiate(key, Vector3.zero, Quaternion.identity);
        if (gobj == null)
        {
            obj = null;
            return false;
        }

        obj = gobj.GetComponent<T>();
        if(obj == null) return false;

        obj.transform.SetParent(parentTf);
        obj.rtf.localScale = Vector3.one;
        obj.rtf.rotation = Quaternion.identity;
        return true;
    }

    public static bool TrySpawnGridBlock<T>(string key, Vector3 pos, Quaternion rot, out T obj) where T : GridBlock
    {
        var gobj = Instance.objectPool.Instantiate(key, pos, rot);
        if (gobj == null)
        {
            obj = null;
            return false;
        }

        obj = gobj.GetComponent<T>();
        if(obj != null)
        {
            obj.Initialize();
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool TrySpawnIngameObject<T>(string key, Vector3 pos, Quaternion rot, out T obj) where T : IngameObject
    {
        var gobj = Instance.objectPool.Instantiate(key, pos, rot);
        if (gobj == null)
        {
            obj = null;
            return false;
        }

        obj = gobj.GetComponent<T>();
        obj.Initialize();

        return (obj != null);
    }

    public static bool TrySpawnFx<T>(string key, Vector3 pos, Quaternion rot, out T obj) where T : FxBase
    {
        var gobj = Instance.objectPool.Instantiate(key, pos, rot);
        if (gobj == null)
        {
            obj = null;
            return false;
        }

        obj = gobj.GetComponent<T>();
        return (obj != null);
    }

    public static void Destroy(GameObject obj, string key)
    {
        Instance.objectPool.Destroy(obj, key);
    }
}
