// Purpose: Verifies browser TTS language/voice selection independently from the selected public UI language.
// Responsibility: Covers supported narration languages, fallback metadata and safe voice-selection degradation without a browser or SQL Server.
const test = require("node:test");
const assert = require("node:assert/strict");
const ttsLanguage = require("../../../src/Vivoice.Web/wwwroot/tts-language.js");

test("Vietnamese fallback narration resolves to Vietnamese even when the UI language differs", () => {
  const selectedUiLanguage = "ko";
  const narration = { languageCode: "vi", fallbackUsed: true };
  assert.notEqual(selectedUiLanguage, narration.languageCode);
  assert.equal(ttsLanguage.resolveSpeechLocale(narration.languageCode), "vi-VN");
});

test("all supported narration language codes resolve to their requested speech locales", () => {
  assert.deepEqual(
    ["vi", "ko", "ja", "zh", "th"].map(ttsLanguage.resolveSpeechLocale),
    ["vi-VN", "ko-KR", "ja-JP", "zh-CN", "th-TH"]
  );
});

test("locale-style and underscore narration metadata normalize to the supported primary language", () => {
  assert.equal(ttsLanguage.resolveSpeechLocale("ko-KR"), "ko-KR");
  assert.equal(ttsLanguage.resolveSpeechLocale("zh_CN"), "zh-CN");
  assert.equal(ttsLanguage.normalizeLanguageCode("th-TH"), "th");
});

test("invalid or absent narration language metadata fails safely to Vietnamese", () => {
  assert.equal(ttsLanguage.resolveSpeechLocale(""), "vi-VN");
  assert.equal(ttsLanguage.resolveSpeechLocale("fr-FR"), "vi-VN");
  assert.equal(ttsLanguage.resolveSpeechLocale(null), "vi-VN");
});

test("matching voice prefers an exact narration locale over other available voices", () => {
  const koreanVoice = { name: "Korean", lang: "ko-KR" };
  const selected = ttsLanguage.chooseVoice([{ name: "Vietnamese", lang: "vi-VN" }, koreanVoice], "ko-KR");
  assert.equal(selected, koreanVoice);
});

test("matching voice accepts a same-language prefix when the exact locale is unavailable", () => {
  const japaneseVoice = { name: "Japanese", lang: "ja-JP" };
  assert.equal(ttsLanguage.chooseVoice([japaneseVoice], "ja-JP-x-private"), japaneseVoice);
});

test("missing matching voice leaves browser TTS free to use its native fallback", () => {
  assert.equal(ttsLanguage.chooseVoice([{ name: "Vietnamese", lang: "vi-VN" }], "th-TH"), null);
  assert.equal(ttsLanguage.chooseVoice([], "vi-VN"), null);
});
