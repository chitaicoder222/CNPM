using System.Text;

namespace TourGuide.Api.Utils;

/// <summary>
/// Chia văn bản thành các đoạn không dài quá maxLength ký tự.
/// Ưu tiên cắt theo câu (không cắt giữa câu); câu quá dài mới cắt theo từ; từ quá dài mới cắt cứng.
/// </summary>
public static class TextChunker
{
    private static readonly char[] CjkSentenceEnds = { '。', '！', '？', '…' };

    public static IReadOnlyList<string> Split(string text, int maxLength = 900)
    {
        if (maxLength < 1) throw new ArgumentOutOfRangeException(nameof(maxLength));

        var chunks = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return chunks;

        var current = new StringBuilder();

        void Flush()
        {
            if (current.Length == 0) return;
            chunks.Add(current.ToString());
            current.Clear();
        }

        foreach (var sentence in SplitSentences(text))
        {
            if (sentence.Length > maxLength)
            {
                Flush();
                chunks.AddRange(SplitLong(sentence, maxLength));
                continue;
            }

            if (current.Length > 0 && current.Length + 1 + sentence.Length > maxLength) Flush();
            if (current.Length > 0) current.Append(' ');
            current.Append(sentence);
        }

        Flush();
        return chunks;
    }

    // Tách câu theo . ! ? … (chỉ khi theo sau là khoảng trắng/hết chuỗi), dấu câu CJK và xuống dòng.
    private static IEnumerable<string> SplitSentences(string text)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '\r') continue;

            var end = c == '\n'
                || Array.IndexOf(CjkSentenceEnds, c) >= 0
                || ((c == '.' || c == '!' || c == '?') && (i + 1 == text.Length || char.IsWhiteSpace(text[i + 1])));

            if (c != '\n') sb.Append(c);
            if (!end) continue;

            var s = sb.ToString().Trim();
            sb.Clear();
            if (s.Length > 0) yield return s;
        }

        var rest = sb.ToString().Trim();
        if (rest.Length > 0) yield return rest;
    }

    private static IEnumerable<string> SplitLong(string text, int maxLength)
    {
        var current = new StringBuilder();
        foreach (var word in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            if (word.Length > maxLength)
            {
                if (current.Length > 0) { yield return current.ToString(); current.Clear(); }
                for (var i = 0; i < word.Length; i += maxLength)
                    yield return word.Substring(i, Math.Min(maxLength, word.Length - i));
                continue;
            }

            if (current.Length > 0 && current.Length + 1 + word.Length > maxLength)
            {
                yield return current.ToString();
                current.Clear();
            }
            if (current.Length > 0) current.Append(' ');
            current.Append(word);
        }

        if (current.Length > 0) yield return current.ToString();
    }
}
