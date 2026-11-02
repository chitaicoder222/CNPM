# Trích đoạn dùng chung: giọng đọc và fallback

Nguồn: `origin/main`, commit `a39128650e5e780dc4a2fded45a74b81a50f1c1e`.
Các đoạn dưới giữ nguyên nội dung từ main. Đây là tham chiếu tích hợp, không phải module chạy độc lập hoặc tuyên bố Bảo là tác giả ban đầu. Không chép lại GPS, quản trị, quảng cáo hay database vào gói Bảo.

## Web Speech API

Nguồn: `src/Vivoice.Web/wwwroot/public.js`, từ `refreshVoices` đến trước `playGpsPoi`. Phụ thuộc state, DOM và hàm trạng thái trong host public của An. Bao gồm tải giọng, dừng/hủy, phát theo `narration.languageCode`, timeout và xử lý sự kiện lỗi/kết thúc.

```javascript
  function refreshVoices() { try { availableVoices = typeof window.speechSynthesis?.getVoices === "function" ? window.speechSynthesis.getVoices() : []; } catch { availableVoices = []; } }
  function initializeVoices() {
    if (!("speechSynthesis" in window)) return;
    refreshVoices();
    if (typeof window.speechSynthesis.addEventListener === "function") window.speechSynthesis.addEventListener("voiceschanged", refreshVoices);
    else window.speechSynthesis.onvoiceschanged = refreshVoices;
  }
  function stopSpeech(announce = false) {
    const wasActive = Boolean(activeSpeech) || Boolean(window.speechSynthesis?.speaking) || Boolean(window.speechSynthesis?.pending);
    const interrupted = activeSpeech;
    speechToken += 1; activeSpeech = null; window.clearTimeout(speechStartTimer); speechStartTimer = null;
    if ("speechSynthesis" in window) window.speechSynthesis.cancel();
    interrupted?.resolve(false);
    setSpeechControls({ canReplay: Boolean(selectedNarration) });
    if (announce && wasActive) setSpeechStatus("speechStopped", "stopped"); else if (!announce) setSpeechStatus();
  }
  function speakNarration(narration) {
    stopSpeech();
    if (!String(narration?.content || "").trim()) { setSpeechStatus("speechEmpty", "error"); return Promise.resolve(false); }
    if (!("speechSynthesis" in window) || typeof window.SpeechSynthesisUtterance !== "function" || !ttsLanguage) {
      setSpeechControls({ canReplay: true }); setSpeechStatus("speechUnavailable", "error"); return Promise.resolve(false);
    }
    return new Promise(resolve => {
      const token = ++speechToken;
      const isCurrentSpeech = () => token === speechToken;
      const finish = (success, status, state) => {
        if (!isCurrentSpeech()) return;
        speechToken += 1; window.clearTimeout(speechStartTimer); speechStartTimer = null; activeSpeech = null;
        if (!success) window.speechSynthesis.cancel();
        setSpeechControls({ canReplay: true }); setSpeechStatus(status, state); resolve(success);
      };
      try {
        const utterance = new SpeechSynthesisUtterance(narration.content);
        utterance.lang = ttsLanguage.resolveSpeechLocale(narration.languageCode);
        const matchingVoice = ttsLanguage.chooseVoice(availableVoices, utterance.lang);
        if (matchingVoice) utterance.voice = matchingVoice;
        activeSpeech = { token, utterance, resolve };
        utterance.onstart = () => { if (!isCurrentSpeech()) return; window.clearTimeout(speechStartTimer); speechStartTimer = null; setSpeechControls({ canPause: true, canStop: true }); setSpeechStatus("speaking", "speaking"); };
        utterance.onpause = () => { if (!isCurrentSpeech()) return; elements.pause.textContent = "▶"; setSpeechStatus("speechPaused", "paused"); };
        utterance.onresume = () => { if (!isCurrentSpeech()) return; elements.pause.textContent = text("pause"); setSpeechStatus("speechResumed", "resumed"); };
        utterance.onend = () => finish(true, "speechEnded", "ended");
        utterance.onerror = () => finish(false, "speechUnavailable", "error");
        elements.pause.textContent = text("pause"); setSpeechControls({ canPause: true, canStop: true, canReplay: true }); setSpeechStatus("speechPreparing", "preparing");
        speechStartTimer = window.setTimeout(() => finish(false, "speechUnavailable", "error"), 3000);
        window.speechSynthesis.speak(utterance);
      } catch { finish(false, "speechUnavailable", "error"); }
    });
  }
```

## Fallback bản dịch

Nguồn: `src/Vivoice.Web/Services/PublicContentService.cs`. Thứ tự thực tế: ngôn ngữ yêu cầu → tiếng Việt → bản dịch đầu tiên. API trả ngôn ngữ của nội dung đã chọn để client chọn giọng đúng. `First()` giả định có ít nhất một bản dịch; không tuyên bố hỗ trợ tập rỗng.

```csharp
    private static (PoiTranslation Value, bool FallbackUsed) FindPoiTranslation(IEnumerable<PoiTranslation> translations, string languageCode)
    {
        var requested = translations.FirstOrDefault(item => item.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
        var fallback = requested ?? translations.FirstOrDefault(item => item.LanguageCode.Equals("vi", StringComparison.OrdinalIgnoreCase)) ?? translations.First();
        return (fallback, !ReferenceEquals(fallback, requested));
    }

    private static (TopicTranslation Value, bool FallbackUsed) FindTopicTranslation(IEnumerable<TopicTranslation> translations, string languageCode)
    {
        var requested = translations.FirstOrDefault(item => item.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
        var fallback = requested ?? translations.FirstOrDefault(item => item.LanguageCode.Equals("vi", StringComparison.OrdinalIgnoreCase)) ?? translations.First();
        return (fallback, !ReferenceEquals(fallback, requested));
    }
```

