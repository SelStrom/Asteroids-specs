using System;

namespace SelStrom.Asteroids
{
    public class PlayerInput : IDisposable
    {
        private PlayerActions _actions;

        // Публичные события (D-11)
        public event Action<float> OnRotateAction;       // -1 влево, +1 вправо
        public event Action<bool> OnTrustAction;         // true = тяга нажата, false = отпущена
        public event Action OnAttackAction;              // выстрел (Space / ЛКМ)
        public event Action OnLaserAction;               // лазер (Q)
        public event Action OnBackAction;                // назад (Escape)

        public void Connect()
        {
            _actions = new PlayerActions();
            _actions.Enable();

            _actions.Player.Rotate.performed += ctx => OnRotateAction?.Invoke(ctx.ReadValue<float>());
            _actions.Player.Rotate.canceled  += ctx => OnRotateAction?.Invoke(0f);

            _actions.Player.Thrust.started   += ctx => OnTrustAction?.Invoke(true);
            _actions.Player.Thrust.canceled  += ctx => OnTrustAction?.Invoke(false);

            _actions.Player.Attack.performed  += ctx => OnAttackAction?.Invoke();
            _actions.Player.Laser.performed   += ctx => OnLaserAction?.Invoke();
            _actions.Player.Back.performed    += ctx => OnBackAction?.Invoke();
        }

        public void Dispose()
        {
            _actions?.Disable();
            _actions?.Dispose();
            _actions = null;
        }
    }
}
