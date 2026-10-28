using System.Reflection;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    // Edit mode never runs OnEnable/OnDisable on plain MonoBehaviours, so tests that need the wiring call them directly.
    public static class Lifecycle
    {
        public static void Call(MonoBehaviour target, string message) =>
            target.GetType().GetMethod(message, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(target, null);
    }
}
