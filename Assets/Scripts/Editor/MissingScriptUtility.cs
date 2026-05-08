using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptUtility
{
    [MenuItem("Tools/Missing Scripts/Report Missing Scripts")]
    public static void ReportMissingScripts()
    {
        int total = 0;

        foreach (string sceneGuid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes", "Assets/Prefabs" }))
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            total += ReportGameObjects(scenePath, scene.GetRootGameObjects());
        }

        foreach (string prefabGuid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            total += ReportGameObjects(prefabPath, new[] { root });
            PrefabUtility.UnloadPrefabContents(root);
        }

        Debug.Log($"[MissingScriptUtility] Missing script components found: {total}");
    }

    [MenuItem("Tools/Missing Scripts/Remove Missing Scripts")]
    public static void RemoveMissingScripts()
    {
        int total = 0;

        foreach (string sceneGuid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes", "Assets/Prefabs" }))
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            total += RemoveFromGameObjects(scenePath, scene.GetRootGameObjects());
            EditorSceneManager.SaveScene(scene);
        }

        foreach (string prefabGuid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            int removed = RemoveFromGameObjects(prefabPath, new[] { root });
            if (removed > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            PrefabUtility.UnloadPrefabContents(root);
            total += removed;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MissingScriptUtility] Missing script components removed: {total}");
    }

    private static int ReportGameObjects(string assetPath, IEnumerable<GameObject> roots)
    {
        int total = 0;
        foreach (GameObject root in roots)
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                if (count <= 0)
                {
                    continue;
                }

                total += count;
                Debug.LogWarning($"[MissingScriptUtility] {assetPath} :: {GetPath(transform)} has {count} missing script component(s).", transform.gameObject);
            }
        }

        return total;
    }

    private static int RemoveFromGameObjects(string assetPath, IEnumerable<GameObject> roots)
    {
        int total = 0;
        foreach (GameObject root in roots)
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                if (count <= 0)
                {
                    continue;
                }

                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
                total += count;
                Debug.LogWarning($"[MissingScriptUtility] Removed {count} missing script component(s) from {assetPath} :: {GetPath(transform)}.", transform.gameObject);
            }
        }

        return total;
    }

    private static string GetPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }
}
