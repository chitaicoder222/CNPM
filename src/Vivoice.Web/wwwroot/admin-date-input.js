// Purpose: Converts API UTC instants to browser-local form values without changing the represented instant.
// Responsibility: Keeps VIVOICE admin campaign, payment, advertisement and dashboard date inputs timezone-safe.
(function exposeAdminDateInputs(root, factory) {
  const api = factory();
  if (typeof module !== "undefined" && module.exports) module.exports = api;
  if (root) root.VivoiceAdminDateInputs = api;
})(typeof window === "undefined" ? globalThis : window, () => {
  function pad(value) { return String(value).padStart(2, "0"); }
  function validDate(value) {
    if (!value) return null;
    const date = value instanceof Date ? new Date(value.getTime()) : new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
  }
  function toLocalDateTimeInput(value) {
    const date = validDate(value);
    return date ? `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}` : "";
  }
  function toLocalDateInput(value) {
    const date = validDate(value);
    return date ? `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` : "";
  }
  return Object.freeze({ toLocalDateTimeInput, toLocalDateInput });
});
