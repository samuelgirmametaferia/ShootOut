using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PMS : MonoBehaviour
{
    public static PMS Instance { get; private set; }

    [System.Serializable]
    public class ParticlePoolEntry
    {
        public GameObject prefab;
        public int initialPoolSize = 10;
        public int maxPoolSize = 50;
        [Range(0, 100)]
        public int probabilityWeight = 50;
    }

    [SerializeField] private List<ParticlePoolEntry> particlePrefabs = new List<ParticlePoolEntry>();
    [SerializeField] private Transform particleParent;
    
    private Dictionary<GameObject, Pool> particlePools = new Dictionary<GameObject, Pool>();
    private int totalWeights;

    private class Pool
    {
        private Queue<GameObject> available = new Queue<GameObject>();
        private List<GameObject> inUse = new List<GameObject>();
        private GameObject prefab;
        private Transform parent;
        private int maxSize;

        public Pool(GameObject prefab, int initialSize, int maxSize, Transform parent)
        {
            this.prefab = prefab;
            this.maxSize = maxSize;
            this.parent = parent;

            // Enforce GPU instancing
            var renderer = prefab.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material.enableInstancing = true;
            }

            for (int i = 0; i < initialSize; i++)
            {
                available.Enqueue(CreateInstance());
            }
        }

        private GameObject CreateInstance()
        {
            GameObject obj = Instantiate(prefab, parent);
            obj.SetActive(false);
            obj.AddComponent<ParticleReturnToPool>();
            return obj;
        }

        public GameObject Get()
        {
            if (available.Count == 0)
            {
                if (inUse.Count < maxSize)
                {
                    available.Enqueue(CreateInstance());
                }
                else
                {
                    // Recycle oldest particle
                    GameObject oldest = inUse[0];
                    inUse.RemoveAt(0);
                    oldest.SetActive(false);
                    available.Enqueue(oldest);
                }
            }

            GameObject instance = available.Dequeue();
            inUse.Add(instance);
            return instance;
        }

        public void Return(GameObject instance)
        {
            instance.SetActive(false);
            inUse.Remove(instance);
            available.Enqueue(instance);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        totalWeights = particlePrefabs.Sum(p => p.probabilityWeight);

        foreach (var entry in particlePrefabs)
        {
            if (entry.prefab == null) continue;

            var pool = new Pool(
                entry.prefab,
                entry.initialPoolSize,
                entry.maxPoolSize,
                particleParent
            );

            particlePools.Add(entry.prefab, pool);
        }
    }

    public void SpawnParticle(Vector3 position)
    {
        GameObject selectedPrefab = ChooseParticlePrefab();
        if (selectedPrefab == null) return;

        var pool = particlePools[selectedPrefab];
        GameObject particle = pool.Get();
        particle.transform.position = position;
        particle.SetActive(true);
        particle.GetComponent<ParticleSystem>().Play();
    }

    private GameObject ChooseParticlePrefab()
    {
        int randomWeight = Random.Range(0, totalWeights);
        int accumulatedWeight = 0;

        foreach (var entry in particlePrefabs)
        {
            accumulatedWeight += entry.probabilityWeight;
            if (randomWeight <= accumulatedWeight)
            {
                return entry.prefab;
            }
        }
        return null;
    }

    public void ReturnToPool(GameObject particle)
    {
        foreach (var poolEntry in particlePools)
        {
            if (IsInstanceOfPrefab(particle, poolEntry.Key))
            {
                poolEntry.Value.Return(particle);
                return;
            }
        }

        Destroy(particle);
    }

    private bool IsInstanceOfPrefab(GameObject instance, GameObject prefab)
    {
        return instance.name.StartsWith(prefab.name);
    }
}

public class ParticleReturnToPool : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void OnParticleSystemStopped()
    {
        PMS.Instance.ReturnToPool(gameObject);
    }
}