using System;
using System.Collections.Generic;
using Shtl.Mvvm;
using SelStrom.Asteroids.Configs;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class EntitiesCatalog
    {
        private GameData _configs;
        private ModelFactory _modelFactory;
        private GameObjectPool _pool;

        private readonly Dictionary<IGameEntityModel, IEntityView> _modelToView = new();
        private readonly Dictionary<IGameEntityModel, EventBindingContext> _modelToBind = new();
        private readonly Dictionary<IGameEntityModel, GameObject> _modelToGo = new();
        private readonly Dictionary<GameObject, IGameEntityModel> _goToModel = new();
        private readonly Dictionary<int, GameObject> _prefabRegistry = new(); // prefabId → prefab

        public ShipModel Ship { get; private set; }

        public event Action<BulletModel> OnBulletCreated;

        public void Connect(GameData configs, ModelFactory modelFactory, GameObjectPool pool)
        {
            _configs = configs;
            _modelFactory = modelFactory;
            _pool = pool;

            // Регистрация prefabs в пуле
            RegisterPrefab(_configs.Ship.Prefab);
            RegisterPrefab(_configs.Bullet.Prefab);
            RegisterPrefab(_configs.Bullet.EnemyPrefab);
            RegisterPrefab(_configs.AsteroidBig.Prefab);
            RegisterPrefab(_configs.AsteroidMedium.Prefab);
            RegisterPrefab(_configs.AsteroidSmall.Prefab);
        }

        private void RegisterPrefab(GameObject prefab)
        {
            if (prefab == null) { return; }
            _pool.Register(prefab);
            _prefabRegistry[prefab.GetInstanceID()] = prefab;
        }

        public ShipModel CreateShip()
        {
            // 1. Model
            var model = _modelFactory.Create<ShipModel>();
            model.Setup(_configs);
            model.Reset();

            // 2. View
            var view = _pool.Get<ShipVisual>(_configs.Ship.Prefab);
            view.SetPrefabId(_configs.Ship.Prefab.GetInstanceID());

            // 3. ViewModel + Bindings
            var vm = new ShipViewModel();
            var bind = new EventBindingContext();
            // Биндинг Model.Position → ViewModel.Position (ObservableValue → ReactiveValue)
            bind.From(model.Move.Position).To(vm.Position);
            // Биндинг Model.Thrust.IsActive → ViewModel.IsThrusting
            bind.From(model.Thrust.IsActive).To(vm.IsThrusting);
            // Биндинг Model.Direction → ViewModel.Rotation (угол в градусах)
            bind.From(model.Move.Direction).To(val =>
                vm.Rotation.Value = Mathf.Atan2(val.y, val.x) * Mathf.Rad2Deg);

            vm.IsVisible.Value = true;

            // Соединить view с viewmodel
            view.Connect(vm);

            // Принудительно применить начальные значения (Reset() вызывается до биндинга)
            bind.InvokeAll();

            // 4. Register
            _modelToView[model] = view;
            _modelToBind[model] = bind;
            _modelToGo[model] = view.gameObject;
            _goToModel[view.gameObject] = model;

            Ship = model;
            return model;
        }

        public BulletModel CreateBullet(Vector2 position, Vector2 velocity, bool isEnemy = false)
        {
            var prefab = isEnemy ? _configs.Bullet.EnemyPrefab : _configs.Bullet.Prefab;

            // 1. Model
            var model = _modelFactory.Create<BulletModel>();
            model.IsEnemy = isEnemy;
            model.Setup(_configs.Bullet.LifeTimeSeconds);
            model.Move.Position.Value = position;
            model.Move.Direction.Value = velocity.normalized;
            model.Move.Speed.Value = velocity.magnitude;

            // 2. View
            var view = _pool.Get<BulletVisual>(prefab);
            view.SetPrefabId(prefab.GetInstanceID());

            // 3. ViewModel + Bindings
            var vm = new BulletViewModel();
            var bind = new EventBindingContext();
            bind.From(model.Move.Position).To(vm.Position);

            view.Connect(vm);

            // 4. Register
            _modelToView[model] = view;
            _modelToBind[model] = bind;
            _modelToGo[model] = view.gameObject;
            _goToModel[view.gameObject] = model;

            OnBulletCreated?.Invoke(model);
            return model;
        }

        public AsteroidModel CreateAsteroid(AsteroidData data, Vector2 position, Vector2 velocity)
        {
            // 1. Model
            var model = _modelFactory.Create<AsteroidModel>();
            model.Size = data == _configs.AsteroidBig ? 3
                       : data == _configs.AsteroidMedium ? 2
                       : 1;
            model.AngularSpeed = UnityEngine.Random.Range(30f, 120f)
                               * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
            model.Move.Position.Value = position;
            model.Move.Direction.Value = velocity.normalized;
            model.Move.Speed.Value = velocity.magnitude;

            // 2. View
            var view = _pool.Get<AsteroidVisual>(data.Prefab);
            view.SetPrefabId(data.Prefab.GetInstanceID());
            view.SetAngularSpeed(model.AngularSpeed);

            // 3. Выбрать случайный спрайт из SpriteVariants
            var sprite = data.SpriteVariants != null && data.SpriteVariants.Length > 0
                ? data.SpriteVariants[UnityEngine.Random.Range(0, data.SpriteVariants.Length)]
                : null;

            // 4. ViewModel + Bindings
            var vm = new AsteroidViewModel();
            var bind = new EventBindingContext();
            bind.From(model.Move.Position).To(vm.Position);
            vm.Sprite.Value = sprite;

            view.Connect(vm);

            // 5. Register
            _modelToView[model] = view;
            _modelToBind[model] = bind;
            _modelToGo[model] = view.gameObject;
            _goToModel[view.gameObject] = model;

            return model;
        }

        public void Release(IGameEntityModel model)
        {
            if (_modelToBind.TryGetValue(model, out var bind))
            {
                bind.CleanUp(); // КРИТИЧНО: снять биндинги model→vm (Pitfall 1)
                _modelToBind.Remove(model);
            }

            if (_modelToView.TryGetValue(model, out var view))
            {
                var go = view.gameObject;
                var prefabId = view.PrefabInstanceId;

                // Снять биндинги vm→view и отсоединить ViewModel
                if (view is ShipVisual sv) { sv.Dispose(); }
                else if (view is BulletVisual bv) { bv.Dispose(); }
                else if (view is AsteroidVisual av) { av.Dispose(); }

                if (_prefabRegistry.TryGetValue(prefabId, out var prefab))
                {
                    _pool.Release(go, prefab);
                }
                _goToModel.Remove(go);
                _modelToView.Remove(model);
                _modelToGo.Remove(model);
            }

            if (model is ShipModel) { Ship = null; }
        }

        public IGameEntityModel GetModelByGo(GameObject go)
        {
            _goToModel.TryGetValue(go, out var model);
            return model;
        }

        public IEntityView GetViewByModel(IGameEntityModel model)
        {
            _modelToView.TryGetValue(model, out var view);
            return view;
        }

        public void Reset()
        {
            // Очистить словари от мёртвых ключей предыдущей сессии.
            // Pool и prefabRegistry НЕ трогать — объекты возвращены в pool через Release() или CleanUp().
            _modelToView.Clear();
            _modelToBind.Clear();
            _modelToGo.Clear();
            _goToModel.Clear();
        }

        public void Dispose()
        {
            foreach (var bind in _modelToBind.Values) { bind.CleanUp(); }
            _modelToView.Clear();
            _modelToBind.Clear();
            _modelToGo.Clear();
            _goToModel.Clear();
            Ship = null;
        }
    }
}
