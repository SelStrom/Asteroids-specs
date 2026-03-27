namespace SelStrom.Asteroids
{
    public class ModelFactory
    {
        private Model _model;

        public void Connect(Model model)
        {
            _model = model;
        }

        public TModel Create<TModel>() where TModel : IGameEntityModel, new()
        {
            var model = new TModel();
            _model.AddEntity(model);
            return model;
        }

        public void Release(IGameEntityModel model)
        {
            // TODO @a.shatalov: model pool
        }
    }
}
