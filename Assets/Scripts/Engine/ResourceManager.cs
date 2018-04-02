using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LitJson;
using System;


public class BaseResInfo {
    public class Point {
        public double x = 0.5;
        public double y = 0.5;
    }
    public Point pivot = new Point();

    public class Action {
        public int frames = 1;
        public double delay = 0.1;
        public int special = -1;
    }
    public Dictionary<string, Action> actions = new Dictionary<string, Action>();
    public List<string> frames;
}

public class UnitResInfo : BaseResInfo {
    public class Fire {
        public double x = 0.0;
        public double y = 0.0;
    }
    public Fire fire;

    public class Half {
        public double x = 0.2;
        public double y = 0.2;
    }
    public Half half;
}

public class ProjectileResInfo : BaseResInfo {
}

[Serializable]
public class AttackInfo {
    public static AttackInfo invalid {
        get {
            AttackInfo attack = new AttackInfo();
            attack.animations = null;
            return attack;
        }
    }

    public bool valid {
        get {
            return animations != null;
        }
    }

    public string name = "Normal Attack";
    public double cd;
    public string type;
    public double value;
    public double vrange = 0.15;
    public double range;
    public bool horizontal = false;
    public string[] animations = null;
    public string projectile;
}

[Serializable]
public class UnitInfo {
    public string model;
    public string name;
    public double maxHp;
    public double move = 0.2;
    public bool revivable;
    public AttackInfo attackSkill = AttackInfo.invalid;
    public bool isfixed = false;
}

[Serializable]
public class ProjectileInfo {
    public string model;
    public double move;
    public double height;
    public string fire;
    public int effect = 1;
}

[Serializable]
public class TankInfo {
    public string model;
    public string name;
    public double maxHp;
    public double move = 0.2;
    public bool revivable;
    public AttackInfo attackSkill = AttackInfo.invalid;
    public bool isfixed = false;
}

public class ResourceManager {
    public static ResourceManager instance {
        get {
            return s_instance ?? (s_instance = new ResourceManager());
        }
    }

    public UnitResInfo LoadUnitModel(string path) {
        return Load<UnitResInfo>(path);
    }

    public ProjectileResInfo LoadProjectileModel(string path) {
        return Load<ProjectileResInfo>(path);
    }

    /// <summary>
    /// 加载Units或Projectile模型资源(动画和帧)
    /// 因为模型下info结构不同，所以需要用TYPE来区分(UnitResInfo或ProjectileResInfo)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    TYPE Load<TYPE>(string path)
        where TYPE : BaseResInfo {
        BaseResInfo baseInfo;
        if (m_infos.TryGetValue(path, out baseInfo)) {
            return baseInfo as TYPE;
        }
			
        TextAsset res = Resources.Load<TextAsset>(string.Format("{0}/info", path));
        TYPE resInfo = JsonMapper.ToObject<TYPE>(res.text);
		Resources.UnloadAsset(res);
        m_infos.Add(path, resInfo);

        Vector2 pivot = new Vector2((float)resInfo.pivot.x, (float)resInfo.pivot.y);

        foreach (KeyValuePair<string, BaseResInfo.Action> action in resInfo.actions) {
            string actName = action.Key;
            BaseResInfo.Action actData = action.Value;
            int aframes = actData.frames;
            float adelay = (float)actData.delay;
            int aspecial = actData.special;

            Sprite[] sprites = new Sprite[aframes];
            for (int i = 0; i < aframes; ++i) {
                Texture2D texture = Resources.Load<Texture2D>(string.Format("{0}/{1}/{2:00}", path, actName, i));
                sprites[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot);
                Resources.UnloadAsset(texture);
                //sprites[i] = Resources.Load<Sprite>(string.Format("{0}/{1}/{2:00}", path, actName, i));
            }

            cca.Animation animation = new cca.Animation(sprites, adelay);
            if (aspecial >= 0) {
                animation.setFrameData(aspecial, "onSpecial");
            }
            m_animations.Add(string.Format("{0}/{1}", path, actName), animation);
        }

        foreach (string frame in resInfo.frames) {
            Texture2D texture = Resources.Load<Texture2D>(string.Format("{0}/{1}", path, frame));
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot);
			Resources.UnloadAsset(texture);
            m_frames.Add(string.Format("{0}/{1}", path, frame), sprite);
        }

