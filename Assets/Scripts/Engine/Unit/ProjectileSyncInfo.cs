using System;
using UnityEngine;


[Serializable]
public class ProjectileSyncInfo {
    public ProjectileSyncInfo() {
    }

    public ProjectileSyncInfo(int id, ProjectileInfo baseInfo) {
        Debug.Assert(GamePlayerController.localClient.isServer);
        this.id = id;
        this.baseInfo = baseInfo;
    }

#if false
    public SyncProjectileInfo(Projectile projectile) {
        ProjectileNode node = projectile.Node;

        baseInfo.model = projectile.Model;
        baseInfo.move = projectile.MoveSpeed;
        baseInfo.height = projectile.MaxHeightDelta;
        baseInfo.fire = Projectile.FireTypeToName(projectile.TypeOfFire);
        baseInfo.effect = (int)projectile.EffectFlags;

        //position = node.position;
        //visible = node.visible;
        fromTo = projectile.TypeOfFromTo;
        useFireOffset = projectile.UseFireOffset;
        srcUnit = projectile.SourceUnit != null ? projectile.SourceUnit.Id : 0;
        fromUnit = projectile.FromUnit != null ? projectile.FromUnit.Id : 0;
        toUnit = projectile.ToUnit != null ? projectile.ToUnit.Id : 0;
        fromPos = projectile.FromPosition;
        toPos = projectile.ToPosition;
    }
#endif

    public int id;
    public ProjectileInfo baseInfo = new ProjectileInfo();
    //public Vector2Serializable position;
    //public bool visible;
    public Projectile.FromToType fromTo;
    public bool useFireOffset;
    public int srcUnit;
    public int fromUnit;
    public int toUnit;
    public Vector2Serializable fromPos;
    public Vector2Serializable toPos;
}
