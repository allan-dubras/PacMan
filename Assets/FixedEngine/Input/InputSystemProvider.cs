// File: InputSystemProvider.cs
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace FixedEngine.Input
{
    /// <summary>
    /// Lit les InputActions de l’Asset Unity et fournit des valeurs fixes déterministes.
    /// Utilise uniquement des boucles for pour éviter les allocations de foreach.
    /// </summary>
    public class InputSystemProvider<TFormat> : MonoBehaviour, IInputProvider<TFormat>
        where TFormat : struct, IFixedPointFormat
    {
        [Tooltip("L’Input Action Asset contenant vos maps et actions.")]
        public InputActionAsset asset;

        [Tooltip("Liste des noms de ActionMaps à activer. Laisser vide pour activer toutes les maps.")]
        public List<string> actionMapNames = new();

        private readonly List<string> actionNames = new();
        private readonly Dictionary<string, InputAction> actions = new();
        private readonly Dictionary<string, bool> currentButtonStates = new();
        private readonly Dictionary<string, bool> lastButtonStates = new();
        private readonly Dictionary<string, FixedPoint<TFormat>> currentAxisStates = new();

        private int lastUpdatedTick = -2;

        private void Awake()
        {
            if (asset == null)
            {
                Debug.LogError("InputSystemProvider: Aucun InputActionAsset assigné.");
                return;
            }

            actions.Clear();
            actionNames.Clear();
            currentButtonStates.Clear();
            lastButtonStates.Clear();
            currentAxisStates.Clear();

            // Parcours des maps par index
            var maps = asset.actionMaps;
            for (int mi = 0; mi < maps.Count; mi++)
            {
                var map = maps[mi];
                if (actionMapNames.Count == 0 || actionMapNames.Contains(map.name))
                {
                    map.Enable();

                    // Parcours des actions par index
                    var acts = map.actions;
                    for (int ai = 0; ai < acts.Count; ai++)
                    {
                        var action = acts[ai];
                        string fullName = $"{map.name}/{action.name}";

                        actions[fullName] = action;
                        actionNames.Add(fullName);

                        // Initialisation des états
                        currentButtonStates[fullName] = false;
                        lastButtonStates[fullName] = false;
                        currentAxisStates[fullName] = FixedMath.Zero<TFormat>();
                    }
                }
            }
        }

        /// <summary>
        /// Doit être appelé une seule fois par tick (via TickManager).
        /// Met à jour les états boutons et axes en fixe pur.
        /// </summary>
        public void UpdateState()
        {
            int currentTick = FixedTickContext.CurrentTick;
            if (lastUpdatedTick == currentTick)
            {
                Debug.LogError("[InputSystemProvider] UpdateState appelé plusieurs fois dans le même tick.");
                return;
            }
            lastUpdatedTick = currentTick;

            // Seuil fixe raw pour considérer un axe comme "appuyé"
            int thresholdRaw = FixedMath.Half<TFormat>().Raw;

            // Parcours des noms d'actions par index
            for (int i = 0; i < actionNames.Count; i++)
            {
                string name = actionNames[i];
                if (!actions.TryGetValue(name, out var action))
                    continue;

                // 1) Lecture Unity (float) et quantification fixe
                var axisFp = FixedInputQuantizer.Quantize<TFormat>(action.ReadValue<float>());
                currentAxisStates[name] = axisFp;

                // 2) État bouton (aucun float)
                bool isPressed = axisFp.Raw > thresholdRaw;

                // 3) Mise à jour des historiques
                bool prevPressed = currentButtonStates[name];
                lastButtonStates[name] = prevPressed;
                currentButtonStates[name] = isPressed;
            }
        }

        public bool GetButton(string name) =>
            currentButtonStates.TryGetValue(name, out var v) && v;

        public bool GetButtonDown(string name) =>
            currentButtonStates.TryGetValue(name, out var curr) &&
            lastButtonStates.TryGetValue(name, out var prev) &&
            curr && !prev;

        public bool GetButtonUp(string name) =>
            currentButtonStates.TryGetValue(name, out var curr) &&
            lastButtonStates.TryGetValue(name, out var prev) &&
            !curr && prev;

        public FixedPoint<TFormat> GetAxis(string name) =>
            currentAxisStates.TryGetValue(name, out var v)
                ? v
                : FixedMath.Zero<TFormat>();

        public FixedVector2<TFormat> GetVector2(string name) =>
            FixedInputQuantizer.QuantizeVector2<TFormat>(ReadRaw<Vector2>(name));

        public FixedVector3<TFormat> GetVector3(string name) =>
            FixedInputQuantizer.QuantizeVector3<TFormat>(ReadRaw<Vector3>(name));

        private T ReadRaw<T>(string name) where T : struct =>
            actions.TryGetValue(name, out var action) ? action.ReadValue<T>() : default;
    }
}
