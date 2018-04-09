using UnityEngine;
using System;


public class ProjectileController : MonoBehaviour {
    protected Projectile m_projectile;

    public Projectile projectile {
        get { return m_projectile; }
    }

    // 用于projectile克隆，只包含少数信息
    public static ProjectileController Create(string path) {
        GameObject gameObject = GameObjectPool.instance.Instantiate(WorldController.instance.projectilePrefab);
        ProjectileController projCtrl = gameObject.GetComponent<ProjectileController>();

        ResourceManager.instance.LoadProjectileModel(path);  // high time cost
        //ProjectileRenderer r = new ProjectileRenderer(WorldController.instance.projectilePrefab, gameObject);
        ProjectileNode r = ObjectPool<ProjectileNode>.instance.Instantiate();
        r.Init(WorldController.instance.projectilePrefab, gameObject);
        ResourceManager.instance.AssignModelToProjectileNode(path, r);

        //Projectile projectile = new Projectile(r);
        Projectile projectile = ObjectPool<Projectile>.instance.Instantiate();
        projectile.Init(r);
        projectile.m_model = path;

        projCtrl.m_projectile = projectile;
        World.Main.AddProjectile(projectile);

        return projCtrl;
    }

    // 创建projectile
    public static ProjectileController Create(SyncProjectileInfo syncInfo) {
        ProjectileController projCtrl = Create(syncInfo.baseInfo.model);
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

    // 用于创建projectile模板，通常用于配置技能
    public static Projectile CreateProjectileTemplate(string path) {
        ProjectileInfo baseInfo = ResourceManager.instance.LoadProjectile(path);
        if (baseInfo == null) {
            return null;
        }

        ProjectileNode r = ObjectPool<ProjectileNode>.instance.Instantiate();
        //Projectile projectile = new Projectile(r);
        Projectile projectile = ObjectPool<Projectile>.instance.Instantiate();
        projectile.Init(r);
        projectile.m_model = baseInfo.model;
        SetProjectileFromBaseInfo(projectile, baseInfo);

        return projectile;
    }

    // 从baseInfo中读取除model之外的信息
    static void SetProjectileFromBaseInfo(Projectile projectile, ProjectileInfo baseInfo) {
        projectile.MoveSpeed = (float)baseInfo.move;
        projectile.MaxHeightDelta = (float)baseInfo.height;
        projectile.TypeOfFire = Projectile.FireNameToType(baseInfo.fire);
        projectile.EffectFlags = (uint)baseInfo.effect;
    }
}
