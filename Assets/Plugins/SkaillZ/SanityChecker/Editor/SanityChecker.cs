using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Skaillz.SanityChecker.Attributes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Skaillz.SanityChecker.Editor
{
    public static class SanityChecker
    {
        private const string DefaultTestFilePath = "Assets/Plugins/SkaillZ/SanityChecker/Editor/SanityCheckerTests.cs";
        private const string TestFileClassName = "SanityCheckerTests";

        private static Dictionary<Type, ISanityCheck> checks = new Dictionary<Type, ISanityCheck>();

        [MenuItem("Tools/Sanity Checker/Run Checks in Build Scenes and Prefabs %&#c", false, 0)]
        private static void RunChecksInBuildScenesAndPrefabsMenuItem()
        {
            RunChecksInBuildScenes(interactiveMode: true);
            RunChecksInPrefabs(interactiveMode: true);
            Debug.Log("[SanityChecker] All checks completed.");
        }

        [MenuItem("Tools/Sanity Checker/Run Checks in Current Scenes %&c", false, 1)]
        private static void RunChecksInCurrentScenesMenuItem()
        {
            RunChecksInCurrentScenes(interactiveMode: true);
            Debug.Log("[SanityChecker] All checks completed.");
        }
        
        [MenuItem("Tools/Sanity Checker/Run Checks in Build Scenes %&s", false, 100)]
        private static void RunChecksInBuildScenesMenuItem()
        {
            RunChecksInBuildScenes(interactiveMode: true);
            Debug.Log("[SanityChecker] All checks completed.");
        }
        
        [MenuItem("Tools/Sanity Checker/Run Checks in Prefabs %&p", false, 101)]
        private static void RunChecksInPrefabsMenuItem()
        {
            RunChecksInPrefabs(interactiveMode: true);
            Debug.Log("[SanityChecker] All checks completed.");
        }
        
        [MenuItem("Tools/Sanity Checker/Enable Editor Tests...", false, 200)]
        private static void EditorTestInfoMenuItem()
        {
            bool openFile = EditorUtility.DisplayDialog("Sanity Checker",
                "To enable automated EditMode tests, open the file '" + DefaultTestFilePath + "' in your text editor or IDE and delete the first and last lines starting with '#'."
                + "The tests should then show up in the Test Runner.",
                ok: "Open the source file",
                cancel: "Cancel");

            if (openFile)
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(DefaultTestFilePath);
                if (script == null)
                {
                    EditorUtility.DisplayDialog("Sanity Checker", "Couldn't open the source file at '" + DefaultTestFilePath + "'. Please find and edit it manually if you moved it.", "OK");
                    return;
                }
                AssetDatabase.OpenAsset(script, 1);
            }
        }
        
        [MenuItem("Tools/Sanity Checker/Enable Editor Tests...", true, 200)]
        private static bool ValidateEditorTestInfoMenuItem() // Validator for EditorTestInfoMenuItem()
        {
            // ReSharper disable once SimplifyLinqExpression
            try
            {
                return !Assembly.GetExecutingAssembly().GetTypes().Any(type =>
                    type.FullName == typeof(SanityChecker).Namespace + "." + TestFileClassName); // The test class should be in the same namespace
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void RunChecksInCurrentScenes(bool interactiveMode = false)
        {
            RunChecksInScenes(GetActiveScenes().Select(scene => scene.path).ToList(), interactiveMode);
        }

        public static void RunChecksInBuildScenes(bool interactiveMode = false)
        {
            var buildScenes = GetBuildScenes();
            if (buildScenes.Count == 0 && interactiveMode)
            {
                EditorUtility.DisplayDialog("Sanity Checker", "There aren't any scenes configured in your build settings. Please add some scenes to your build an try again.", "OK");
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return; // Don't check the other scenes if the user doesn't want to save
            
            // Save the currently open scene paths to restore them later
            var previousScenes = GetActiveScenes().Select(scene => scene.path).ToArray();
            
            RunChecksInScenes(buildScenes, interactiveMode);
            
            // Restore previously open scenes
            if (!interactiveMode)
                return;
            
            EditorUtility.DisplayProgressBar("Sanity Checker", "Reopening scenes...", 1f);
            try
            {
                var firstScene = previousScenes.FirstOrDefault();
                if (!string.IsNullOrEmpty(firstScene))
                {
                    EditorSceneManager.OpenScene(firstScene);
                    foreach (var previousScene in previousScenes.Skip(1))
                    {
                        EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Additive);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void RunChecksInPrefabs(bool interactiveMode = false)
        {
            try
            {
                if (interactiveMode)
                    EditorUtility.DisplayProgressBar("Sanity Checker", "Finding Prefabs...", 0f);

                var prefabGUIDs = AssetDatabase.FindAssets("t:GameObject");
                for (int i = 0; i < prefabGUIDs.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]);

                    if (interactiveMode)
                    {
                        if (ShowPrefabProgress(path, i, prefabGUIDs.Length))
                            throw new UserInterruptedException();
                    }

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    foreach (var monoBehaviour in prefab.GetComponentsInChildren<MonoBehaviour>())
                    {
                        PerformChecks(monoBehaviour, interactiveMode, monoBehaviour.gameObject);
                    }
                }
            }
            catch (UserInterruptedException)
            {
                if (interactiveMode)
                    Debug.LogWarning("[SanityChecker] Checks interrupted by user.");
            }
            finally
            {
                if (interactiveMode)
                    EditorUtility.ClearProgressBar();
            }
        }
        
        public static void RunChecksInScenes(List<string> scenePaths, bool interactiveMode = false)
        {
            try
            {
                for (int i = 0; i < scenePaths.Count; i++)
                {
                        var scenePath = scenePaths[i];
                        var loadedScene = GetActiveScenes().FirstOrDefault(scene => scene.path == scenePath);
                        if (loadedScene == default(Scene))
                        {
                            // The scene isn't actually loaded and we have to load it in the editor manually
                            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                throw new UserInterruptedException();
                            
                            loadedScene = EditorSceneManager.OpenScene(scenePath);
                        }
                        CheckScene(loadedScene, interactiveMode, currentSceneIndex: i, totalScenes: scenePaths.Count);
                    }
                }
            catch (UserInterruptedException)
            {
                if (interactiveMode)
                    Debug.LogWarning("[SanityChecker] Checks interrupted by user.");
            }
            finally
            {
                if (interactiveMode)
                    EditorUtility.ClearProgressBar();
            }
        }

        public static void CheckScene(Scene scene, bool interactiveMode = false, int currentSceneIndex = 0, int totalScenes = 0)
        {
            if (interactiveMode)
                ShowGatheringReferencesMessage(scene.name, currentSceneIndex, totalScenes);
            
            var allMonos = scene.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<MonoBehaviour>()).ToArray();

            for (int i = 0; i < allMonos.Length; i++)
            {
                var monoBehaviour = allMonos[i];
                if (interactiveMode)
                {
                    if (ShowSceneProgress(scene.name, currentSceneIndex, totalScenes, i, allMonos.Length))
                        throw new UserInterruptedException();
                }
                PerformChecks(monoBehaviour, interactiveMode, monoBehaviour.gameObject);
            }
            
            if (interactiveMode)
                EditorUtility.ClearProgressBar();
        }

        public static void PerformChecks(object obj, bool interactiveMode = false, object context = null)
        {
            if (checks == null || checks.Count == 0)
            {
                throw new Exception(
                    "[SanityChecker] No checks were loaded. You might have to restart the Unity editor to fix this.");
            }
            
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                try
                {
                    if (field.GetCustomAttributes(typeof(CheckInsideAttribute), true).FirstOrDefault() != null)
                    {
                        PerformChecks(field.GetValue(obj), interactiveMode, context); // Check inside the object with the attribute recursively
                    }

                    foreach (var attribute in field.GetCustomAttributes(true))
                    {
                        var attributeType = attribute.GetType();
                        if (checks.ContainsKey(attributeType))
                        {
                            checks[attributeType].Check(obj, field, (Attribute) attribute, context);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!interactiveMode)
                        throw; // Just throw the exception again if this is an automated test

                    // Provide context if the check was started interactively
                    if (obj is MonoBehaviour)
                    {
                        var gameObject = ((MonoBehaviour) obj).gameObject;
                        Debug.LogException(e, gameObject); // Enables Unity to highlight the containing prefab when clicked on
                    }
                    else
                    {
                        Debug.LogException(e, obj as UnityEngine.Object);
                    }
                }
            }
        }

        public static void RegisterCheck<TAttributeType>(ISanityCheck check) where TAttributeType : Attribute
        {
            checks.Remove(typeof(TAttributeType));
            checks.Add(typeof(TAttributeType), check);
        }

        private static List<Scene> GetActiveScenes()
        {
            var scenes = new List<Scene>();
            for (int i = 0; i < EditorSceneManager.loadedSceneCount; i++)
            {
               scenes.Add(EditorSceneManager.GetSceneAt(i));
            }
            return scenes;
        }
        
        private static List<string> GetBuildScenes()
        {
            var scenes = new List<string>();
            for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i++)
            {
                scenes.Add(SceneUtility.GetScenePathByBuildIndex(i));
            }
            return scenes;
        }

        private static bool ShowSceneProgress(string sceneName, int currentSceneIndex, int totalNumberOfScenes, int currentMonoIndex, int totalNumberOfMonos)
        {
            float percentage = (float) currentMonoIndex / totalNumberOfMonos;
            return EditorUtility.DisplayCancelableProgressBar("Sanity Checker", "Processing scene '" + sceneName + "'... (" + (currentSceneIndex + 1) + " of " + totalNumberOfScenes + ", " + Mathf.RoundToInt(percentage * 100f) + "%)",
                percentage);
        }
        
        private static void ShowGatheringReferencesMessage(string sceneName, int currentSceneIndex, int totalNumberOfScenes, float displayedProgress = 0f)
        {
            EditorUtility.DisplayProgressBar("Sanity Checker", "Gathering references for scene '" + sceneName + "'... (" + (currentSceneIndex + 1)+ " of " + totalNumberOfScenes + ")", displayedProgress * 100);
        }
        
        private static bool ShowPrefabProgress(string path, int currentPrefabIndex, int totalNumberOfPrefabs)
        {
            float percentage = (float) currentPrefabIndex / totalNumberOfPrefabs;
            return EditorUtility.DisplayCancelableProgressBar("Sanity Checker", "Processing prefab '" + path + "'... (" + (currentPrefabIndex + 1) + " of " + totalNumberOfPrefabs + ", " + Mathf.RoundToInt(percentage * 100f) + "%)",
                percentage);
        }

        internal static string CreateExceptionMessage(FieldInfo field, object offendingObject, string message,
            object context)
        {
            var sb = new StringBuilder();
            sb.Append("[SanityChecker] The field '" + field.Name + "' on '" + offendingObject.GetType().FullName +
                          "' " + message);
            
            if (context is GameObject)
            {
                var go = (GameObject) context;
                sb.AppendLine();

                switch (PrefabUtility.GetPrefabType(go))
                {
                    case PrefabType.None: // The context object lives in a scene
                    case PrefabType.MissingPrefabInstance:
                    case PrefabType.DisconnectedPrefabInstance:
                        sb.Append("The offending object is located inside ");
                        sb.Append(!string.IsNullOrEmpty(go.scene.path) ? "Scene '<i>" + go.scene.path + "</i>' " : "your currently unsaved scene");
                        sb.Append(" at the following path: <b>" + GetGameObjectPath(go) + "</b>");
                        break;
                    case PrefabType.Prefab:
                        sb.Append("The offending prefab is located at: <b>" + AssetDatabase.GetAssetPath(go) + "</b>");
                        break;
                    case PrefabType.PrefabInstance:
                        sb.Append("The offending object is located inside ");
                        sb.Append(!string.IsNullOrEmpty(go.scene.path) ? "Scene '<i>" + go.scene.path + "</i>' " : "your currently unsaved scene");
                        sb.Append(" at the following path: <b>" + GetGameObjectPath(go) + "</b>. ");
                        sb.Append("The prefab this object is linked to is located at: <b>" + AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(go)) + "</b>");
                        break;
                }
            }
            else if (context is ScriptableObject)
            {
                // TODO: implement
                sb.AppendLine();
            }
            else if (context != null)
            {
                sb.AppendLine();
                sb.Append("Context: ");
                sb.Append(context);
            }

            return sb.ToString();
        }
        
        private static string GetGameObjectPath(GameObject current) {
            if (current.transform.parent == null)
                return "/" + current.name;
            return GetGameObjectPath(current.transform.parent.gameObject) + "/" + current.name;
        }

        private class UserInterruptedException : Exception {}
    }
}