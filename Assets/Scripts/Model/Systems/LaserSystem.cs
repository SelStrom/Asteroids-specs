using Model.Components;

namespace SelStrom.Asteroids
{
    public class LaserSystem : BaseModelSystem<LaserComponent>
    {
        protected override void UpdateNode(IGameEntityModel entity, LaserComponent laser, float deltaTime)
        {
            // Восстановление зарядов: каждые LaserUpdateDurationSec добавляем 1 заряд
            if (laser.CurrentShoots.Value < laser.MaxShoots)
            {
                laser.ReloadTimeLeft.Value += deltaTime;
                if (laser.ReloadTimeLeft.Value >= laser.LaserUpdateDurationSec)
                {
                    laser.ReloadTimeLeft.Value -= laser.LaserUpdateDurationSec;
                    laser.CurrentShoots.Value = System.Math.Min(laser.CurrentShoots.Value + 1, laser.MaxShoots);
                }
            }

            // Выстрел лазером
            if (laser.IsLaserFiring && laser.CurrentShoots.Value > 0)
            {
                laser.CurrentShoots.Value--;
                laser.OnLaserFired?.Invoke(laser);
            }
            laser.IsLaserFiring = false; // сброс флага
        }
    }
}
