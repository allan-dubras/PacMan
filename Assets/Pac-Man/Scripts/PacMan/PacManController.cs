using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using FixedEngine;
public class PacManController : MonoBehaviour, IFixedTick
{
    [SerializeField] private InputActionAsset InputActionsAsset;
    [SerializeField] private Tilemap CollisionMap;
    [SerializeField] private int TileSize = 8;
    [SerializeField] private LogicGrid Grid;



    [SerializeField] private int speedRaw = 256;
    private FixedPoint<Q8_8> _speed;
    private FixedTransform2D<Q8_8> _xf;
    private FixedMover2D<Q8_8> _mover;

    private InputAction _moveAction;

    private FixedVector2<Q8_8> _desiredDir = new FixedVector2<Q8_8>();
    private FixedVector2<Q8_8> _currentDir = new FixedVector2<Q8_8>();
    private bool _isPreTurning;

    private Vector3Int _nextCell;
    private FixedPoint<Q8_8> _turnTargetX;
    private FixedPoint<Q8_8> _turnTargetY;

    public void Awake()
    {
        //Input
        var map = InputActionsAsset.FindActionMap("Player", true);
        _moveAction = map.FindAction("Move", true);
        _moveAction.Enable();

        //Init FixedEngine
        _speed = new FixedPoint<Q8_8>(speedRaw);
        _xf = new FixedTransform2D<Q8_8>();
        _mover = new FixedMover2D<Q8_8> { Transform = _xf };

        _mover.SnapToCellFromUnity(transform.position, TileSize, Grid.origin);

        //1) Calculer la cellule de départ
        Vector3Int startCell = CollisionMap.WorldToCell(transform.position);

        //2) Récupère la logicCell
        var logic = Grid.GetCell(startCell);

        TickManager.Instance.Register(this);
    }

    public void OnDestroy()
    {
        TickManager.Instance.Unregister(this);
        _moveAction.Disable();
    }
    public void FixedTick()
    {
        //1.a)Gestion input 
        Vector2 rawInput = _moveAction.ReadValue<Vector2>();
        var inputDir = FixedInputQuantizer.QuantizeVector2<Q8_8>(rawInput);

        //Ignorer les diagonales
        if ((inputDir.x.Raw != 0 && inputDir.y.Raw == 0) || (inputDir.y.Raw != 0 && inputDir.x.Raw == 0))
        {
            if (!_isPreTurning)
            {
                _desiredDir = inputDir;
            }
            else
            {
                //1.b.iii) En pre-turn, on ignore les directions identiques ou opposées au currentDir
                if (inputDir != _currentDir && inputDir != -_currentDir)
                {
                    _desiredDir = inputDir;
                }
            }
        }

        //2) Position courante , cellule et centre 
        FixedVector2<Q8_8> pos = _mover.Position2D;
        var worldPos = new Vector3(pos.x.ToFloat(), pos.y.ToFloat(), transform.position.z);
        Vector3Int cell = CollisionMap.WorldToCell(worldPos);
        FixedVector2<Q8_8> center = GetFixedCenter(cell);

        //3) Lancement initial ou quand Pac-Man est à l'arrêt 
        if (_currentDir == FixedVector2<Q8_8>.Zero
        && _desiredDir != FixedVector2<Q8_8>.Zero
        && CanMove(cell, _desiredDir))
        {
            _currentDir = _desiredDir;
        }

        //4) Demi-Tour autorisée à tout moment
        if (_desiredDir == -_currentDir && CanMove(cell, _desiredDir))
        {
            _currentDir = _desiredDir;
        }

        //5)
        if (!_isPreTurning
            && _currentDir != FixedVector2<Q8_8>.Zero
            && _desiredDir != _currentDir
            && _desiredDir != -_currentDir
            && CanMove(cell, _desiredDir))
        {
            var nCell = cell + new Vector3Int(
                FixedMath.Sign(_desiredDir.x),
                FixedMath.Sign(_desiredDir.y),
                0);

            //vérification supplémentaire: on n'anticipe pas dans le vide 
            var logic = Grid.GetCell(nCell);
            if (logic == null || logic.isWalkable)
            {
                goto SkipPreTurn;
            }
            var nextCenter = GetFixedCenter(nCell);

            if (_currentDir.x.Raw != 0)
            {
                _turnTargetX = center.x;
                _turnTargetY = nextCenter.y;
            }
            else
            {
                _turnTargetX = nextCenter.x;
                _turnTargetY = center.y;
            }
            _isPreTurning = true;
        }

    SkipPreTurn:
        //6) Execution du pré-virage (cornering)
        if (_isPreTurning)
        {
            FixedVector2<Q8_8> curPos = _mover.Position2D;
            var dx = _turnTargetX - curPos.x;
            var dy = _turnTargetY - curPos.y;

            FixedPoint<Q8_8> moveX = new FixedPoint<Q8_8>(0);
            FixedPoint<Q8_8> moveY = new FixedPoint<Q8_8>(0);

            if (dx.Raw != 0)
                moveX = (FixedMath.Abs(dx).Raw <= _speed.Raw) ? dx : FixedMath.Sign(dx) * _speed;

            if (dy.Raw != 0)
                moveY = (FixedMath.Abs(dy).Raw <= _speed.Raw) ? dy : FixedMath.Sign(dy) * _speed;

            _mover.Move(new FixedVector2<Q8_8>(moveX, moveY));

            if (dx.Raw == 0 && dy.Raw == 0)
            {
                _isPreTurning = false;
                _currentDir = _desiredDir;
            }

        }
        //7) Protège contre le fait de traverser les murs , sinon déplacement normal.
        if (_currentDir != FixedVector2<Q8_8>.Zero)
        {
            if (CanMove(cell, _currentDir))
            {
                SafeMove(_currentDir);
            }
            else
            {
                var deltaX = center.x - pos.x;
                var deltaY = center.y - pos.y;

                var moveX = (FixedMath.Abs(deltaX).Raw <= _speed.Raw) ? deltaX : FixedMath.Sign(deltaX) * _speed;
                var moveY = (FixedMath.Abs(deltaY).Raw <= _speed.Raw) ? deltaY : FixedMath.Sign(deltaY) * _speed;
            }

        }
    }

