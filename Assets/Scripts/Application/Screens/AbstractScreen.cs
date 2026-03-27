using Shtl.Mvvm;

namespace SelStrom.Asteroids
{
    public abstract class AbstractScreen
    {
        protected EventBindingContext Bind { get; } = new();

        public virtual void Dispose()
        {
            Bind.CleanUp();
        }
    }
}
