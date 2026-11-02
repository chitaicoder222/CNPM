// Purpose: Checks local documentation links and anchors after consolidation or future edits.
const fs = require("node:fs");
const path = require("node:path");
const root = path.resolve(__dirname, "../docs");
function markdownFiles(directory) {
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap(entry => {
    const target = path.join(directory, entry.name);
    return entry.isDirectory() ? markdownFiles(target) : entry.name.endsWith(".md") ? [target] : [];
  });
}
let links = 0;
const errors = [];
const documents = markdownFiles(root);
for (const file of documents) {
  const content = fs.readFileSync(file, "utf8");
  for (const match of content.matchAll(/\[[^\]]*\]\(([^)]+)\)/g)) {
    const href = match[1];
    if (/^[a-z][a-z0-9+.-]*:/i.test(href)) continue;
    links++;
    const [relative, fragment] = href.split("#");
    const target = relative ? path.resolve(path.dirname(file), decodeURIComponent(relative)) : file;
    if (!fs.existsSync(target)) { errors.push(`${path.relative(root, file)}: missing ${href}`); continue; }
    if (!fragment || !target.endsWith(".md")) continue;
    const destination = fs.readFileSync(target, "utf8");
    const anchors = new Set([...destination.matchAll(/<a\s+id="([^"]+)"/g)].map(item => item[1]));
    for (const heading of destination.matchAll(/^#+\s+(.+)$/gm)) {
      anchors.add(heading[1].toLowerCase().replace(/[^\p{L}\p{N}\s_-]/gu, "").replace(/\s/g, "-"));
    }
    if (!anchors.has(decodeURIComponent(fragment))) errors.push(`${path.relative(root, file)}: missing anchor ${href}`);
  }
}
if (errors.length) { console.error(errors.join("\n")); process.exitCode = 1; }
else console.log(`Checked ${links} local links in ${documents.length} Markdown documents.`);
