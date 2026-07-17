// Packages doc/skills/* into public/downloads/fennecs-skill.zip so the docs
// site serves the agent skill as a single download. Runs automatically before
// `npm run dev` and `npm run build` (see package.json pre-scripts).
import { zipSync } from 'fflate';
import { readFileSync, writeFileSync, mkdirSync, readdirSync } from 'node:fs';
import { join, relative, dirname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const skillsRoot = join(here, 'skills');
const outFile = join(here, 'public', 'downloads', 'fennecs-skill.zip');

const files = {};
for (const entry of readdirSync(skillsRoot, { recursive: true, withFileTypes: true })) {
  if (!entry.isFile()) continue;
  const abs = join(entry.parentPath, entry.name);
  const zipPath = relative(skillsRoot, abs).split(sep).join('/');
  files[zipPath] = readFileSync(abs);
}

if (Object.keys(files).length === 0) {
  throw new Error(`no skill files found under ${skillsRoot}`);
}

mkdirSync(dirname(outFile), { recursive: true });
writeFileSync(outFile, zipSync(files, { level: 9 }));
console.log(`fennecs-skill.zip: ${Object.keys(files).length} files -> ${outFile}`);
