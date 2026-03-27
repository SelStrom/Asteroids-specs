// ВНИМАНИЕ: Этот файл должен быть сгенерирован Unity Input System.
// Если Unity Editor доступен, выбрать Assets/Input/PlayerActions.inputactions →
// Inspector → Generate C# Class → путь: Assets/Scripts/Input/Generated/PlayerActions.cs
// В противном случае — минимальная ручная реализация:
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace SelStrom.Asteroids
{
    public partial class PlayerActions : IInputActionCollection2, System.IDisposable
    {
        private InputActionAsset _asset;
        private PlayerMap _playerMap;

        public PlayerActions()
        {
            _asset = InputActionAsset.FromJson(LoadAssetJson());
            _playerMap = new PlayerMap(_asset.FindActionMap("Player", throwIfNotFound: true));
        }

        private static string LoadAssetJson()
        {
            // Загружаем .inputactions из Resources или через AssetDatabase в Editor
            var asset = Resources.Load<InputActionAsset>("PlayerActions");
            if (asset != null) { return asset.ToJson(); }
            // Fallback — пустой JSON
            return "{\"name\":\"PlayerActions\",\"maps\":[{\"name\":\"Player\",\"actions\":[],\"bindings\":[]}]}";
        }

        public PlayerMap Player => _playerMap;

        public void Enable() { _asset.Enable(); }
        public void Disable() { _asset.Disable(); }
        public void Dispose() { _asset?.Dispose(); }

        // IInputActionCollection2 members
        public InputBinding? bindingMask { get => _asset.bindingMask; set => _asset.bindingMask = value; }
        public ReadOnlyArray<InputDevice>? devices { get => _asset.devices; set => _asset.devices = value; }
        public ReadOnlyArray<InputControlScheme> controlSchemes => _asset.controlSchemes;
        public bool Contains(InputAction action) => _asset.Contains(action);
        public IEnumerator<InputAction> GetEnumerator() => _asset.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        public InputActionRebindingExtensions.RebindingOperation PerformInteractiveRebinding(InputAction action, int bindingIndex = -1) => action.PerformInteractiveRebinding(bindingIndex);

        public struct PlayerMap
        {
            private InputActionMap _map;
            public InputAction Rotate;
            public InputAction Thrust;
            public InputAction Attack;
            public InputAction Laser;
            public InputAction Back;

            public PlayerMap(InputActionMap map)
            {
                _map = map;
                Rotate = map.FindAction("Rotate", throwIfNotFound: true);
                Thrust = map.FindAction("Thrust", throwIfNotFound: true);
                Attack = map.FindAction("Attack", throwIfNotFound: true);
                Laser  = map.FindAction("Laser", throwIfNotFound: true);
                Back   = map.FindAction("Back", throwIfNotFound: true);
            }

            public void Enable() => _map.Enable();
            public void Disable() => _map.Disable();
        }
    }
}
