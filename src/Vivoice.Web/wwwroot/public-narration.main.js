// Purpose: Owns the browser Web Speech lifecycle for VIVOICE narration.
// Responsibility: Keeps voice selection, stale callback protection and speech
// failure handling separate from the public map/GPS orchestration.
(() => {
  function createNarrationController({ elements, text, ttsLanguage, window, setSpeechStatus, setSpeechControls, getSelectedNarration, setNarrationCanReplay, isReplayAllowed }) {
    let availableVoices = [];
    let speechToken = 0;
    let activeSpeech = null;
    let speechStartTimer = null;

    function refreshVoices() {
      try { availableVoices = typeof window.speechSynthesis?.getVoices === "function" ? window.speechSynthesis.getVoices() : []; }
      catch { availableVoices = []; }
    }
    function initializeVoices() {
      if (!("speechSynthesis" in window)) return;
      refreshVoices();
      if (typeof window.speechSynthesis.addEventListener === "function") window.speechSynthesis.addEventListener("voiceschanged", refreshVoices);
      else window.speechSynthesis.onvoiceschanged = refreshVoices;
    }
    function stop(announce = false) {
      const wasActive = Boolean(activeSpeech) || Boolean(window.speechSynthesis?.speaking) || Boolean(window.speechSynthesis?.pending);
      const interrupted = activeSpeech;
      speechToken += 1; activeSpeech = null; window.clearTimeout(speechStartTimer); speechStartTimer = null;
      if ("speechSynthesis" in window) window.speechSynthesis.cancel();
      interrupted?.resolve(false);
      setSpeechControls({ canReplay: Boolean(getSelectedNarration()) });
      if (announce && wasActive) setSpeechStatus("speechStopped", "stopped"); else if (!announce) setSpeechStatus();
    }
    function speak(narration) {
      stop();
      if (!String(narration?.content || "").trim()) { setSpeechStatus("speechEmpty", "error"); return Promise.resolve(false); }
      if (!("speechSynthesis" in window) || typeof window.SpeechSynthesisUtterance !== "function" || !ttsLanguage) {
        setNarrationCanReplay(true); setSpeechControls({ canReplay: true }); setSpeechStatus("speechUnavailable", "error"); return Promise.resolve(false);
      }
      return new Promise(resolve => {
        const token = ++speechToken;
        const isCurrentSpeech = () => token === speechToken;
        const finish = (success, status, state) => {
          if (!isCurrentSpeech()) return;
          speechToken += 1; window.clearTimeout(speechStartTimer); speechStartTimer = null; activeSpeech = null;
          setNarrationCanReplay(true); if (!success) window.speechSynthesis.cancel();
          setSpeechControls({ canReplay: true }); setSpeechStatus(status, state); resolve(success);
        };
        try {
          const utterance = new window.SpeechSynthesisUtterance(narration.content);
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
    return Object.freeze({ initializeVoices, stop, speak, isActive: () => Boolean(activeSpeech), canReplay: () => Boolean(getSelectedNarration()) && Boolean(isReplayAllowed()) });
  }
  const api = Object.freeze({ create: createNarrationController });
  globalThis.VivoiceNarrationController = api;
  if (typeof module !== "undefined" && module.exports) module.exports = api;
})();
