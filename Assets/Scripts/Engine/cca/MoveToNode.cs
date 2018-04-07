using UnityEngine;
using System.Collections;
using cca;

// projectile move to unit
public class MoveToNode : ActionInterval
{
    public const int kTypeNode = 1;
    public const int kTypeUnit = 2;
    public const int kTypeProjectile = 3;

    public MoveToNode(float duration, UnitNode unit, bool fixRotation = true, float maxHeightDelta = 0.0f)
        : base(duration)
    {
        _eToType = kTypeUnit;
        _unit = unit;

        _bFixRotation = fixRotation;
        _fMaxHeightDelta = maxHeightDelta;
        _fMinSpeed = 0;
    }

    public override Action clone()
    {
        return new MoveToNode(_duration, _unit, _bFixRotation, _fMaxHeightDelta);
    }

    public override Action reverse()
    {
        Debug.Assert(false);
        return null;
    }

    public override void startWithTarget(Node target)
    {
        base.startWithTarget(target);

        _eFromType = kTypeProjectile;
        NodeWithHeight rendererTarget = target as NodeWithHeight;
        _oStartPos = rendererTarget.position;
        _oEndPos = _unit.Node.position;
        _fFromHeight = rendererTarget.height;

        _oDeltaPos = _oEndPos - _oStartPos;
        _fMinSpeed = Mathf.Sqrt(_oDeltaPos.x * _oDeltaPos.x + _oDeltaPos.y * _oDeltaPos.y) / _duration;
    }

    public override void update(float time)
    {
        if (!_target)
        {
            return;
        }

        if (_unit.Valid)
        {
            _oEndPos = _unit.Node.position;
            _fToHeight = _unit.Node.height + _unit.HalfOfHeight;
        }

        _oDeltaPos = _oEndPos - _oStartPos;
        _fDeltaHeight = _fToHeight - _fFromHeight;

        float distance = Mathf.Sqrt(_oDeltaPos.x * _oDeltaPos.x + _oDeltaPos.y * _oDeltaPos.y);
        _duration = distance / _fMinSpeed;

        // 抛物线
        float fA = distance;
        float fX = time * fA - fA / 2;
        fA = -4 * _fMaxHeightDelta / (fA * fA);
        float fHeightDelta = fA * fX * fX + _fMaxHeightDelta;

        NodeWithHeight rendererTarget = _target as NodeWithHeight;
        rendererTarget.position = _oStartPos + _oDeltaPos * time;
        rendererTarget.height = _fFromHeight + _fDeltaHeight * time + fHeightDelta;

        if (_bFixRotation)
        {
            // 修正角度
            //float fOffsetR = Mathf.Atan(fA * fX);
            rendererTarget.rotation = (
                Mathf.Atan2(_oEndPos.y - _oStartPos.y, _oEndPos.x - _oStartPos.x) +
                (_oStartPos.x < _oEndPos.x ? Mathf.Atan(fA * fX) : -Mathf.Atan(fA * fX))
                ) * Mathf.Rad2Deg;
        }
    }
    
    protected Vector2 _oStartPos;
    protected Vector2 _oEndPos;
    protected Vector2 _oDeltaPos;

    //protected RendererNode _endNode;
    protected UnitNode _unit;
    protected int _eFromType;
    protected int _eToType;
    protected float _fMinSpeed;
    protected float _fFromHeight;
    protected float _fToHeight;
    protected float _fDeltaHeight;
    protected float _fMaxHeightDelta;
    protected float _fA;
    protected bool _bFixRotation;
}
