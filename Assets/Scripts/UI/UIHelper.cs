using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class UIHelper
{
    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public static Transform FindChildRecursive(Transform parent, string targetName)
    {
        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == targetName)
            {
                return child;
            }

            Transform nestedChild = FindChildRecursive(child, targetName);
            if (nestedChild != null)
            {
                return nestedChild;
            }
        }
        return null;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public static Transform FindInActiveScene(string targetName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject.name == targetName)
            {
                return rootObject.transform;
            }

            Transform nestedChild = FindChildRecursive(rootObject.transform, targetName);
            if (nestedChild != null)
            {
                return nestedChild;
            }
        }
        return null;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public static List<Transform> FindAllInActiveScene(string targetName)
    {
        List<Transform> results = new List<Transform>();
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject.name == targetName)
            {
                results.Add(rootObject.transform);
            }

            FindAllChildrenRecursive(rootObject.transform, targetName, results);
        }

        return results;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    private static void FindAllChildrenRecursive(Transform parent, string targetName, List<Transform> results)
    {
        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == targetName)
            {
                results.Add(child);
            }

            FindAllChildrenRecursive(child, targetName, results);
        }
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public static TextMeshProUGUI FindText(Transform parent, params string[] names)
    {
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (string name in names)
        {
            Transform child = FindChildRecursive(parent, name);
            if (child != null && child.TryGetComponent(out TextMeshProUGUI text))
            {
                return text;
            }
        }
        return null;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public static Button FindButton(Transform parent, params string[] names)
    {
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (string name in names)
        {
            Transform child = FindChildRecursive(parent, name);
            if (child != null && child.TryGetComponent(out Button button))
            {
                return button;
            }
        }
        return null;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public static Image FindImage(Transform parent, params string[] names)
    {
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (string name in names)
        {
            Transform child = FindChildRecursive(parent, name);
            if (child != null && child.TryGetComponent(out Image image))
            {
                return image;
            }
        }
        return null;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public static Slider FindSlider(Transform parent, params string[] names)
    {
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (string name in names)
        {
            Transform child = FindChildRecursive(parent, name);
            if (child != null && child.TryGetComponent(out Slider slider))
            {
                return slider;
            }
        }
        return null;
    }
}
