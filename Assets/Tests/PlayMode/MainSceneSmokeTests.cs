using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace PocketPlanet.PlayTests
{
    public class MainSceneSmokeTests
    {
        const string MainScene = "Assets/Scenes/Main.unity";
        const int FramesToRun = 120;

        readonly List<string> errors = new List<string>();

        void Record(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
                errors.Add($"{message}\n{stackTrace}");
        }

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator Main_PlaysWithoutCameraErrors()
        {
            LogAssert.ignoreFailingMessages = true;
            Application.logMessageReceived += Record;
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode(MainScene, new LoadSceneParameters(LoadSceneMode.Single));
                for (int i = 0; i < FramesToRun; i++)
                    yield return null;
            }
            finally
            {
                Application.logMessageReceived -= Record;
            }

            foreach (var error in errors.Distinct())
                Debug.Log($"Main scene error: {error}");
            var cameraErrors = errors.Where(e => e.Contains("Camera")).Distinct().ToList();
            Assert.IsEmpty(cameraErrors, string.Join("\n", cameraErrors));
        }
#endif
    }
}
