using System;
using System.Collections;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public interface IApplicationComponent
    {
        event Action<float> OnUpdate;
        event Action OnPause;
        event Action OnResume;

        // Используется Application для запуска Coroutine через MonoBehaviour (D-09)
        Coroutine StartCoroutine(IEnumerator routine);
    }
}
