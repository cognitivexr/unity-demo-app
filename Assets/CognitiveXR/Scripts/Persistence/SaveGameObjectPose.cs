using System;
using System.Collections;
using System.IO;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.WorldLocking.Core;
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

        IEnumerator Start()
        {
            yield return new WaitForSeconds(1.0f);
            Load();
        }

        public void Load()
        {
            LoadFromCache(out var json);

            if (!string.IsNullOrEmpty(json))
            {
                SerializedPose data = JsonUtility.FromJson<SerializedPose>(json);
                //Transform cachedTransform = transform;
                //cachedTransform.position = data.position;
                //cachedTransform.rotation = data.rotation;
                
                SpacePinOrientable spacePinOrientable = GetComponent<SpacePinOrientable>();
                if (spacePinOrientable)
                {
                    //spacePinOrientable.SetLockedPose(new Pose(data.position, data.rotation));
                    spacePinOrientable.SetLockedPose(new Pose(data.position, data.rotation));
                }
            }
        }

        public void Save()
        {
            Transform cachedTransform = transform;
            Pose pose = cachedTransform.GetGlobalPose();
            
            // transform to sponek
            SpacePinOrientable spacePinOrientable = GetComponent<SpacePinOrientable>();
            if (spacePinOrientable)
            {
                spacePinOrientable.SetFrozenPose(pose);
            }
            
            
            pose = cachedTransform.GetGlobalPose();
            
            WorldLockingManager wltMgr = WorldLockingManager.GetInstance();
            Pose savePose = wltMgr.LockedFromFrozen.Multiply(pose);
            
            SerializedPose data = new SerializedPose
            {
                position = savePose.position,
                rotation = savePose.rotation
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
