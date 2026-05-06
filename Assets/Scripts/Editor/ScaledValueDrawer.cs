using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ScaledValue))]
public class ScaledValueDrawer : PropertyDrawer
{
    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Bu satir: 'EditorGUI' objesi uzerindeki 'BeginProperty' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
        EditorGUI.BeginProperty(position, label, property);

        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect foldoutRect = new Rect(position.x, position.y, position.width, singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            // Bu satir: 'property' uzerindeki 'FindPropertyRelative' metodunu cagirir ve donen sonucu 'modeProp' degiskenine kaydeder.
            SerializedProperty modeProp = property.FindPropertyRelative("mode");
            // Bu satir: 'property' uzerindeki 'FindPropertyRelative' metodunu cagirir ve donen sonucu 'baseValueStringProp' degiskenine kaydeder.
            SerializedProperty baseValueStringProp = property.FindPropertyRelative("baseValueString");
            // Bu satir: 'property' uzerindeki 'FindPropertyRelative' metodunu cagirir ve donen sonucu 'baseValueProp' degiskenine kaydeder.
            SerializedProperty baseValueProp = property.FindPropertyRelative("baseValue");
            // Bu satir: 'property' uzerindeki 'FindPropertyRelative' metodunu cagirir ve donen sonucu 'multiplierProp' degiskenine kaydeder.
            SerializedProperty multiplierProp = property.FindPropertyRelative("multiplierPerLevel");

            Rect modeRect = new Rect(position.x, position.y + singleLineHeight + spacing, position.width, singleLineHeight);
            Rect baseValueRect = new Rect(position.x, modeRect.y + singleLineHeight + spacing, position.width, singleLineHeight);
            Rect parsedValueRect = new Rect(position.x, baseValueRect.y + singleLineHeight + spacing, position.width, singleLineHeight);
            Rect multiplierRect = new Rect(position.x, parsedValueRect.y + singleLineHeight + spacing, position.width, singleLineHeight);

            // Bu satir: 'EditorGUI' objesi uzerindeki 'PropertyField' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
            EditorGUI.PropertyField(modeRect, modeProp);

            // Geriye donuk uyumluluk ve ilk acilis icin eger baseValueString bossa, mevcut baseValue'yu string'e cevirip goster
            if (string.IsNullOrWhiteSpace(baseValueStringProp.stringValue) && baseValueProp.doubleValue > 0)
            {
                baseValueStringProp.stringValue = NumberFormatter.Format(baseValueProp.doubleValue);
            }
            // Eger kullanici sadece harf girdiyse (orn "M") ve yaninda sayi yoksa, onu formatli bir yapiya donusturmek iyi olabilir
            // ama kullanicinin yazdigini bozmamak icin dokunmuyoruz.

            // Kullanicidan string formatinda degeri aliyoruz (ornegin "62.4B")
            // Bu satir: 'EditorGUI' objesi uzerindeki 'BeginChangeCheck' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
            EditorGUI.BeginChangeCheck();
            // Bu satir: 'EditorGUI' uzerindeki 'TextField' metodunu cagirir ve donen sonucu 'newString' degiskenine kaydeder.
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
            // Bu satir: 'EditorGUI' objesi uzerindeki 'DoubleField' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
            EditorGUI.DoubleField(parsedValueRect, "Parsed Value (Gercek Deger)", baseValueProp.doubleValue);
            GUI.enabled = true;

            // Bu satir: 'EditorGUI' objesi uzerindeki 'PropertyField' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
            EditorGUI.PropertyField(multiplierRect, multiplierProp);

            EditorGUI.indentLevel--;
        }

        // Bu satir: 'EditorGUI' objesi uzerindeki 'EndProperty' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
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
