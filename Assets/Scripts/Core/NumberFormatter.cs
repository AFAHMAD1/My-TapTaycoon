using UnityEngine;

/// <summary>
/// Çok büyük sayıları (milyon, milyar vb.) kısaltarak ekranda daha okunabilir hale getiren yardımcı sınıf.
/// </summary>
public static class NumberFormatter
{
    // Standart kısaltmalar: K (Bin), M (Milyon), B (Milyar), T (Trilyon)
    private static readonly string[] StandardSuffixes = { "K", "M", "B", "T" };

    /// <summary>
    /// Ham sayı değerini formatlı bir string'e dönüştürür. 
    /// Örn: 1500 -> "1.5K", 1000000 -> "1M"
    /// </summary>
    public static string Format(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return "0";
        }

        if (value < 0d)
        {
            return "-" + Format(-value);
        }

        // 1000'den küçük sayıları direkt olduğu gibi yaz (ondalıksız)
        if (value < 1000d)
        {
            return value.ToString("F0");
        }

        int suffixIndex = -1;
        double scaledValue = value;

        // Sayıyı 1000'e bölerek hangi harf takısının geleceğini bul
        while (scaledValue >= 1000d)
        {
            scaledValue /= 1000d;
            suffixIndex++;
        }

        // Yuvarlama sonrası 1000'e ulaşıldıysa bir üst seviyeye çık
        if (scaledValue >= 999.5d)
        {
            scaledValue /= 1000d;
            suffixIndex++;
        }

        string suffix = GetSuffix(suffixIndex);
        
        // Sayının büyüklüğüne göre ondalık sayısını ayarla (Örn: 1.25M veya 12.5M veya 125M)
        string numericFormat = scaledValue >= 100d ? "F0" : scaledValue >= 10d ? "F1" : "F2";
        return scaledValue.ToString(numericFormat, System.Globalization.CultureInfo.InvariantCulture) + suffix;
    }

    /// <summary>
    /// Suffix listesinden uygun harfi çeker. Liste biterse AA, BB gibi alfabetik devam eder.
    /// </summary>
    private static string GetSuffix(int suffixIndex)
    {
        if (suffixIndex < StandardSuffixes.Length)
        {
            return StandardSuffixes[suffixIndex];
        }

        // T'den sonrası için AA, BB, CC döngüsü...
        int alphabeticIndex = suffixIndex - StandardSuffixes.Length;
        int letterCount = (alphabeticIndex / 26) + 2;
        int characterIndex = alphabeticIndex % 26;
        char suffixCharacter = (char)('A' + characterIndex);
        return new string(suffixCharacter, letterCount);
    }

    /// <summary>
    /// Yazılı bir değeri (Örn: "1.5K") tekrar ham sayıya dönüştürür. (Test ve Debug için)
    /// </summary>
    public static bool TryParse(string input, out double result)
    {
        result = 0d;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim().ToUpperInvariant();
        
        // Sayı ve harf kısımlarını ayır
        int letterIndex = -1;
        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsLetter(input[i]))
            {
                letterIndex = i;
                break;
            }
        }

        string numberPart = letterIndex >= 0 ? input.Substring(0, letterIndex).Trim() : input;
        string suffixPart = letterIndex >= 0 ? input.Substring(letterIndex).Trim() : "";

        numberPart = numberPart.Replace(",", ".");
        if (!double.TryParse(numberPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numberValue))
        {
            return false;
        }

        if (string.IsNullOrEmpty(suffixPart))
        {
            result = numberValue;
            return true;
        }

        int multiplierIndex = -1;
        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < StandardSuffixes.Length; i++)
        {
            if (StandardSuffixes[i] == suffixPart)
            {
                multiplierIndex = i;
                break;
            }
        }

        if (multiplierIndex == -1)
        {
            if (suffixPart.Length > 0 && suffixPart[0] >= 'A' && suffixPart[0] <= 'Z')
            {
                int letterCount = suffixPart.Length;
                int characterIndex = suffixPart[0] - 'A';
                int alphabeticIndex = (letterCount - 2) * 26 + characterIndex;
                multiplierIndex = StandardSuffixes.Length + alphabeticIndex;
            }
        }

        if (multiplierIndex >= 0)
        {
            result = numberValue * System.Math.Pow(1000d, multiplierIndex + 1);
        }
        else
        {
            result = numberValue;
        }

        return true;
    }
}
