using UnityEngine;
using System.Collections.Generic;

public class BaseObjectPool<KEY> {
    protected BaseObjectPool() {
    }

    protected virtual void DestroyObject(object obj) {
    }

    protected virtual void OnAddToPool(object obj) {
    }

    protected virtual void OnRemoveFromPool(object obj) {
    }

    public static BaseObjectPool<KEY> instance {
        get {
            return s_inst ?? (s_inst = new BaseObjectPool<KEY>());
        }
    }
    protected static BaseObjectPool<KEY> s_inst;

    /// <summary>
    /// 给某个subPool填充count个type类型的对象
    /// </summary>
    /// <param name="subPool"></param>
    /// <param name="type"></param>
    /// <param name="count"></param>
    protected void AllocSubPool(SubPool subPool, KEY type, int count) {
        for (int i = 0; i < count; ++i) {
            object obj = subPool.create(type);
            OnAddToPool(obj);
            subPool.pool.Push(obj);
        }
    }

    public const int CONST_DEFAULT_CAPACITY = 10;

    public delegate object CreateFunction(KEY type);
    public delegate void ResetFunction(object obj);
    public delegate void DestroyFunction(object obj);
    public void Alloc(KEY type, int capacity, CreateFunction create, ResetFunction reset = null, DestroyFunction destroy = null) {
        Debug.Assert(capacity > 0);
        Debug.Assert(create != null);

        SubPool subPool;
        if (m_pool.TryGetValue(type, out subPool)) {
            if (capacity > subPool.pool.Count) {
                subPool.capacity = capacity;
            }
            //Debug.AssertFormat(create == null && reset == null, "create and reset function are always ignored when realloc the subpool");
            capacity -= subPool.pool.Count;
        } else {
            subPool = new SubPool();
            subPool.pool = new Stack<object>();
            subPool.capacity = capacity;
            subPool.create = create;
            subPool.reset = reset;
            subPool.destroy = destroy;
            m_pool.Add(type, subPool);
        }

        AllocSubPool(subPool, type, capacity);
    }

    public void Clear(KEY type) {
        SubPool subPool;
        if (m_pool.TryGetValue(type, out subPool)) {
            while (subPool.pool.Count > 0) {
                DestroyObject(subPool.pool.Pop());
            }
            subPool.capacity = 0;
            m_pool.Remove(type);
        }
    }

    public void Clear() {
        foreach (KEY type in m_pool.Keys) {
            Clear(type);
        }
    }

    public TYPE Instantiate<TYPE>(KEY type) {
        SubPool subPool;
        if (m_pool.TryGetValue(type, out subPool)) {
            if (m_enabled) {
                if (subPool.pool.Count == 0) {
                    if (!m_autoReAlloc) {
                        return (TYPE)subPool.create(type);
                    }
                    AllocSubPool(subPool, type, subPool.capacity);
                }
            } else {
                return (TYPE)subPool.create(type);
            }
        } else {
            Debug.AssertFormat(false, "type({0}) is not registered", type);
            return default(TYPE);
        }

        object obj = subPool.pool.Pop();
        OnRemoveFromPool(obj);
        if (subPool.reset != null) {
            subPool.reset(obj);
        }
        return (TYPE)obj;
    }

    public void Destroy(KEY type, object obj) {
        if (!m_enabled) {
            DestroyObject(obj);
            return;
        }

        SubPool subPool;
        if (m_pool.TryGetValue(type, out subPool)) {
            if (subPool.destroy != null) {
                subPool.destroy(obj);
            }
            OnAddToPool(obj);
            subPool.pool.Push(obj);
        } else {
            DestroyObject(obj);
        }
    }

    public bool enabled {
        get {
            return m_enabled;
        }

        set {
            m_enabled = value;
        }
    }

    public bool autoReAlloc {
        get {
            return m_autoReAlloc;
        }

        set {
            m_autoReAlloc = value;
        }
    }

