using EasyFramework;
using UnityEngine;

namespace EasyFramework.Samples.Basics
{
    public readonly struct LoginCompleted { }

    public sealed class EventExample : MonoBehaviour
    {
        private void OnEnable() => F.Event.Add<LoginCompleted>(OnLogin);
        private void OnDisable() => F.Event.Remove<LoginCompleted>(OnLogin);
        private void Start() => F.Event.Invoke(new LoginCompleted());
        private void OnLogin(LoginCompleted args) => Debug.Log("LoginCompleted received");
    }
}