        return resInfo;
    }

    // like "Malik/move"
    public cca.Animation GetAnimation(string name) {
        return m_animations[name];
    }

    // like "Malik/default"
    public Sprite GetFrame(string name) {
        return m_frames[name];
    }

    public BaseResInfo GetInfo(string name) {
        return m_infos[name];
    }

    void PrepareResource(string name, ObjectRenderer renderer, BaseResInfo resInfo) {
        foreach (var frame in resInfo.frames) {
            renderer.PrepareFrame(ObjectRenderer.NameToId(frame), string.Format("{0}/{1}", name, frame));
        }

        foreach (var action in resInfo.actions) {
            renderer.PrepareAnimation(ObjectRenderer.NameToId(action.Key), string.Format("{0}/{1}", name, action.Key));
        }
    }

    public void PrepareUnitResource(string name, UnitRenderer renderer) {
        var resInfo = GetInfo(name) as UnitResInfo;
        PrepareResource(name, renderer, resInfo);
        renderer.SetGeometry((float)resInfo.half.x, (float)resInfo.half.y, new Vector2((float)resInfo.fire.x, (float)resInfo.fire.y));
    }

    public void PrepareProjectileResource(string name, ProjectileRenderer renderer) {
        var resInfo = GetInfo(name) as ProjectileResInfo;
        PrepareResource(name, renderer, resInfo);
    }

    static ResourceManager s_instance;
    Dictionary<string, BaseResInfo> m_infos = new Dictionary<string, BaseResInfo>();
    Dictionary<string, cca.Animation> m_animations = new Dictionary<string, cca.Animation>();
    Dictionary<string, Sprite> m_frames = new Dictionary<string, Sprite>();

    /// <summary>
    /// 加载预设UnitsData中的数据
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public UnitInfo LoadUnit(string path) {
        if (path == null) {
            return null;
        }

        UnitInfo baseInfo;
        if (m_unitInfos.TryGetValue(path, out baseInfo)) {
            LoadUnitModel(baseInfo.model);
            return baseInfo;
        }

        TextAsset res = Resources.Load<TextAsset>(string.Format("{0}", path));
        if (res == null) {
            return null;
        }

        baseInfo = JsonMapper.ToObject<UnitInfo>(res.text);
		//Resources.UnloadAsset(res);
        if (baseInfo == null) {
            return null;
        }

        m_unitInfos.Add(path, baseInfo);
        LoadUnitModel(baseInfo.model);
        return baseInfo;
    }

    /// <summary>
    /// 加载预设UnitsData中的数据
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public UnitInfo LoadUnit(string path, string data) {
        if (path == null) {
            return null;
        }

        // 如果该路径已经存在对应映射，直接返回
        UnitInfo baseInfo;
        if (path.Length > 0 && m_unitInfos.TryGetValue(path, out baseInfo)) {
            LoadUnitModel(baseInfo.model);
            return baseInfo;
        }

        // 从数据源获取对象
        baseInfo = JsonMapper.ToObject<UnitInfo>(data);
        if (baseInfo == null) {
            return null;
        }

        // 建立对应映射
        if (path.Length > 0) {
            if (m_unitInfos.ContainsKey(path)) {
                m_unitInfos[path] = baseInfo;
            } else {
                m_unitInfos.Add(path, baseInfo);
            }
        }

        LoadUnitModel(baseInfo.model);
        return baseInfo;
    }

    public UnitInfo GetUnitInfo(string name) {
        return m_unitInfos[name];
    }

    Dictionary<string, UnitInfo> m_unitInfos = new Dictionary<string, UnitInfo>();

    /// <summary>
    /// 加载预设ProjectilesData中的数据
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public ProjectileInfo LoadProjectile(string path) {
        if (path == null) {
            return null;
        }

        ProjectileInfo baseInfo;
        if (m_projectileInfos.TryGetValue(path, out baseInfo)) {
            LoadProjectileModel(baseInfo.model);
            return baseInfo;
        }

        TextAsset res = Resources.Load<TextAsset>(string.Format("{0}", path));
        if (res == null) {
            return null;
        }

        baseInfo = JsonMapper.ToObject<ProjectileInfo>(res.text);
		//Resources.UnloadAsset(res);
        if (baseInfo == null) {
            return null;
        }

        m_projectileInfos.Add(path, baseInfo);
        LoadProjectileModel(baseInfo.model);
        return baseInfo;
    }

    /// <summary>
    /// 加载预设ProjectilesData中的数据
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public ProjectileInfo LoadProjectile(string path, string data) {
        if (path == null) {
            return null;
        }

        ProjectileInfo baseInfo;
        if (path.Length > 0 && m_projectileInfos.TryGetValue(path, out baseInfo)) {
            LoadProjectileModel(baseInfo.model);
            return baseInfo;
        }

        baseInfo = JsonMapper.ToObject<ProjectileInfo>(data);
        if (baseInfo == null) {
            return null;
        }

        if (path.Length > 0) {
            if (m_projectileInfos.ContainsKey(path)) {
                m_projectileInfos[path] = baseInfo;
            } else {
                m_projectileInfos.Add(path, baseInfo);
            }
        }

        LoadProjectileModel(baseInfo.model);
        return baseInfo;
    }

    public ProjectileInfo GetProjectileInfo(string name) {
        return m_projectileInfos[name];
    }

    Dictionary<string, ProjectileInfo> m_projectileInfos = new Dictionary<string, ProjectileInfo>();


    // Skills
    public void LoadBaseSkills() {
        Skill skill;
        skill = new SplashPas("SplashAttack", 0.5f, new Coeff(0.75f, 0), 1f, new Coeff(0.25f, 0));
        AddBaseSkill(skill);
    }

    void AddBaseSkill(Skill skill) {
        Type t = skill.GetType();
        m_skills.Add(t.Name, skill);
    }

    public Skill LoadSkill(string path) {
        if (path == null) {
            return null;
        }

        Skill skill;
        if (path.Length > 0 && m_skills.TryGetValue(path, out skill)) {
            return skill;
        }

        TextAsset res = Resources.Load<TextAsset>(path);
        if (res == null) {
            return null;
        }

        SkillInfoOnlyBaseId baseInfo = JsonUtility.FromJson<SkillInfoOnlyBaseId>(res.text);
		//Resources.UnloadAsset(res);
        if (baseInfo == null || baseInfo.baseId.Length == 0) {
            return null;
        }

        Skill baseSkill = LoadSkill(baseInfo.baseId);
        if (baseSkill == null) {
            return null;
        }

        return baseSkill.Clone(res.text);
    }

    Dictionary<string, Skill> m_skills = new Dictionary<string, Skill>();

    public enum DataType {
        UnitData,
        ProjectileData
    }

    public struct ResInfo {
        public ResInfo(DataType type, string path) {
            this.type = type;
            this.path = path;
        }

        public DataType type;
        public string path;
    }

    Queue<ResInfo> m_loadingQueue = new Queue<ResInfo>();

    /// <summary>
    /// 将要加载的资源添加到加载队列中
    /// </summary>
    /// <param name="res"></param>
    public void AddResourceToLoadingQueue(ResInfo res) {
        m_loadingQueue.Enqueue(res);
    }

    public void AddUnitsToLoadingQueue(string[] units) {
        foreach (var unit in units) {
            AddResourceToLoadingQueue(new ResInfo(DataType.UnitData, unit));
        }
    }

    public void AddProjectilesToLoadingQueue(string[] projectiles) {
        foreach (var projectile in projectiles) {
            AddResourceToLoadingQueue(new ResInfo(DataType.ProjectileData, projectile));
        }
    }

    public enum LoadingProgressType {
        Resource,
        Scene
    }

    public struct LoadingProgressInfo {
        public LoadingProgressInfo(LoadingProgressType type, float total) {
            this.type = type;
            current = 0.0f;
            this.total = total;
        }

        public LoadingProgressType type;
        public float current;
        public float total;
    }

    public delegate void OnUpdateProgress(LoadingProgressInfo prog);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="onUpdate"></param>
    /// <returns></returns>
    IEnumerator LoadResourcesFromQueue(OnUpdateProgress onUpdate) {
        int total = m_loadingQueue.Count;
        LoadingProgressInfo prog = new LoadingProgressInfo(LoadingProgressType.Resource, m_loadingQueue.Count);
        onUpdate(prog);
        yield return null;
        while (m_loadingQueue.Count > 0) {
            ResInfo res = m_loadingQueue.Dequeue();
            switch (res.type) {
            case DataType.UnitData:
                LoadUnit(res.path);
                break;
            case DataType.ProjectileData:
                LoadProjectile(res.path);
                break;
            }
            prog.current += 1.0f;;
            onUpdate(prog);
            yield return null;
        }
        // yield return *;
    }

    string m_nextScene;
    AsyncOperation m_nextSceneAop;

    public void StartLoadingScene(string nextScene) {
        m_nextScene = nextScene;
        SceneManager.LoadScene("Loading");
    }

    IEnumerator ReplaceScene(OnUpdateProgress onUpdate) {
        m_nextSceneAop = SceneManager.LoadSceneAsync(m_nextScene);
        m_nextSceneAop.allowSceneActivation = false;
        LoadingProgressInfo prog = new LoadingProgressInfo(LoadingProgressType.Scene, 0.9f);
        onUpdate(prog);
        yield return null;
        while (m_nextSceneAop.progress < 0.9f) {
            prog.current = m_nextSceneAop.progress;
            onUpdate(prog);
            yield return null;
        }
        prog.current = 0.9f;
        onUpdate(prog);
        yield return null;
    }

    public IEnumerator LoadResourcesFromQueueAndReplaceScene(OnUpdateProgress onUpdate) {
        yield return LoadResourcesFromQueue(onUpdate);
        yield return ReplaceScene(onUpdate);
    }

    public void StartScene() {
        m_nextSceneAop.allowSceneActivation = true;
    }
}

