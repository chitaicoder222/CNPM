// Purpose: Resolves browser TTS language and voice from the narration content language, not the UI language.
// Responsibility: Keeps the language mapping testable and lets SpeechSynthesis fall back safely when no matching voice is installed.
(() => {
  const speechLocales = Object.freeze(new Map([
    ["vi", "vi-VN"], ["ko", "ko-KR"], ["ja", "ja-JP"], ["zh", "zh-CN"], ["en", "en-US"]
  ]));

  function normalizeLocale(value) {
    return typeof value === "string" ? value.trim().replaceAll("_", "-").toLowerCase() : "";
  }

  function normalizeLanguageCode(languageCode) {
    const primaryLanguage = normalizeLocale(languageCode).split("-")[0];
    return speechLocales.has(primaryLanguage) ? primaryLanguage : "vi";
  }

  function resolveSpeechLocale(languageCode) {
    return speechLocales.get(normalizeLanguageCode(languageCode));
  }

  function chooseVoice(voices, speechLocale) {
    if (!Array.isArray(voices) || voices.length === 0) return null;
    const normalizedLocale = normalizeLocale(speechLocale);
    const primaryLanguage = normalizedLocale.split("-")[0];
    const exactMatch = voices.find(voice => normalizeLocale(voice.lang) === normalizedLocale);
    if (exactMatch) return exactMatch;
    return voices.find(voice => normalizeLocale(voice.lang).split("-")[0] === primaryLanguage) ?? null;
  }

  const api = Object.freeze({ normalizeLanguageCode, resolveSpeechLocale, chooseVoice });
  if (typeof module !== "undefined" && module.exports) module.exports = api;
  globalThis.VivoiceTtsLanguage = api;
})();
