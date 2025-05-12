using System;
using UnityEngine;

namespace FixedEngine
{
    /// <summary>
    /// Un timer déterministe basé sur le temps logique (`FixedPoint<TFormat>`), compatible avec des systèmes à tick fixe.
    /// </summary>
    /// <typeparam name="TFormat">
    /// Le format fixe utilisé pour les calculs (par exemple Q8_4 ou Q8_4U).
    /// </typeparam>
    /// <remarks>
    /// Ce timer ne dépend pas du framerate. Il est conçu pour les moteurs rétro, les systèmes de jeu réseau ou WebGL,
    /// où les comportements doivent être strictement déterministes.
    /// </remarks>
    /// <example>
    /// Exemple d’utilisation :
    ///
    /// <code>
    /// var timer = new FixedTimer<Q8_4>(2f, loop: true, repeatCount: 3, label: "BonusCooldown");
    /// timer.Update(FixedPoint<Q8_4>.FromFloat(0.5f));
    /// if (timer.DoneThisFrame) SpawnBonus();
    /// </code>
    /// </example>
    public struct FixedTimer<TFormat> where TFormat : struct, IFixedPointFormat
    {
        // Label for identification when multiplexing timers
        public string Label;

        private FixedPoint<TFormat> duration;
        private FixedPoint<TFormat> elapsed;
        private FixedPoint<TFormat> lastElapsed;

        public bool Loop;
        public int RepeatCount; // 0 = infinite if Loop = true
        private int repeatCounter;

        public bool IsPaused { get; private set; }
        private bool wasFinishedLastFrame;


        /// <summary>
        /// Action à exécuter automatiquement lorsque le timer atteint sa fin (ou boucle).
        /// </summary>
        /// <remarks>
        /// Appelée une seule fois par boucle ou exécution complète, tant que le timer n’est pas en pause.
        /// Peut être utilisée pour déclencher un événement logique ou une animation.
        /// </remarks>

        public Action OnFinish;

        // ========== ETATS DÉRIVÉS ==========
        /// <summary>
        /// Indique si le timer a été initialisé avec une durée strictement positive.
        /// </summary>
        /// <remarks>
        /// Un timer non initialisé ne devrait pas être utilisé avec `Update()`.
        /// </remarks>

        public bool IsInitialized => duration.Raw > 0;

        /// <summary>
        /// Indique si le timer est arrivé à sa fin.
        /// Cela signifie que le temps est écoulé et qu'il n’est ni en boucle ni en répétition restante.
        /// </summary>

        public bool IsFinished => !Loop && RepeatCount <= 0 && elapsed >= duration;

        /// <summary>
        /// Indique que le timer vient tout juste de finir pendant cette frame.
        /// </summary>
        /// <remarks>
        /// Très utile pour déclencher une action une seule fois au moment précis où le timer atteint sa durée.
        /// </remarks>

        public bool DoneThisFrame => !IsPaused && !wasFinishedLastFrame && IsFinishedNow;

        /// <summary>
        /// Indique que le timer est actif (ni en pause ni terminé).
        /// </summary>

        public bool IsRunning => !IsPaused && !IsFinished;

        /// <summary>
        /// Indique que le timer a bouclé pendant cette frame, si `Loop` est actif.
        /// </summary>
        /// <remarks>
        /// Peut être utilisé pour détecter un événement cyclique (animation, boucle de tir, etc.).
        /// </remarks>

        public bool HasJustLooped => Loop && DoneThisFrame && (RepeatCount == 0 || repeatCounter > 0);

        /// <summary>
        /// État interne indiquant si le timer a atteint ou dépassé sa durée actuelle à cette frame.
        /// </summary>
        /// <remarks>
        /// Utilisé en interne pour détecter les transitions (fin, boucle, déclenchement).
        /// Diffère de <see cref="IsFinished"/> car ne prend pas en compte les répétitions ou le mode `Loop`.
        /// </remarks>

        private bool IsFinishedNow => elapsed >= duration;

        /// <summary>
        /// Temps restant avant la fin du timer.
        /// </summary>

        public FixedPoint<TFormat> Remaining => duration - elapsed;

        /// <summary>
        /// Temps déjà écoulé depuis le début ou la dernière boucle du timer.
        /// </summary>

        public FixedPoint<TFormat> Elapsed => elapsed;

        /// <summary>
        /// Renvoie la progression du timer sous forme normalisée (`0.0` à `1.0`).
        /// </summary>
        /// <remarks>
        /// Idéal pour animer une jauge ou une interpolation liée au temps.
        /// </remarks>

