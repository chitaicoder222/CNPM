// Purpose: Resolves browser TTS language and voice from the narration content language, not the UI language.
// Responsibility: Keeps the language mapping testable and lets SpeechSynthesis fall back safely when no matching voice is installed.
(() => {
  const speechLocales = Object.freeze({ vi: "vi-VN", ko: "ko-KR", ja: "ja-JP", zh: "zh-CN", th: "th-TH" });

  function normalizeLanguageCode(languageCode) {
    const normalized = typeof languageCode === "string" ? languageCode.trim().replaceAll("_", "-").toLowerCase() : "";
    const primaryLanguage = normalized.split("-")[0];
    return Object.hasOwn(speechLocales, primaryLanguage) ? primaryLanguage : "vi";
  }

  function resolveSpeechLocale(languageCode) {
    return speechLocales[normalizeLanguageCode(languageCode)];
  }

  function chooseVoice(voices, speechLocale) {
    if (!Array.isArray(voices) || voices.length === 0) return null;
    const normalizedLocale = String(speechLocale || "").replaceAll("_", "-").toLowerCase();
    const primaryLanguage = normalizedLocale.split("-")[0];
    return voices.find(voice => String(voice.lang || "").toLowerCase() === normalizedLocale)
      ?? voices.find(voice => String(voice.lang || "").replaceAll("_", "-").toLowerCase().split("-")[0] === primaryLanguage)
      ?? null;
  }

  const api = Object.freeze({ normalizeLanguageCode, resolveSpeechLocale, chooseVoice });
  if (typeof module !== "undefined" && module.exports) module.exports = api;
  globalThis.VivoiceTtsLanguage = api;
})();
