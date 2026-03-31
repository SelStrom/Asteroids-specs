// ВНИМАНИЕ: Этот файл должен быть сгенерирован Unity Input System.
// Если Unity Editor доступен, выбрать Assets/Input/PlayerActions.inputactions →
// Inspector → Generate C# Class → путь: Assets/Scripts/Input/Generated/PlayerActions.cs
// В противном случае — минимальная ручная реализация:
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
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
#if UNITY_EDITOR
            var editorAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Input/PlayerActions.inputactions");
            if (editorAsset != null) { return editorAsset.ToJson(); }
#endif
            var asset = Resources.Load<InputActionAsset>("PlayerActions");
            if (asset != null) { return asset.ToJson(); }
            return EmbeddedJson;
        }

        private const string EmbeddedJson = @"{
    ""name"": ""PlayerActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""a1b2c3d4-0000-0000-0000-000000000001"",
            ""actions"": [
                { ""name"": ""Rotate"", ""type"": ""Value"", ""id"": ""a1b2c3d4-0000-0000-0000-000000000002"", ""expectedControlType"": ""Axis"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Thrust"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0000-0000-0000-000000000003"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Attack"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0000-0000-0000-000000000004"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Laser"",  ""type"": ""Button"", ""id"": ""a1b2c3d4-0000-0000-0000-000000000005"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Back"",   ""type"": ""Button"", ""id"": ""a1b2c3d4-0000-0000-0000-000000000006"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false }
            ],
            ""bindings"": [
                { ""name"": ""1D Axis"",  ""id"": ""a1b2c3d4-0000-0000-0000-000000000010"", ""path"": ""1DAxis"",              ""action"": ""Rotate"", ""isComposite"": true,  ""isPartOfComposite"": false },
                { ""name"": ""negative"", ""id"": ""a1b2c3d4-0000-0000-0000-000000000011"", ""path"": ""<Keyboard>/a"",        ""action"": ""Rotate"", ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": ""positive"", ""id"": ""a1b2c3d4-0000-0000-0000-000000000012"", ""path"": ""<Keyboard>/d"",        ""action"": ""Rotate"", ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": ""negative"", ""id"": ""a1b2c3d4-0000-0000-0000-000000000013"", ""path"": ""<Keyboard>/leftArrow"",  ""action"": ""Rotate"", ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": ""positive"", ""id"": ""a1b2c3d4-0000-0000-0000-000000000014"", ""path"": ""<Keyboard>/rightArrow"", ""action"": ""Rotate"", ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": """",         ""id"": ""a1b2c3d4-0000-0000-0000-000000000020"", ""path"": ""<Keyboard>/w"",        ""action"": ""Thrust"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",         ""id"": ""a1b2c3d4-0000-0000-0000-000000000021"", ""path"": ""<Keyboard>/upArrow"",  ""action"": ""Thrust"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",         ""id"": ""a1b2c3d4-0000-0000-0000-000000000030"", ""path"": ""<Keyboard>/space"",    ""action"": ""Attack"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",         ""id"": ""a1b2c3d4-0000-0000-0000-000000000031"", ""path"": ""<Mouse>/leftButton"",  ""action"": ""Attack"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",         ""id"": ""a1b2c3d4-0000-0000-0000-000000000040"", ""path"": ""<Keyboard>/q"",        ""action"": ""Laser"",  ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",         ""id"": ""a1b2c3d4-0000-0000-0000-000000000050"", ""path"": ""<Keyboard>/escape"",   ""action"": ""Back"",   ""isComposite"": false, ""isPartOfComposite"": false }
            ]
        }
    ],
    ""controlSchemes"": []
}";

        public PlayerMap Player => _playerMap;

        public void Enable() { _asset.Enable(); }
        public void Disable() { _asset.Disable(); }
        public void Dispose() { if (_asset != null) { Object.Destroy(_asset); } }

        // IInputActionCollection2 members
        public InputBinding? bindingMask { get => _asset.bindingMask; set => _asset.bindingMask = value; }
        public ReadOnlyArray<InputDevice>? devices { get => _asset.devices; set => _asset.devices = value; }
        public ReadOnlyArray<InputControlScheme> controlSchemes => _asset.controlSchemes;
        public bool Contains(InputAction action) => _asset.Contains(action);
        public IEnumerator<InputAction> GetEnumerator() => _asset.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        public InputActionRebindingExtensions.RebindingOperation PerformInteractiveRebinding(InputAction action, int bindingIndex = -1) => action.PerformInteractiveRebinding(bindingIndex);
        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false) => _asset.FindAction(actionNameOrId, throwIfNotFound);
        public int FindBinding(InputBinding mask, out InputAction action) => _asset.FindBinding(mask, out action);
        public IEnumerable<InputBinding> bindings => _asset.bindings;

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
