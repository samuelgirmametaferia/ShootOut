using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ObjectSaveData
{
    public string objectName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public float health;
}

public class SaveSystem : MonoBehaviour
{
    public List<GameObject> staticObjects = new List<GameObject>(); // Static objects in the scene
    public List<string> tags = new List<string>(); // Tags to identify dynamic objects

    private string saveFilePath;

    private void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
    }

    // Save function
    public void SaveScene()
    {
        List<ObjectSaveData> saveData = new List<ObjectSaveData>();

        // Save static objects (transform and health data)
        foreach (var obj in staticObjects)
        {
            ObjectSaveData data = new ObjectSaveData
            {
                objectName = obj.name,
                position = obj.transform.position,
                rotation = obj.transform.rotation,
                scale = obj.transform.localScale,
                health = obj.GetComponent<HealthSystem>() ? obj.GetComponent<HealthSystem>().CurrentHealth : -1f // Save health if exists
            };

            saveData.Add(data);
        }

        // Save dynamic objects
        foreach (var tag in tags)
        {
            GameObject[] dynamicObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (var dynamicObj in dynamicObjects)
            {
                // Dynamic object data (position, prefab to spawn later, etc.)
                ObjectSaveData data = new ObjectSaveData
                {
                    objectName = dynamicObj.name,
                    position = dynamicObj.transform.position,
                    rotation = dynamicObj.transform.rotation,
                    scale = dynamicObj.transform.localScale,
                    health = dynamicObj.GetComponent<HealthSystem>() ? dynamicObj.GetComponent<HealthSystem>().CurrentHealth : -1f
                };

                saveData.Add(data);
            }
        }

        // Save the data as JSON
        string json = JsonUtility.ToJson(new SaveDataWrapper { objectData = saveData });
        File.WriteAllText(saveFilePath, json);
    }

    // Load function
    public void LoadScene()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveDataWrapper loadedData = JsonUtility.FromJson<SaveDataWrapper>(json);

            foreach (var data in loadedData.objectData)
            {
                GameObject obj = GameObject.Find(data.objectName);

                if (obj != null)
                {
                    // Override transform values
                    obj.transform.position = data.position;
                    obj.transform.rotation = data.rotation;
                    obj.transform.localScale = data.scale;

                    // Override health if HealthSystem is present
                    HealthSystem healthSystem = obj.GetComponent<HealthSystem>();
                    if (healthSystem != null && data.health >= 0f)
                    {
                        healthSystem.ResetHealth();
                        healthSystem.Heal(data.health - healthSystem.CurrentHealth); // Restore health to saved value
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class SaveDataWrapper
{
    public List<ObjectSaveData> objectData;
}
