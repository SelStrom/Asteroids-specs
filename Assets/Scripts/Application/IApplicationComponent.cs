using System;

namespace SelStrom.Asteroids
{
    public interface IApplicationComponent
    {
        event Action<float> OnUpdate;
        event Action OnPause;
        event Action OnResume;
    }
}
