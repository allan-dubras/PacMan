// File: IInputProvider.cs
using UnityEngine;

namespace FixedEngine
{
    /// <summary>
    /// Interface déterministe pour l'accès aux entrées utilisateurs dans le FixedEngine.
    /// Toutes les valeurs analogiques sont exprimées en FixedPoint<TFormat>.
    /// </summary>
    public interface IInputProvider<TFormat> where TFormat : struct, IFixedPointFormat
    {
        /// <summary>Met à jour les états de toutes les actions au début de chaque tick logique.</summary>
        void UpdateState();

        /// <summary>Retourne vrai si l’action booléenne est active ce tick (ex: bouton).</summary>
        bool GetButton(string actionName);

        /// <summary>Retourne vrai si l’action a été pressée ce tick (transition up→down).</summary>
        bool GetButtonDown(string actionName);

        /// <summary>Retourne vrai si l’action a été relâchée ce tick (transition down→up).</summary>
        bool GetButtonUp(string actionName);

        /// <summary>Valeur déterministe d’un axe analogique entre -1 et 1.</summary>
        FixedPoint<TFormat> GetAxis(string actionName);

        /// <summary>Valeur d’un vecteur 2D (stick, d-pad) en coordonnées déterministes.</summary>
        FixedVector2<TFormat> GetVector2(string actionName);

        /// <summary>Valeur d’un vecteur 3D (rarement utilisé, ex: simulateur).</summary>
        FixedVector3<TFormat> GetVector3(string actionName);
    }
}
