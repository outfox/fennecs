// Generates public/llms.txt and public/llms-full.txt (https://llmstxt.org)
// from the site's markdown sources, using each page's frontmatter title and
// description. Runs automatically before `npm run dev` / `npm run build`.
import { readFileSync, writeFileSync, readdirSync } from 'node:fs';
import { join, relative, dirname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const HOST = 'https://fennecs.net';

const ROOTS = ['docs', 'cookbook', 'examples', 'misc'];

function* mdFiles() {
  yield join(here, 'index.md');
  for (const root of ROOTS) {
    for (const entry of readdirSync(join(here, root), { recursive: true, withFileTypes: true })) {
      if (entry.isFile() && entry.name.endsWith('.md')) yield join(entry.parentPath, entry.name);
    }
  }
}

function frontmatter(file) {
  const text = readFileSync(file, 'utf8').replace(/^﻿/, '');
  const match = text.match(/^---\r?\n([\s\S]*?)\r?\n---\r?\n/);
  if (!match) return {};
  const get = (field) => {
    const m = match[1].match(new RegExp(`^${field}: (?:'(.*)'|"(.*)"|(.+))$`, 'm'));
    return m ? (m[1] ?? m[2] ?? m[3]).replace(/''/g, "'").trim() : undefined;
  };
  return { title: get('title'), description: get('description'), body: text.slice(match[0].length) };
}

const SECTION_LABEL = {
  docs: 'Documentation',
  cookbook: 'Cookbook (tutorials)',
  examples: 'Examples & Demos',
  misc: 'Miscellaneous',
};

// reading order within each section (folders/files not listed sort last, by path)
const READING_ORDER = {
  docs: ['index.md', 'Concepts.md', 'World.md', 'Entities', 'Components', 'Queries', 'Streams', 'Advanced'],
  cookbook: ['index.md', 'appetizers', 'staples', 'substitutes', 'godot', 'mise-en-place', 'cursed'],
  examples: ['index.md'],
  misc: ['index.md', 'Glossary.md', 'Changelog.md', 'Roadmap.md', 'Acknowledgements.md'],
};

const rank = (rel) => {
  const [root, ...rest] = rel.split('/');
  const order = READING_ORDER[root] ?? [];
  const idx = order.indexOf(rest.length > 1 ? rest[0] : rest[0] ?? '');
  // strip a trailing index.md so each folder's overview page sorts first
  return [idx === -1 ? order.length : idx, rel.replace(/index\.md$/, '')];
};

const sections = { docs: [], cookbook: [], examples: [], misc: [] };
let home;

for (const file of [...mdFiles()].sort()) {
  const rel = relative(here, file).split(sep).join('/');
  const { title, description, body } = frontmatter(file);
  if (!title || !description) continue; // every page has both; skip anything odd
  const page = { rel, url: `${HOST}/${rel}`, title, description, body };
  if (rel === 'index.md') home = page;
  else sections[rel.split('/')[0]].push(page);
}

for (const pages of Object.values(sections)) {
  pages.sort((a, b) => {
    const [ra, ka] = rank(a.rel), [rb, kb] = rank(b.rel);
    return ra - rb || (ka < kb ? -1 : ka > kb ? 1 : 0);
  });
}

// llms-full.txt: section banners + a YAML divider per page; bodies verbatim
const divider = (page, label) =>
  `---\npage: ${page.title}\nsection: ${label}\nsource: ${page.url}\ndescription: ${page.description}\n---`;

const fullParts = [
  `<!-- fennecs full documentation - one file, ${1 + Object.values(sections).flat().length} pages.
Each page begins with a YAML block (page/section/source/description); page content
follows verbatim. Index: ${HOST}/llms.txt - Agent Skill: ${HOST}/downloads/fennecs-skill.zip -->`,
  divider(home, 'Home'),
  home.body.trim(),
];
for (const [key, label] of Object.entries(SECTION_LABEL)) {
  for (const page of sections[key]) {
    fullParts.push(divider(page, label), page.body.trim());
  }
}

const section = (label, key) =>
  `## ${label}\n\n` + sections[key].map(p => `- [${p.title}](${p.url}): ${p.description}`).join('\n');

const llms = `# fennecs

> ${home.description}

fennecs is a free & open-source Entity-Component System (ECS) for modern C#/.NET,
made for games and simulations. Zero codegen, zero dependencies, archetype
storage, entity-entity relations, object links, and SIMD-friendly streams.
NuGet package: \`fennecs\` - https://www.nuget.org/packages/fennecs/

Every page below is also served as raw Markdown: replace \`.html\` with \`.md\`
(the URLs below already point at the Markdown versions). The entire site is
also available concatenated at ${HOST}/llms-full.txt.

## For Coding Agents

- [fennecs Agent Skill (zip)](${HOST}/downloads/fennecs-skill.zip): The official skill - SKILL.md plus three
  reference files of source-verified fennecs 0.7.0 API guidance. Unzip into your skills
  directory (Claude Code: \`.claude/skills/\`, Codex: \`.agents/skills/\`, OpenCode, Pi, Hermes: see guide).
- [Agent install guide](${HOST}/cookbook/mise-en-place/Agents.md): where the skill goes for each agent.

${section('Documentation', 'docs')}

${section('Cookbook (tutorials)', 'cookbook')}

${section('Examples & Demos', 'examples')}

## Optional

${sections.misc.map(p => `- [${p.title}](${p.url}): ${p.description}`).join('\n')}
`;

const full = fullParts.join('\n\n') + '\n';
writeFileSync(join(here, 'public', 'llms.txt'), llms);
writeFileSync(join(here, 'public', 'llms-full.txt'), full);
console.log(`llms.txt: ${1 + Object.values(sections).flat().length} pages indexed; llms-full.txt: ${(full.length / 1024).toFixed(0)} KiB`);
