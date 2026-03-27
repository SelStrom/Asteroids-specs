using System.Collections.Generic;

namespace SelStrom.Asteroids
{
    public interface IModelSystem
    {
        void Update(float deltaTime);
        void Remove(IGameEntityModel entity);
        void CleanUp();
    }

    public abstract class BaseModelSystem<TNode> : IModelSystem
    {
        protected readonly Dictionary<IGameEntityModel, TNode> _nodes = new();

        public void Add(IGameEntityModel entity, TNode node)
        {
            _nodes[entity] = node;
        }

        public void Update(float deltaTime)
        {
            foreach (var pair in _nodes)
            {
                UpdateNode(pair.Key, pair.Value, deltaTime);
            }
        }

        protected abstract void UpdateNode(IGameEntityModel entity, TNode node, float deltaTime);

        public void Remove(IGameEntityModel entity)
        {
            _nodes.Remove(entity);
        }

        public void CleanUp()
        {
            _nodes.Clear();
        }
    }
}
