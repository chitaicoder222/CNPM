// Purpose: Verifies timezone-safe conversion for VIVOICE admin date and datetime-local controls.
// Responsibility: Prevents a displayed campaign-rate instant from being submitted as a second UTC conversion.
const assert = require("node:assert/strict");
const test = require("node:test");
const { toLocalDateInput, toLocalDateTimeInput } = require("../../../src/Vivoice.Web/wwwroot/admin-date-input.js");

function pad(value) { return String(value).padStart(2, "0"); }
function expectedLocalDateTime(date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}
function expectedLocalDate(date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

test("an API UTC instant renders as the browser-local datetime-local value and round-trips once", () => {
  const instant = new Date("2026-10-29T12:34:00.000Z");
  const controlValue = toLocalDateTimeInput(instant.toISOString());
  assert.equal(controlValue, expectedLocalDateTime(instant));
  assert.equal(new Date(controlValue).toISOString(), instant.toISOString());
});

test("date-only dashboard filters retain the browser-local calendar day", () => {
  const instant = new Date("2026-10-29T00:15:00.000Z");
  assert.equal(toLocalDateInput(instant), expectedLocalDate(instant));
});

test("invalid or empty API values do not populate an admin date control", () => {
  assert.equal(toLocalDateTimeInput("not-a-date"), "");
  assert.equal(toLocalDateInput(null), "");
});
