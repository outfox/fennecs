// markdown-emoji-plugin.ts
import MarkdownIt from 'markdown-it';
import { existsSync, readdirSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

interface EmojiPluginOptions {
  baseUrl: string;
  prefix: string;
  postfix: string;
  fileExtension: string;
  emojiDir: string;
}

export function neofoxPlugin(md: MarkdownIt, options?: Partial<EmojiPluginOptions>) {
  const defaultOptions: EmojiPluginOptions = {
    baseUrl: '/emoji/neofox_',
    prefix: ':neofox_',
    postfix: ':',
    fileExtension: '.png',
    emojiDir: resolve(dirname(fileURLToPath(import.meta.url)), '../public/emoji')
  };

  const opts = { ...defaultOptions, ...options };

  // Known emoji names, scanned once at startup (restart the dev server after adding images).
  // ':neofox_foo:' is only valid if 'public/emoji/neofox_foo.png' exists.
  const knownEmojis = new Set<string>();
  if (existsSync(opts.emojiDir)) {
    for (const file of readdirSync(opts.emojiDir)) {
      if (file.startsWith('neofox_') && file.endsWith(opts.fileExtension)) {
        knownEmojis.add(file.slice('neofox_'.length, -opts.fileExtension.length));
      }
    }
  } else {
    console.warn(`[neofox] emoji directory not found: ${opts.emojiDir} — emoji existence checks disabled`);
  }

  md.inline.ruler.after('emphasis', 'custom_emoji', (state, silent): boolean => {
    const startPos = state.pos;

    if (state.src.charAt(startPos) !== opts.prefix.charAt(0)) return false;

    const match = state.src.slice(startPos).match(new RegExp(`^\\${opts.prefix}(.*?)\\${opts.postfix}`));
    if (!match) return false;

    if (knownEmojis.size > 0 && !knownEmojis.has(match[1])) {
      // Unknown emoji: leave the literal text in place so it's visible on the page.
      if (!silent) {
        const source = state.env?.relativePath ?? state.env?.path ?? 'unknown file';
        console.warn(`[neofox] unknown emoji ${opts.prefix}${match[1]}${opts.postfix} in ${source} — no neofox_${match[1]}${opts.fileExtension} in public/emoji, rendering as plain text`);
      }
      return false;
    }

    if (!silent) {
      // Instead of creating a Token directly, use the push method to add a new token
      const token = state.push('emoji', '', 0);
      token.content = `${opts.baseUrl}${match[1]}${opts.fileExtension}`;
      token.meta = { fileName: match[1] };
    }

    state.pos += match[0].length;
    return true;
  });

  md.renderer.rules.emoji = (tokens, idx) => {
    const fileNameWithoutExtension = tokens[idx].meta.fileName;
    // Use the file name without extension as the alt text
    return `<img src="${tokens[idx].content}" alt="${'Neofox: ' + fileNameWithoutExtension}" title="${fileNameWithoutExtension}" style="display: inline-block; width: auto; height: 64px; margin: 0; padding: 0; vertical-align: middle;" />`;
  };
}
