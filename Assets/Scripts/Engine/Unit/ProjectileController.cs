using UnityEngine;
using System;


[Serializable]
public class SyncProjectileInfo
{
    public static SyncProjectileInfo Create(Projectile projectile)
    {
        SyncProjectileInfo syncInfo = new SyncProjectileInfo();

        syncInfo.baseInfo.root = projectile.Root;
        syncInfo.baseInfo.move = projectile.MoveSpeed;
        syncInfo.baseInfo.height = projectile.MaxHeightDelta;
        syncInfo.baseInfo.fire = Projectile.FireTypeToName(projectile.TypeOfFire);
        syncInfo.baseInfo.effect = (int)projectile.EffectFlags;

        syncInfo.position = projectile.Renderer.Node.position;
        syncInfo.visible = projectile.Renderer.Node.visible;
        syncInfo.fromTo = projectile.TypeOfFromTo;
        syncInfo.useFireOffset = projectile.UseFireOffset;
        syncInfo.srcUnit = projectile.SourceUnit != null ? projectile.SourceUnit.Id : 0;
        syncInfo.fromUnit = projectile.FromUnit != null ? projectile.FromUnit.Id : 0;
        syncInfo.toUnit = projectile.ToUnit != null ? projectile.ToUnit.Id : 0;
        syncInfo.fromPos = projectile.FromPosition;
        syncInfo.toPos = projectile.ToPosition;

        return syncInfo;
    }

    public int id;
    public ProjectileInfo baseInfo = new ProjectileInfo();
    public Vector2Serializable position;
    public bool visible;
    public Projectile.FromToType fromTo;
    public bool useFireOffset;
    public int srcUnit;
    public int fromUnit;
    public int toUnit;
    public Vector2Serializable fromPos;
    public Vector2Serializable toPos;
}


public class ProjectileController : MonoBehaviour
{
    protected Projectile m_projectile;

    public Projectile projectile
    {
        get
        {
            return m_projectile;
        }
    }

    // ����projectile��¡��ֻ����������Ϣ
    public static ProjectileController Create(string path)
    {
		GameObject gameObject = GameObjectPool.instance.Instantiate(WorldController.instance.projectilePrefab);
        ProjectileController projCtrl = gameObject.GetComponent<ProjectileController>();

        ResourceManager.instance.Load<ProjectileResInfo>(path);
        //ProjectileRenderer r = new ProjectileRenderer(WorldController.instance.projectilePrefab, gameObject);
        ProjectileRenderer r = ObjectPool<ProjectileRenderer>.instance.Instantiate(); r.Init(WorldController.instance.projectilePrefab, gameObject);
        ResourceManager.instance.PrepareProjectileResource(path, r);

        //Projectile projectile = new Projectile(r);
        Projectile projectile = ObjectPool<Projectile>.instance.Instantiate(); projectile.Init(r);
        projectile.m_root = path;

        projCtrl.m_projectile = projectile;
        WorldController.instance.world.AddProjectile(projectile);

        return projCtrl;
    }

    // ����projectile
    public static ProjectileController Create(SyncProjectileInfo syncInfo)
    {
        ProjectileController projCtrl = Create(syncInfo.baseInfo.root);
        Projectile projectile = projCtrl.projectile;
        SetProjectileFromBaseInfo(projectile, syncInfo.baseInfo);

        projectile.Renderer.Node.position = syncInfo.position;
        projectile.Renderer.Node.visible = syncInfo.visible;
        projectile.TypeOfFromTo = syncInfo.fromTo;
        projectile.UseFireOffset = syncInfo.useFireOffset;
        projectile.SourceUnit = projectile.World.GetUnit(syncInfo.srcUnit);
        projectile.FromUnit = projectile.World.GetUnit(syncInfo.fromUnit);
        projectile.ToUnit = projectile.World.GetUnit(syncInfo.toUnit);
        projectile.FromPosition = syncInfo.fromPos;
        projectile.ToPosition = syncInfo.toPos;

        return projCtrl;
    }

    // ���ڴ���projectileģ�壬ͨ���������ü���
    public static Projectile CreateProjectileTemplate(string path)
    {
        ProjectileInfo baseInfo = ResourceManager.instance.LoadProjectile(path);
        if (baseInfo == null)
        {
            return null;
        }

        ProjectileRenderer r = ObjectPool<ProjectileRenderer>.instance.Instantiate();
        //Projectile projectile = new Projectile(r);
        Projectile projectile = ObjectPool<Projectile>.instance.Instantiate(); projectile.Init(r);
        projectile.m_root = baseInfo.root;
        SetProjectileFromBaseInfo(projectile, baseInfo);

        return projectile;
    }

    // ��baseInfo�ж�ȡ��root֮�����Ϣ
	static void SetProjectileFromBaseInfo(Projectile projectile, ProjectileInfo baseInfo)
    {
        projectile.MoveSpeed = (float)baseInfo.move;
        projectile.MaxHeightDelta = (float)baseInfo.height;
        projectile.TypeOfFire = Projectile.FireNameToType(baseInfo.fire);
        projectile.EffectFlags = (uint)baseInfo.effect;
    }
}
