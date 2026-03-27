using Shtl.Mvvm;
using UnityEngine;

namespace SelStrom.Asteroids.Bindings
{
    public static class BindingToExtensions
    {
        // Биндинг ObservableValue<Vector2> → Transform.position
        public static void ToPosition(this BindFrom<ObservableValue<Vector2>> binding, Transform transform)
        {
            binding.To(val => transform.position = new Vector3(val.x, val.y, transform.position.z));
        }

        // Биндинг ObservableValue<float> → Transform.eulerAngles.z (для вращения)
        public static void ToRotation(this BindFrom<ObservableValue<float>> binding, Transform transform)
        {
            binding.To(val => transform.eulerAngles = new Vector3(0f, 0f, val));
        }

        // Биндинг ObservableValue<bool> → SpriteRenderer.enabled
        public static void ToEnabled(this BindFrom<ObservableValue<bool>> binding, SpriteRenderer spriteRenderer)
        {
            binding.To(val => spriteRenderer.enabled = val);
        }
    }
}
