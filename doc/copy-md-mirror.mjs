// Copies the site's markdown sources into .vitepress/dist after the build,
// so every rendered page has a raw-Markdown twin (/docs/Foo.html -> /docs/Foo.md)
// for AI agents and other machine readers. Runs automatically after
// `npm run build` (see package.json postbuild).
import { readFileSync, writeFileSync, mkdirSync, readdirSync } from 'node:fs';
import { join, relative, dirname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const dist = join(here, '.vitepress', 'dist');
const ROOTS = ['docs', 'cookbook', 'examples', 'misc'];

let count = 0;
const copy = (file) => {
  const rel = relative(here, file);
  const target = join(dist, rel);
  mkdirSync(dirname(target), { recursive: true });
  writeFileSync(target, readFileSync(file, 'utf8').replace(/^﻿/, ''));
  count++;
};

copy(join(here, 'index.md'));
for (const root of ROOTS) {
  for (const entry of readdirSync(join(here, root), { recursive: true, withFileTypes: true })) {
    if (entry.isFile() && entry.name.endsWith('.md')) copy(join(entry.parentPath, entry.name));
  }
}
console.log(`md mirror: ${count} markdown files copied into dist`);
