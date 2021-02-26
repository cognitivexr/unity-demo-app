using System;
using System.IO;
using UnityEngine;

namespace CognitiveXR.Persistence
{
    public class SaveGameObjectPose : MonoBehaviour
    {

        [Serializable]
        public struct SerializedPose
        {
            public Vector3 position;
            public Quaternion rotation;
        }
    
        public string guid;
    
        private string FilePath => Path.Combine(Application.persistentDataPath, guid);

        private void Start()
        {
            Load();
        }

        public void Load()
        {
            LoadFromCache(out var json);

            if (!string.IsNullOrEmpty(json))
            {
                SerializedPose data = JsonUtility.FromJson<SerializedPose>(json);
                Transform cachedTransform = transform;
                cachedTransform.position = data.position;
                cachedTransform.rotation = data.rotation;   
            }
        }

        public void Save()
        {
            Transform cachedTransform = transform;
            SerializedPose data = new SerializedPose
            {
                position = cachedTransform.position,
                rotation = cachedTransform.rotation
            };

            string json = JsonUtility.ToJson(data);
            SaveToCache(json);
        }
    
        void SaveToCache(string data)
        {
            try
            {
                File.WriteAllText(FilePath, data);

            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {FilePath} with exception {e}");
                throw;
            }
        }

        void LoadFromCache(out string data)
        {
            try
            {            
                data = File.ReadAllText(FilePath);
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to read from {FilePath} with exception {e}");
                data = string.Empty;
            }
        }
    }
}