    protected class SubPool {
        public Stack<object> pool;
        public int capacity;
        public CreateFunction create;
        public ResetFunction reset;
        public DestroyFunction destroy;
    }
    protected Dictionary<KEY, SubPool> m_pool = new Dictionary<KEY, SubPool>();
    protected bool m_enabled = true;
    protected bool m_autoReAlloc = false;
}

public class ObjectPool<TYPE> : BaseObjectPool<System.Type>
    where TYPE : new() {
    public ObjectPool() {
    }

    public static new ObjectPool<TYPE> instance {
        get {
            return s_inst ?? (s_inst = new ObjectPool<TYPE>());
        }
    }
    protected new static ObjectPool<TYPE> s_inst;

    protected static TYPE CreateObjectFunction() {
        return new TYPE();
    }

    public new delegate TYPE CreateFunction();
    public new delegate void ResetFunction(TYPE obj);
    public new delegate void DestroyFunction(TYPE obj);

    public void Alloc(int capacity, CreateFunction create, ResetFunction reset = null, DestroyFunction destroy = null) {
        BaseObjectPool<System.Type>.CreateFunction baseCreate = delegate (System.Type type) {
            return create();
        };
        BaseObjectPool<System.Type>.ResetFunction baseReset;
        if (reset != null) {
            baseReset = delegate (object obj) {
                reset((TYPE)obj);
            };
        } else {
            baseReset = null;
        }
        BaseObjectPool<System.Type>.DestroyFunction baseDestroy;
        if (destroy != null) {
            baseDestroy = delegate (object obj) {
                destroy((TYPE)obj);
            };
        } else {
            baseDestroy = null;
        }
        Alloc(typeof(TYPE), capacity, baseCreate, baseReset, baseDestroy);
        m_subPool = m_pool[typeof(TYPE)];
    }

    public void Alloc(int capacity, ResetFunction reset = null, DestroyFunction destroy = null) {
        Alloc(capacity, CreateObjectFunction, reset, destroy);
    }

    public new void Clear() {
        if (m_subPool != null) {
            while (m_subPool.pool.Count > 0) {
                DestroyObject(m_subPool.pool.Pop());
            }
            m_subPool.capacity = 0;
            m_pool.Remove(typeof(TYPE));
            m_subPool = null;
        }
    }

    public TYPE Instantiate() {
        if (m_subPool != null) {
            if (m_enabled) {
                if (m_subPool.pool.Count == 0) {
                    if (!m_autoReAlloc) {
                        return (TYPE)m_subPool.create(typeof(TYPE));
                    }
                    AllocSubPool(m_subPool, m_type, m_subPool.capacity);
                }
            } else {
                return (TYPE)m_subPool.create(m_type);
            }
        } else {
            Debug.AssertFormat(false, "type({0}) is not registered", m_type);
            return default(TYPE);
        }

        object obj = m_subPool.pool.Pop();
        OnRemoveFromPool(obj);
        if (m_subPool.reset != null) {
            m_subPool.reset(obj);
        }
        return (TYPE)obj;
    }

    public void Destroy(object obj) {
        if (!m_enabled) {
            DestroyObject(obj);
            return;
        }

        if (m_subPool != null) {
            if (m_subPool.destroy != null) {
                m_subPool.destroy(obj);
            }
            OnAddToPool(obj);
            m_subPool.pool.Push(obj);
        } else {
            DestroyObject(obj);
        }
    }

    protected readonly System.Type m_type = typeof(TYPE);
    protected SubPool m_subPool;
}

public class MutiObjectPool : BaseObjectPool<System.Type> {
    public static new MutiObjectPool instance {
        get {
            return s_inst ?? (s_inst = new MutiObjectPool());
        }
    }
    protected new static MutiObjectPool s_inst;

    protected static TYPE CreateObjectFunction<TYPE>() where TYPE : new() {
        return new TYPE();
    }

