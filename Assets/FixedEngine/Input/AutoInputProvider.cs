// File: AutoInputProvider.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FixedEngine;
using FixedEngine.Input;

[DefaultExecutionOrder(-100)]
public class AutoInputProvider : MonoBehaviour
{
    [Tooltip("Asset d'entrée (InputActionAsset)")]
    [SerializeField] private InputActionAsset inputAsset;

    [Tooltip("Liste des noms d'ActionMap à activer (laisser vide = activer toutes les maps)")]
    [SerializeField] private List<string> actionMapNames = new();

    private void Awake()
    {
        if (inputAsset == null)
        {
            Debug.LogError("[AutoInputProvider] Aucun InputActionAsset assigné.");
            enabled = false;
            return;
        }

        // 1) Activation des ActionMaps sur l'InputActionAsset
        if (actionMapNames == null || actionMapNames.Count == 0)
        {
            var maps = inputAsset.actionMaps;
            for (int i = 0; i < maps.Count; i++)
                maps[i].Enable();
        }
        else
        {
            for (int i = 0; i < actionMapNames.Count; i++)
            {
                var map = inputAsset.FindActionMap(actionMapNames[i], true);
                map.Enable();
            }
        }

        // 2) Ajout et configuration du provider générique
        var provider = gameObject.AddComponent<InputSystemProvider<Q8_4>>();
        provider.asset = inputAsset;
        // Creuse une copie de la liste pour éviter les références partagées
        provider.actionMapNames = actionMapNames != null
            ? new List<string>(actionMapNames)
            : new List<string>();

        // 3) Enregistrement du provider dans FixedInput
        FixedInput<Q8_4>.Current = provider;
    }
}
