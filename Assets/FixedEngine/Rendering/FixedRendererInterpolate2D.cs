using UnityEngine;
using FixedEngine;

//[DisallowMultipleComponent]
public class FixedRendererInterpolated2D : MonoBehaviour
{
    /*// 1) Choix du format dans l’Inspector
    public enum Format { Q8_8, Q8_4  }
    public Format format = Format.Q8_8;

    // 2) Interface interne
    interface IRenderer { void Render(); }
    IRenderer _renderer;


    FixedTransform2D<Q8_8> xf;


    void Start()
    {
        // 1) Récupère d’abord le GhostController (qui sait créer et garder sa XF)
        var ghost = GetComponentInParent<GhostController>();
        if (ghost == null)
        {
            Debug.LogError("FixedRendererInterpolated2D : pas de GhostController parent !", this);
            enabled = false;
            return;
        }

        // 2) Demande-lui sa transform logique
        switch (format)
        {
            case Format.Q8_8:
                var xf88 = ghost.XF88;
                if (xf88 == null)
                {
                    Debug.LogError("GhostController.XF est null !", this);
                    enabled = false;
                    return;
                }

                _renderer = new GenericRenderer<Q8_8>(xf88, transform);
                break;

                // … autres formats …
        }
    }

    void LateUpdate()
    {
        _renderer?.Render();
    }

    // 3) Classe générique qui contient toute la magie de l’interpolation
    class GenericRenderer<TFormat> : IRenderer
        where TFormat : struct, IFixedPointFormat
    {
        readonly FixedTransform2D<TFormat> _xf;
        readonly Transform _view;
        FixedVector3<TFormat> _prev, _curr;

        public GenericRenderer(FixedTransform2D<TFormat> xf, Transform view)
        {
            _xf = xf;
            _view = view;
            _curr = xf.LocalPosition;
            _prev = _curr;
        }

        public void Render()
        {
            // 1) MàJ des positions quand la simulation tick
            var np = _xf.LocalPosition;
            if (!np.Equals(_curr))
            {
                _prev = _curr;
                _curr = np;
            }

            // 2) Alpha
            double acc = TickManager.Instance.accumulator;
            double interval = TickManager.Instance.tickInterval;
            var raw = FixedPoint<TFormat>.FromDouble(acc / interval);
            var alpha = FixedPoint<TFormat>.Clamp(raw,
                               FixedPoint<TFormat>.Zero,
                               FixedPoint<TFormat>.One);

            // 3) Lerp composante par composante
            var ip = FixedVectorMath.Lerp<TFormat>(_prev, _curr, alpha);

            // 4) Snap pixel-perfect
            int fb = FixedFormatUtil<TFormat>.FractionBits;
            int x = ip.x.Raw >> fb;
            int y = ip.y.Raw >> fb;

            // 5) On bouge la vue en world space
            _view.position = new Vector3(x, y, _view.position.z);
        }
    }*/
}
