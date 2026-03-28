using System;
using System.Collections.Generic;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class Model
    {
        private readonly List<IModelSystem> _systems = new();
        private readonly Dictionary<Type, IModelSystem> _typeToSystem = new();
        private readonly List<IGameEntityModel> _entities = new();
        private readonly List<IGameEntityModel> _newEntities = new();

        public ActionScheduler ActionScheduler { get; } = new();
        public Vector2 GameArea { get; set; }
        public int Score { get; set; }

        private GroupCreator _groupCreator;

        public Model()
        {
            // Строгий порядок из ARCHITECTURE.md (Pitfall 4)
            RegisterSystem(new RotateSystem());
            RegisterSystem(new ThrustSystem());
            RegisterSystem(new MoveSystem());
            RegisterSystem(new LifeTimeSystem());
            RegisterSystem(new GunSystem());
            RegisterSystem(new LaserSystem());
            RegisterSystem(new ShootToSystem()); // Phase 4 заглушка
            RegisterSystem(new MoveToSystem());  // Phase 4 заглушка
            _groupCreator = new GroupCreator(this);
        }

        private void RegisterSystem(IModelSystem system)
        {
            _systems.Add(system);
            _typeToSystem[system.GetType()] = system;
        }

        public TSystem GetSystem<TSystem>() where TSystem : IModelSystem
        {
            return (TSystem)_typeToSystem[typeof(TSystem)];
        }

        public void AddEntity(IGameEntityModel entity)
        {
            _newEntities.Add(entity);
        }

        public event Action<IGameEntityModel> OnEntityDestroyed;

        public void Update(float deltaTime)
        {
            ActionScheduler.Update(deltaTime);

            if (_newEntities.Count > 0)
            {
                foreach (var entity in _newEntities)
                {
                    _entities.Add(entity);
                    entity.AcceptWith(_groupCreator);
                }
                _newEntities.Clear();
            }

            foreach (var system in _systems)
            {
                system.Update(deltaTime);
            }

            for (var i = _entities.Count - 1; i >= 0; i--)
            {
                if (_entities[i].IsDead())
                {
                    OnEntityDestroyed?.Invoke(_entities[i]);
                    foreach (var system in _systems) { system.Remove(_entities[i]); }
                    _entities.RemoveAt(i);
                }
            }
        }

        public void CleanUp()
        {
            foreach (var system in _systems) { system.CleanUp(); }
            _entities.Clear();
            _newEntities.Clear();
            ActionScheduler.CleanUp();
            Score = 0;
        }

        // GroupCreator — вложенный класс Visitor для регистрации сущностей в системах
        private class GroupCreator : IGroupVisitor
        {
            private readonly Model _owner;

            public GroupCreator(Model owner) { _owner = owner; }

            public void Visit(ShipModel model)
            {
                _owner.GetSystem<RotateSystem>().Add(model, model.Rotate);
                _owner.GetSystem<ThrustSystem>().Add(model, (model.Thrust, model.Move, model.Rotate));
                _owner.GetSystem<MoveSystem>().Add(model, model.Move);
                _owner.GetSystem<GunSystem>().Add(model, model.Gun);
                _owner.GetSystem<LaserSystem>().Add(model, model.Laser);
            }

            public void Visit(BulletModel model)
            {
                _owner.GetSystem<MoveSystem>().Add(model, model.Move);
                _owner.GetSystem<LifeTimeSystem>().Add(model, model.LifeTime);
            }

            public void Visit(AsteroidModel model)
            {
                _owner.GetSystem<MoveSystem>().Add(model, model.Move);
            }
            public void Visit(UfoBigModel model) { /* Phase 4-5 */ }
        }
    }
}