    public delegate TYPE CreateFunction<TYPE>() where TYPE : new();
    public delegate void ResetFunction<TYPE>(TYPE obj) where TYPE : new();
    public delegate void DestroyFunction<TYPE>(TYPE obj) where TYPE : new();
    public void Alloc<TYPE>(int capacity, CreateFunction<TYPE> create, ResetFunction<TYPE> reset = null, DestroyFunction<TYPE> destroy = null)
        where TYPE : new() {
        CreateFunction baseCreate = delegate (System.Type type) {
            return create();
        };
        ResetFunction baseReset;
        if (reset != null) {
            baseReset = delegate (object obj) {
                reset((TYPE)obj);
            };
        } else {
            baseReset = null;
        }
        DestroyFunction baseDestroy;
        if (destroy != null) {
            baseDestroy = delegate (object obj) {
                destroy((TYPE)obj);
            };
        } else {
            baseDestroy = null;
        }
        Alloc(typeof(TYPE), capacity, baseCreate, baseReset, baseDestroy);
    }

    public void Alloc<TYPE>(int capacity, ResetFunction<TYPE> reset = null, DestroyFunction<TYPE> destroy = null)
        where TYPE : new() {
        Alloc(capacity, CreateObjectFunction<TYPE>, reset, destroy);
    }

    public TYPE Instantiate<TYPE>()
         where TYPE : new() {
        return Instantiate<TYPE>(typeof(TYPE));
    }

    public void Destroy<TYPE>(object obj)
         where TYPE : new() {
        Destroy(typeof(TYPE), obj);
    }

    public void Clear<TYPE>()
         where TYPE : new() {
        Clear(typeof(TYPE));
    }
}

public class GameObjectPool : BaseObjectPool<GameObject> {
    protected GameObjectPool() {
        GameObject root = new GameObject("GameObjectPool");
        m_root = root.transform;
#if !UNITY_EDITOR
        Object.DontDestroyOnLoad(root);
#endif
        root.SetActive(false);
    }

    public static new GameObjectPool instance {
        get {
            return s_inst ?? (s_inst = new GameObjectPool());
        }
    }
    protected new static GameObjectPool s_inst;

    protected static GameObject CreateGameObjectFunction(GameObject prefab) {
        return Object.Instantiate(prefab);
    }

    public new delegate GameObject CreateFunction(GameObject prefab);
    public new delegate void ResetFunction(GameObject obj);
    public new delegate void DestroyFunction(GameObject obj);
    public void Alloc(GameObject prefab, int capacity, CreateFunction create, ResetFunction reset = null, DestroyFunction destroy = null) {
        BaseObjectPool<GameObject>.CreateFunction baseCreate = delegate (GameObject type) {
            return create(type);
        };
        BaseObjectPool<GameObject>.ResetFunction baseReset;
        if (reset != null) {
            baseReset = delegate (object obj) {
                reset((GameObject)obj);
            };
        } else {
            baseReset = null;
        }
        BaseObjectPool<GameObject>.DestroyFunction baseDestroy;
        if (destroy != null) {
            baseDestroy = delegate (object obj) {
                destroy((GameObject)obj);
            };
        } else {
            baseDestroy = null;
        }
        Alloc(prefab, capacity, baseCreate, baseReset, baseDestroy);
    }

    public void Alloc(GameObject prefab, int capacity, ResetFunction reset = null, DestroyFunction destroy = null) {
        Alloc(prefab, capacity, CreateGameObjectFunction, reset, destroy);
    }

    protected override void DestroyObject(object obj) {
        Object.Destroy((GameObject)obj);
    }

    protected override void OnAddToPool(object obj) {
        ((GameObject)obj).transform.SetParent(m_root);
    }

    protected override void OnRemoveFromPool(object obj) {
        ((GameObject)obj).transform.SetParent(null);
    }

    public GameObject Instantiate(GameObject type) {
        return Instantiate<GameObject>(type);
    }

    Transform m_root;
}
