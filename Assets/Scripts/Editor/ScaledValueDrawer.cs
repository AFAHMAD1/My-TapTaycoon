using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ScaledValue))]
public class ScaledValueDrawer : PropertyDrawer
{
    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect foldoutRect = new Rect(position.x, position.y, position.width, singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            SerializedProperty modeProp = property.FindPropertyRelative("mode");
            SerializedProperty baseValueStringProp = property.FindPropertyRelative("baseValueString");
            SerializedProperty baseValueProp = property.FindPropertyRelative("baseValue");
            SerializedProperty multiplierProp = property.FindPropertyRelative("multiplierPerLevel");

            Rect modeRect = new Rect(position.x, position.y + singleLineHeight + spacing, position.width, singleLineHeight);
            Rect baseValueRect = new Rect(position.x, modeRect.y + singleLineHeight + spacing, position.width, singleLineHeight);
            Rect parsedValueRect = new Rect(position.x, baseValueRect.y + singleLineHeight + spacing, position.width, singleLineHeight);
            Rect multiplierRect = new Rect(position.x, parsedValueRect.y + singleLineHeight + spacing, position.width, singleLineHeight);

            EditorGUI.PropertyField(modeRect, modeProp);

            // Geriye donuk uyumluluk ve ilk acilis icin eger baseValueString bossa, mevcut baseValue'yu string'e cevirip goster
            if (string.IsNullOrWhiteSpace(baseValueStringProp.stringValue) && baseValueProp.doubleValue > 0)
            {
                baseValueStringProp.stringValue = NumberFormatter.Format(baseValueProp.doubleValue);
            }
            // Eger kullanici sadece harf girdiyse (orn "M") ve yaninda sayi yoksa, onu formatli bir yapiya donusturmek iyi olabilir
            // ama kullanicinin yazdigini bozmamak icin dokunmuyoruz.

            // Kullanicidan string formatinda degeri aliyoruz (ornegin "62.4B")
            EditorGUI.BeginChangeCheck();
            string newString = EditorGUI.TextField(baseValueRect, "Base Value", baseValueStringProp.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                baseValueStringProp.stringValue = newString;
                
                // Aninda parse edip asil degiskene atalim
                if (!string.IsNullOrWhiteSpace(newString) && NumberFormatter.TryParse(newString, out double parsedValue))
                {
                    baseValueProp.doubleValue = parsedValue;
                }
                else if (double.TryParse(newString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double num))
                {
                    baseValueProp.doubleValue = num;
                }
            }

            // Gosterim amaciyla, okunan gercek sayisal degeri disable sekilde gosterelim
            GUI.enabled = false;
            EditorGUI.DoubleField(parsedValueRect, "Parsed Value (Gercek Deger)", baseValueProp.doubleValue);
            GUI.enabled = true;

            EditorGUI.PropertyField(multiplierRect, multiplierProp);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded)
        {
            return (singleLineHeight * 5) + (spacing * 4); // Foldout + 4 properties
        }

        return singleLineHeight;
    }
}
