using System.Collections.Generic;
using System.Diagnostics;

using UnityEditor;

using UnityEngine;

namespace FixedEngine
{
    [DefaultExecutionOrder(-1000)]
    public class TickManager : MonoBehaviour
    {
        public static TickManager Instance { get; private set; }

        [Tooltip("Ticks logiques par seconde")]
        public int ticksPerSecond = 60;

        private readonly Stopwatch sw = Stopwatch.StartNew();     // Démarre tout de suite
        public double accumulator;                                // Temps accumulé en secondes
        public double tickInterval;                               // 1 / ticksPerSecond

        private readonly List<IFixedTick> listeners = new();

        private bool wasPausedLastFrame = false;
        private double logicalTimeSeconds = 0;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            tickInterval = 1.0 / ticksPerSecond;
            accumulator = 0.0;

            sw.Restart();

            Application.targetFrameRate = 144;
        }

#if !UNITY_EDITOR
        void OnEnable()
        {
            EditorApplication.update += EditorUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            if (!Application.isPlaying)
                return;

            if (EditorApplication.isPaused)
            {
                sw.Restart(); // reset stopwatch during pause
                return;
            }

            ProcessTicks();
        }
#endif


#if UNITY_EDITOR
        void Update()
        {
            ProcessTicks();
        }
#endif


        void ProcessTicks()
        {
            // 1) Récupère tout de suite le temps écoulé depuis le dernier Update
            double deltaSeconds = sw.Elapsed.TotalSeconds;
            sw.Restart();


            // 2) Accumule
            accumulator += deltaSeconds;

            // 3) Rattrape chaque tick manqué
            while (accumulator >= tickInterval)
            {
                for (int i = 0, n = listeners.Count; i < n; i++)
                    listeners[i].FixedTick();

                accumulator -= tickInterval;
            }
        }



        public void Register(IFixedTick t) => listeners.Add(t);
        public void Unregister(IFixedTick t) => listeners.Remove(t);
    }



    public interface IFixedTick
    {
        void FixedTick();
    }
}
