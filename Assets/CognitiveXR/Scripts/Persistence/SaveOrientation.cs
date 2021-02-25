using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SaveOrientation : MonoBehaviour
{

    [Serializable]
    public struct SerializedTransform
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    
    public string guid;
    
    private string filePath => Path.Combine(Application.persistentDataPath, guid);

    private void Start()
    {
        Load();
    }

    public void Load()
    {
        string json;
        LoadFromCache(out json);

        if (!string.IsNullOrEmpty(json))
        {
            SerializedTransform data = JsonUtility.FromJson<SerializedTransform>(json);
            transform.position = data.position;
            transform.rotation = data.rotation;   
        }
    }

    public void Save()
    {
        SerializedTransform data = new SerializedTransform
        {
            position = transform.position,
            rotation = transform.rotation
        };

        string json = JsonUtility.ToJson(data);
        SaveToCache(json);
    }
    
    void SaveToCache(string data)
    {
        try
        {
            File.WriteAllText(filePath, data);

        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to {filePath} with exception {e}");
            throw;
        }
    }

    void LoadFromCache(out string data)
    {
        try
        {            
            data = File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to read from {filePath} with exception {e}");
            data = string.Empty;
        }
    }
}
