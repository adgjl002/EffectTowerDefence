using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ResourceManager {

    public static ResourceManager Instance { get { return AppManager.Instance.resourceManager; } }

    private Dictionary<string, GameObject> prefabs;
    public Dictionary<string, GameObject>.KeyCollection GetPrefabKeyCollection()
    {
        if(prefabs != null)
        {
            return prefabs.Keys;
        }
        return null;
    }

    private Dictionary<string, AudioClip> audioClips;
    private Dictionary<string, Sprite> sprites;
    private Dictionary<string, Material> materials;

    public void Initialize()
    {
        prefabs = new Dictionary<string, GameObject>();
        audioClips = new Dictionary<string, AudioClip>();
        sprites = new Dictionary<string, Sprite>();
        materials = new Dictionary<string, Material>();

        var gobjCollections = Resources.LoadAll<GameObjectCollection>(string.Empty);
        foreach (var c in gobjCollections)
        {
            foreach (var d in c.datas)
            {
                prefabs.Add(d.key, d.value);
            }
        }

        var audioClipCollection = Resources.LoadAll<GenericCollection>("AudioClipCollection");
        foreach (var c in audioClipCollection)
        {
            foreach (var d in c.datas)
            {
                audioClips.Add(d.key, (AudioClip)d.res);
            }
        }

        var spriteColleciton = Resources.LoadAll<SpriteCollection>(string.Empty);
        foreach (var c in spriteColleciton)
        {
            foreach (var d in c.datas)
            {
                sprites.Add(d.key, d.sprite);
            }
        }
        
        var materialColleciton = Resources.LoadAll<MaterialCollection>(string.Empty);
        foreach (var m in materialColleciton)
        {
            foreach (var d in m.datas)
            {
                materials.Add(d.key, d.res);
            }
        }

        Debug.Log(GetLog());
    }
    
    public string GetLog()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Format("#### ResourceManager Log ####"));

        sb.AppendLine(string.Format("## Prefab List"));
        if (prefabs != null)
        {
            foreach (var p in prefabs)
            {
                sb.AppendLine(string.Format("# Registed Key : {0}", p.Key));
            }
        }

        sb.AppendLine(string.Format("## AudioClip List"));
        if (audioClips != null)
        {
            foreach (var a in audioClips)
            {
                sb.AppendLine(string.Format("# Registed Key : {0}", a.Key));
            }
        }

        sb.AppendLine(string.Format("## Sprite List"));
        if (sprites != null)
        {
            foreach (var s in sprites)
            {
                sb.AppendLine(string.Format("# Registed Key : {0}", s.Key));
            }
        }

        sb.AppendLine(string.Format("## Material List"));
        if (materials != null)
        {
            foreach (var m in materials)
            {
                sb.AppendLine(string.Format("# Registed Key : {0}", m.Key));
            }
        }

        sb.AppendLine("############");
        return sb.ToString();
    }

    public void Release()
    {
        if (materials != null) materials.Clear();
        materials = null;

        if (sprites != null) sprites.Clear();
        sprites = null;

        if (audioClips != null) audioClips.Clear();
        audioClips = null;
        
        if (prefabs != null) prefabs.Clear();
        prefabs = null;
    }

    public bool TryGetPrefab(string key, out GameObject prefab)
    {
        return prefabs.TryGetValue(key, out prefab);
    }

    public bool TryGetAudioClip(string key, out AudioClip audioClip)
    {
        return audioClips.TryGetValue(key, out audioClip);
    }

    public static AudioClip GetAudioClip(string key)
    {
        AudioClip clip;
        if(Instance.audioClips.TryGetValue(key, out clip))
        {
            return clip;
        }
        return null;
    }

    public bool TryGetSprite(string key, out Sprite sprite)
    {
        return sprites.TryGetValue(key, out sprite);
    }

    public Sprite GetSprite(string key)
    {
        Sprite sprite;
        if(sprites.TryGetValue(key, out sprite))
        {
            return sprite;
        }
        return null;
    }

    public bool TryGetMaterial(string key, out Material mat)
    {
        return materials.TryGetValue(key, out mat);
    }

    public Material GetMaterial(string key)
    {
        Material mat;
        if(materials.TryGetValue(key, out mat))
        {
            return mat;
        }
        return null;
    }

    public static Material GetSpriteGrayScaleMaterial()
    {
        return Instance.GetMaterial("SpriteGrayScale");
    }

    public static Material GetSpriteSpecularEffectMaterial()
    {
        return Instance.GetMaterial("SpriteSpecEffect");
    }

    public static Material GetSkillMapLineRenderMaterial()
    {
        return Instance.GetMaterial("SkillMapLineRender");
    }
}
