using FixedEngine;
using UnityEngine;

[AddComponentMenu("FixedEngine/Movement/FixedMover2D_Q8_4")]
public class FixedMover2D_Q8_4 : MonoBehaviour, IFixedTick
{
    [SerializeField] private bool snapOnAwake = true;

    private FixedMover2D<Q8_4> mover;
    private FixedTransform2D<Q8_4> xf;

    void Awake()
    {
        xf = new FixedTransform2D<Q8_4>();
        mover = new FixedMover2D<Q8_4> { Transform = xf };

        if (snapOnAwake)
            mover.SnapFromUnityPosition(transform.position);

        TickManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        TickManager.Instance.Unregister(this);
    }

    public void FixedTick()
    {
        // logique pure, à cadence fixe et déterministe
        // ex: mover.Move(delta); appelé avant ou injecté
    }

    void LateUpdate()
    {
        // rendu : non déterministe, mais stable
        xf.ApplyToTransform2D(transform);
    }

    // Ces méthodes seront appelées par ton GameObject Gameplay :
    public void Move(FixedVector2<Q8_4> delta) => mover.Move(delta);
}
