using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolManager : SingletonMonoBehaviour<PoolManager>
{
    #region Tooltip
    [Tooltip("This array is populated with the prefabs that are gonna be added to the pool, also specify the number of gameobjects to be created for each prefab")]
    #endregion
    public Pool[] poolArray = null;
    private Transform objectPoolTransform;

    private Dictionary<int, Queue<Component>> poolDictionary = new();

    [System.Serializable]
    public struct Pool
    {
        [Tooltip("The pool size this object will have, the number of gameobjects created in the pool" +
            " will be defined by the pool size")]
        public int PoolSize;
        [Tooltip("The prefab to instantiate in the pool")]
        public GameObject prefabToUse;
        [Tooltip("Type exactly the type of component that the prefab is," +
            "otherwise it won't work.")]
        public string componentType;
    }

    protected override void Awake()
    {
        base.Awake();
        // This singleton gameobject will be the parent pool gameobject
        objectPoolTransform = this.gameObject.transform;

        for (int i = 0; i < poolArray.Length; i++)
        {
            CreatePool(poolArray[i].prefabToUse, poolArray[i].PoolSize, poolArray[i].componentType);
        }
    }

    //private void Start()
    //{
    //    //This singleton gameobject will be the parent pool gameobject
    //    objectPoolTransform = this.gameObject.transform;

    //    for (int i = 0; i < poolArray.Length; i++)
    //    {
    //        CreatePool(poolArray[i].prefabToUse, poolArray[i].PoolSize, poolArray[i].componentType);
    //    }
    //}

    /// <summary>
    /// Creates The Object Pool With The Specified Prefabs, The PoolSize And the Type Of Component For Each One
    /// </summary>
    private void CreatePool(GameObject prefabToUse, int poolSize, string typeOfComponent)
    {
        int poolKey = prefabToUse.GetInstanceID();
        string prefabName = prefabToUse.name; //Gets the name of the prefab

        GameObject parentGameObject = new GameObject(prefabName + " Anchor"); //Creates the parent gameobject to attached the child gameobjects to
        parentGameObject.transform.SetParent(objectPoolTransform);

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<Component>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefabToUse, parentGameObject.transform) as GameObject;

                newObject.SetActive(false);

                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(typeOfComponent)));
            }
        }
    }

    /// <summary>
    /// Reuses A Game Object Contained In The Pool.
    /// </summary>
    /// <param name="prefabToUse">The Prefab Containing The Component.</param>
    /// <param name="position">The World Position Where The Gameobject Should Appear When Enabled.</param>
    /// <param name="rotation">Should Be Set If The Component Needs A Rotation.</param>
    /// <returns></returns>
    public Component ReuseComponent(GameObject prefabToUse, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefabToUse.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            //Get the object from the pool queue
            Component componentToReuse = GetComponentFromPool(poolKey);

            ResetObject(position, rotation, componentToReuse, prefabToUse);

            return componentToReuse;
        }
        else
        {
            Debug.Log("No Object Pool For " + prefabToUse);
            return null;
        }
    }

    /// <summary>
    /// Gets A Gameobject From The Pool using The 'poolKey'
    /// </summary>
    private Component GetComponentFromPool(int poolKey)
    {
        Component componentToReuse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(componentToReuse);

        if (componentToReuse.gameObject.activeSelf == true)
        {
            componentToReuse.gameObject.SetActive(false);
        }

        return componentToReuse;
    }

    /// <summary>
    /// Resets The GameObject
    /// </summary>
    private void ResetObject(Vector3 position, Quaternion rotation, Component componentToReuse, GameObject prefabToUse)
    {
        componentToReuse.transform.position = position;
        componentToReuse.transform.rotation = rotation;
        componentToReuse.gameObject.transform.localScale = prefabToUse.transform.localScale;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(poolArray), poolArray);
    }
#endif
    #endregion
}