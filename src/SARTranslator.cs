using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SARTranslator
{
    public static class Mod
    {
        private static Dictionary<string, string> _dict;
        private static Dictionary<string, string> _dictIgnoreCase;
        private static bool _initialized;
        private static bool _removeAccents;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;
            _dict = new Dictionary<string, string>(StringComparer.Ordinal);
            _dictIgnoreCase = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string asmDir = "";
                try
                {
                    string loc = typeof(Mod).Assembly.Location;
                    if (!string.IsNullOrEmpty(loc)) asmDir = Path.GetDirectoryName(loc);
                }
                catch { }

                List<string> candidates = new List<string>();
                if (!string.IsNullOrEmpty(asmDir))
                {
                    candidates.Add(Path.Combine(asmDir, "translation_ptbr.txt"));
                    candidates.Add(Path.Combine(asmDir, "..\\translation_ptbr.txt"));
                    candidates.Add(Path.Combine(asmDir, "..\\..\\translation_ptbr.txt"));
                }
                if (!string.IsNullOrEmpty(baseDir))
                {
                    candidates.Add(Path.Combine(baseDir, "Super.Alloy.Ranger_Data\\translation_ptbr.txt"));
                    candidates.Add(Path.Combine(baseDir, "translation_ptbr.txt"));
                    candidates.Add(Path.Combine(baseDir, "Translation\\pt\\Text\\translation_ptbr.txt"));
                }

                foreach (string p in candidates)
                {
                    if (File.Exists(p))
                    {
                        using (StreamReader sr = new StreamReader(p, Encoding.UTF8))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                line = line.TrimStart();
                                if (line.StartsWith("#") || line.StartsWith("//"))
                                {
                                    continue;
                                }

                                int eq = line.IndexOf('=');
                                if (eq > 0)
                                {
                                    string k = line.Substring(0, eq).Replace("\\n", "\n");
                                    string v = line.Substring(eq + 1).Replace("\\n", "\n");
                                    if (k.Equals("RemoveAccents", StringComparison.OrdinalIgnoreCase))
                                    {
                                        bool.TryParse(v, out _removeAccents);
                                    }
                                    else
                                    {
                                        _dict[k] = v;
                                        if (!_dictIgnoreCase.ContainsKey(k))
                                        {
                                            _dictIgnoreCase[k] = v;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    string errPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sar_translator_err.txt");
                    File.WriteAllText(errPath, ex.ToString());
                }
                catch { }
            }
        }

        public static string Translate(string original)
        {
            if (string.IsNullOrEmpty(original)) return original;
            if (!_initialized) Init();
            if (_dict == null || _dict.Count == 0) return original;

            string translated;
            if (_dict.TryGetValue(original, out translated))
            {
                return CleanResult(translated);
            }

            string trimmed = original.Trim();
            if (trimmed.Length > 0 && _dict.TryGetValue(trimmed, out translated))
            {
                string res = original.Replace(trimmed, translated);
                return CleanResult(res);
            }

            if (_dictIgnoreCase != null && _dictIgnoreCase.TryGetValue(trimmed, out translated))
            {
                string res = original.Replace(trimmed, translated);
                return CleanResult(res);
            }

            return original;
        }

        private static string CleanResult(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (_removeAccents)
            {
                text = StripAccents(text);
            }
            // VonwaonBitmap font does not have the 'ç' / 'Ç' glyph, causing it to render blank
            if (text.IndexOf('ç') >= 0 || text.IndexOf('Ç') >= 0)
            {
                text = text.Replace('ç', 'c').Replace('Ç', 'C');
            }
            return text;
        }

        public static void TranslateButtonTip(UnityEngine.Transform root)
        {
            if (root == null) return;
            try
            {
                if (!_initialized) Init();
                var texts = root.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                if (texts != null)
                {
                    for (int i = 0; i < texts.Length; i++)
                    {
                        var t = texts[i];
                        if (t != null)
                        {
                            t.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
                            t.verticalOverflow = UnityEngine.VerticalWrapMode.Overflow;

                            if (!string.IsNullOrEmpty(t.text))
                            {
                                float oldW = 0f;
                                if (t.rectTransform != null)
                                {
                                    oldW = t.rectTransform.sizeDelta.x;
                                }

                                t.text = Translate(t.text);

                                float newW = t.preferredWidth;
                                if (newW <= 0f)
                                {
                                    newW = t.text.Length * 9.5f;
                                }

                                if (newW > oldW)
                                {
                                    float diff = newW - oldW;
                                    if (t.rectTransform != null)
                                    {
                                        t.rectTransform.sizeDelta = new UnityEngine.Vector2(newW + 4f, t.rectTransform.sizeDelta.y);
                                    }

                                    var rootRt = root.GetComponent<UnityEngine.RectTransform>();
                                    if (rootRt != null)
                                    {
                                        rootRt.sizeDelta = new UnityEngine.Vector2(rootRt.sizeDelta.x + diff + 12f, rootRt.sizeDelta.y);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static void TranslateLanguageUI(UnityEngine.Transform root)
        {
            if (root == null) return;
            try
            {
                if (!_initialized) Init();

                string ptLabel = Translate("English");
                if (string.IsNullOrEmpty(ptLabel) || ptLabel.Equals("English", StringComparison.OrdinalIgnoreCase))
                {
                    ptLabel = "Português";
                }
                ptLabel = CleanResult(ptLabel);

                // 1. Check all child transforms (look for "en" or "english")
                var allTransforms = root.GetComponentsInChildren<UnityEngine.Transform>(true);
                if (allTransforms != null)
                {
                    for (int i = 0; i < allTransforms.Length; i++)
                    {
                        var tr = allTransforms[i];
                        if (tr == null) continue;

                        if (tr.name.Equals("en", StringComparison.OrdinalIgnoreCase) ||
                            tr.name.Equals("english", StringComparison.OrdinalIgnoreCase))
                        {
                            var texts = tr.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                            if (texts != null)
                            {
                                for (int j = 0; j < texts.Length; j++)
                                {
                                    ApplyPtBrLanguageLabel(texts[j], ptLabel);
                                }
                            }
                        }
                    }
                }

                // 2. Also check all Text components whose text equals "English", "Inglês", or "Ingles"
                var allTexts = root.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                if (allTexts != null)
                {
                    for (int i = 0; i < allTexts.Length; i++)
                    {
                        var t = allTexts[i];
                        if (t != null && !string.IsNullOrEmpty(t.text))
                        {
                            string trimmed = t.text.Trim();
                            if (trimmed.Equals("English", StringComparison.OrdinalIgnoreCase) ||
                                trimmed.Equals("Ingles", StringComparison.OrdinalIgnoreCase) ||
                                trimmed.Equals("Inglês", StringComparison.OrdinalIgnoreCase))
                            {
                                ApplyPtBrLanguageLabel(t, ptLabel);
                            }
                        }
                    }
                }

                // 3. If this is LangueageBox (has ArrowL and ArrowR), adjust arrow positions to never overlap
                var arrowL = root.Find("ArrowL");
                var arrowR = root.Find("ArrowR");
                if (arrowL != null && arrowR != null)
                {
                    var langContainer = root.Find("Language");
                    float activeWidth = 0f;
                    if (langContainer != null)
                    {
                        for (int i = 0; i < langContainer.childCount; i++)
                        {
                            var child = langContainer.GetChild(i);
                            if (child != null && child.gameObject.activeSelf)
                            {
                                var txt = child.GetComponentInChildren<UnityEngine.UI.Text>(true);
                                if (txt != null)
                                {
                                    activeWidth = txt.preferredWidth;
                                    if (activeWidth <= 0f && !string.IsNullOrEmpty(txt.text))
                                    {
                                        activeWidth = txt.text.Length * 9.5f;
                                    }
                                }
                                break;
                            }
                        }
                    }

                    if (activeWidth <= 0f) activeWidth = ptLabel.Length * 9.5f;

                    float targetOffset = Math.Max(50f, (activeWidth / 2f) + 12f);

                    var rtL = arrowL.GetComponent<UnityEngine.RectTransform>();
                    if (rtL != null)
                    {
                        rtL.anchoredPosition = new UnityEngine.Vector2(-targetOffset, rtL.anchoredPosition.y);
                    }
                    else
                    {
                        arrowL.localPosition = new UnityEngine.Vector3(-targetOffset, arrowL.localPosition.y, arrowL.localPosition.z);
                    }

                    var rtR = arrowR.GetComponent<UnityEngine.RectTransform>();
                    if (rtR != null)
                    {
                        rtR.anchoredPosition = new UnityEngine.Vector2(targetOffset, rtR.anchoredPosition.y);
                    }
                    else
                    {
                        arrowR.localPosition = new UnityEngine.Vector3(targetOffset, arrowR.localPosition.y, arrowR.localPosition.z);
                    }
                }
            }
            catch { }
        }

        private static void ApplyPtBrLanguageLabel(UnityEngine.UI.Text t, string label)
        {
            if (t == null) return;
            try
            {
                t.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
                t.verticalOverflow = UnityEngine.VerticalWrapMode.Overflow;
                t.text = label;

                if (t.rectTransform != null)
                {
                    float pref = t.preferredWidth;
                    if (pref <= 0f) pref = label.Length * 9.5f;
                    if (t.rectTransform.sizeDelta.x < pref)
                    {
                        t.rectTransform.sizeDelta = new UnityEngine.Vector2(pref + 16f, t.rectTransform.sizeDelta.y);
                    }
                }
            }
            catch { }
        }

        private static string StripAccents(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder(normalized.Length);

            for (int i = 0; i < normalized.Length; i++)
            {
                char c = normalized[i];
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC).Replace("º", "o").Replace("ª", "a");
        }
    }
}