        public float Progress01 => Math.Clamp(elapsed.ToFloat() / duration.ToFloat(), 0f, 1f);

        // ========== CONSTRUCTEURS ==========
        /// <summary>
        /// Initialise un timer déterministe avec une durée spécifiée, une option de boucle et un nombre de répétitions.
        /// </summary>
        /// <param name="durationSeconds">Durée du timer en secondes (valeur `float`).</param>
        /// <param name="loop">Détermine si le timer boucle indéfiniment (`true`) ou non (`false`).</param>
        /// <param name="repeatCount">
        /// Nombre de répétitions après la première exécution.
        /// Si `loop` est activé et `repeatCount` vaut `0`, le timer bouclera indéfiniment.
        /// </param>
        /// <param name="label">Nom facultatif du timer, utilisé pour le debug.</param>
        /// <remarks>
        /// La durée est automatiquement convertie en format fixe via <see cref="FixedPoint{TFormat}.FromFloat"/>.
        /// Un avertissement est émis si la durée est nulle ou négative.
        /// </remarks>

        
        public FixedTimer(float durationSeconds, bool loop = false, int repeatCount = 0, string label = "")
        {
            Label = label;
            duration = FixedPoint<TFormat>.FromFloat(durationSeconds);
            elapsed = FixedPoint<TFormat>.FromFloat(0f);
            lastElapsed = elapsed;

            Loop = loop;
            RepeatCount = repeatCount;
            repeatCounter = repeatCount;

            OnFinish = null;
            IsPaused = false;
            wasFinishedLastFrame = false;

            if (duration.Raw <= 0)
                Debug.LogWarning($"[FixedTimer<{typeof(TFormat).Name}>][{Label}] Timer créé avec une durée nulle ou négative.");
        }

        // ========== COMMANDES ==========
        /// <summary>
        /// Redémarre le timer avec une nouvelle durée spécifiée.
        /// </summary>
        /// <param name="newDurationSeconds">Nouvelle durée en secondes (`float`).</param>
        /// <remarks>
        /// Réinitialise également l’état du timer, y compris le compteur de répétition.
        /// N’utiliser que des constantes (`1f / 60f`, etc.) dans un contexte déterministe.
        /// </remarks>

        public void Restart(float newDurationSeconds)
        {
            duration = FixedPoint<TFormat>.FromFloat(newDurationSeconds);
            Reset();
        }

        /// <summary>
        /// Redémarre le timer avec sa durée actuelle.
        /// Réinitialise le temps écoulé, les états internes, et le compteur de répétitions.
        /// </summary>

        public void Restart()
        {
            Reset();
        }

        /// <summary>
        /// Réinitialise le timer à 0 sans changer la durée.
        /// Met en pause = false, réinitialise le compteur de répétitions et l’état de fin.
        /// </summary>

        public void Reset()
        {
            elapsed = FixedPoint<TFormat>.FromFloat(0f);
            lastElapsed = elapsed;
            repeatCounter = RepeatCount;
            IsPaused = false;
            wasFinishedLastFrame = false;
        }

        /// <summary>
        /// Arrête immédiatement le timer.
        /// Met l’état en pause et force `elapsed = duration`.
        /// </summary>
        /// <remarks>
        /// Peut être utilisé pour forcer un arrêt propre sans relancer d’événement `OnFinish`.
        /// </remarks>

        public void Stop()
        {
            IsPaused = true;
            elapsed = duration;
            wasFinishedLastFrame = true;
        }

        /// <summary>
        /// Met le timer en pause.
        /// Aucun temps ne s'écoulera tant que `Resume()` n'est pas appelé.
        /// </summary>

        public void Pause() => IsPaused = true;

        /// <summary>
        /// Reprend le timer s’il était en pause.
        /// </summary>

        public void Resume() => IsPaused = false;

        /// <summary>
        /// Définit une nouvelle durée pour le timer à partir d’une valeur en secondes (`float`).
        /// </summary>
        /// <param name="seconds">Durée en secondes.</param>
        /// <remarks>
        /// La durée est convertie en format fixe. Un avertissement est émis si la valeur est nulle ou négative.
        /// N'affecte pas l’état d’exécution du timer (utiliser <see cref="Restart"/> ou <see cref="Reset"/> si nécessaire).
        /// N’utiliser que des constantes (`1f / 60f`, etc.) dans un contexte déterministe.
        /// </remarks>

        public void SetDuration(float seconds)
        {
            duration = FixedPoint<TFormat>.FromFloat(seconds);
            if (duration.Raw <= 0)
                Debug.LogWarning($"[FixedTimer<{typeof(TFormat).Name}>][{Label}] Durée fixée à 0 ou moins.");
        }