    public void SafeMove(FixedVector2<Q8_8> direction)
    {
        /*const int subSteps = 4;
        var step = -speedRaw / FixedPoint<Q8_8>.FromInt(subSteps);
        var start = _mover.Position2D;
        //1)Cellule d'origine
        Vector3 worldStart = new Vector3(start.x.ToFloat(),start.y.ToFloat(),0f);
        Vector3Int origineCell = CollisionMap.WorldToCell(worldStart);

        for (int i = 0; i < subSteps; i++)
        {
            var offset = direction * (step * FixedPoint<Q8_8>.FromInt(i));
            var nextPos = start + offset;
            Vector3 worldPos = new Vector3(nextPos.x.ToFloat(), nextPos.y.ToFloat(), 0f);
            Vector3Int cell = CollisionMap.WorldToCell(worldPos);
            var logic = Grid.GetCell(cell);
            bool walkable = (logic == null) || logic.isWalkable;


        }*/
        _mover.Move(direction * _speed.Raw);
    }

    private bool CanMove(Vector3Int cell, FixedVector2<Q8_8> dir)
    {
        int dx = dir.x.Raw > 0 ? 1 : dir.x.Raw < 0 ? -1 : 0;
        int dy = dir.y.Raw > 0 ? 1 : dir.y.Raw < 0 ? -1 : 0;

        _nextCell = new Vector3Int(cell.x + dx, cell.y + dy, 0);
        var logic = Grid.GetCell(_nextCell);

        return (logic != null && logic.isWalkable) || logic != null;
    }
    private FixedVector2<Q8_8> GetFixedCenter(Vector3Int cell)
    {
        Vector3 worldCenter = CollisionMap.GetCellCenterWorld(cell);
        return new FixedVector2<Q8_8>(worldCenter.x, worldCenter.y);
    }


    void LateUpdate()
    {
        _xf.ApplyToTransform(transform);
    }
}