        /// <summary>
        /// Définit une nouvelle durée pour le timer à partir d’une valeur en `FixedPoint`.
        /// </summary>
        /// <param name="d">Durée fixe.</param>
        /// <remarks>
        /// Même comportement que la surcharge `float`, mais sans conversion.
        /// </remarks>

        public void SetDuration(FixedPoint<TFormat> d)
        {
            duration = d;
            if (duration.Raw <= 0)
                Debug.LogWarning($"[FixedTimer<{typeof(TFormat).Name}>][{Label}] Durée fixée à 0 ou moins.");
        }

        // ========== UPDATE ==========
        /// <summary>
        /// Met à jour le timer en ajoutant un pas de temps (`deltaTime`) exprimé en format fixe.
        /// </summary>
        /// <param name="deltaTime">Temps écoulé depuis la dernière frame, en `FixedPoint`.</param>
        /// <remarks>
        /// Déclenche <see cref="OnFinish"/> si le timer atteint sa fin.  
        /// Gère les boucles, les répétitions et l’état interne automatiquement.
        /// Ignoré si le timer est en pause ou non initialisé.
        /// </remarks>

        public void Update(FixedPoint<TFormat> deltaTime)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning($"[FixedTimer<{typeof(TFormat).Name}>][{Label}] Update() appelé sans durée initialisée.");
                return;
            }

            if (IsPaused) return;

            lastElapsed = elapsed;
            elapsed += deltaTime;

            if (IsFinishedNow && !wasFinishedLastFrame)
            {
                OnFinish?.Invoke();

                if (Loop || RepeatCount > 0)
                {
                    elapsed = FixedPoint<TFormat>.FromFloat(0f);

                    if (RepeatCount > 0)
                        repeatCounter--;

                    wasFinishedLastFrame = !(RepeatCount == 0 || repeatCounter > 0);
                }
                else
                {
                    wasFinishedLastFrame = true;
                }
            }
            else if (!IsFinishedNow)
            {
                wasFinishedLastFrame = false;
            }
        }

        /// <summary>
        /// Version non déterministe à éviter : convertit un `float` en `FixedPoint`.
        /// Ne pas utiliser dans un contexte de jeu logique.
        /// </summary>
        /// <param name="deltaTime">Temps écoulé en secondes (`float`).</param>
        /// <remarks>
        /// À utiliser uniquement avec une valeur constante (ex : <c>1f / 60f</c>).
        /// ⚠ Ne jamais utiliser <c>Time.deltaTime</c>, car cela briserait le déterminisme.
        /// </remarks>

        [System.Obsolete("⚠️ N'utilisez pas cette méthode dans un contexte déterministe. Préférez Update(FixedPoint).")]
        public void Update(float deltaTime)
        {
            Update(FixedPoint<TFormat>.FromFloat(deltaTime));
        }

        // ========== UTILITAIRES ==========
        /// <summary>
        /// Indique si un seuil de temps a été franchi entre la frame précédente et la frame actuelle.
        /// </summary>
        /// <param name="secondsThreshold">Temps en secondes à tester (ex. : 2.0f).</param>
        /// <returns>
        /// `true` si le timer vient juste de dépasser le seuil cette frame (strictement après <c>lastElapsed</c> et >= à <c>elapsed</c>).
        /// </returns>
        /// <remarks>
        /// Utile pour déclencher un événement ponctuel à un moment précis pendant l’exécution du timer (ex : une animation intermédiaire).
        /// Fonctionne uniquement si le timer est actif.
        /// N’utiliser que des constantes (`1f / 60f`, etc.) dans un contexte déterministe.
        /// </remarks>

        public bool Triggered(float secondsThreshold)
        {
            var threshold = FixedPoint<TFormat>.FromFloat(secondsThreshold);
            return !IsPaused && elapsed >= threshold && lastElapsed < threshold;
        }


        /// <summary>
        /// Retourne une représentation textuelle du timer, incluant le label, le temps écoulé, la durée, et les états de boucle et pause.
        /// </summary>
        /// <returns>
        /// Une chaîne de type :
        /// <c>[Timer 'Nom']: 1.000s / 5.000s | Loop: true | Paused: false | Repeat: 3</c>
        /// </returns>
        /// <remarks>
        /// Principalement utilisé pour le debug et l’inspection dans l’éditeur.
        /// </remarks>

        public override string ToString()
        {
            return $"[Timer '{Label}']: {elapsed.ToFloat():F3}s / {duration.ToFloat():F3}s | Loop: {Loop} | Paused: {IsPaused} | Repeat: {RepeatCount}";
        }
    }
}
