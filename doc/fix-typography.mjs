#!/usr/bin/env node
/**
 * Fix typography in markdown files:
 * - Replace em-dashes (—) with en-dashes surrounded by spaces ( – )
 * - Replace smart double quotes ("") with straight quotes (")
 * - Replace smart single quotes ('') with straight quotes (')
 * - Replace ellipsis (…) with three periods (...)
 */

import { readFileSync, writeFileSync, readdirSync, statSync } from 'fs';
import { join, extname } from 'path';

const replacements = [
  { pattern: /—/g, replacement: ' – ', name: 'em-dashes' },
  { pattern: /[""]/g, replacement: '"', name: 'smart double quotes' },
  { pattern: /['']/g, replacement: "'", name: 'smart single quotes' },
  { pattern: /…/g, replacement: '...', name: 'ellipsis' },
];

function findMarkdownFiles(dir, files = []) {
  const entries = readdirSync(dir);
  
  for (const entry of entries) {
    const fullPath = join(dir, entry);
    
    // Skip node_modules and hidden directories
    if (entry === 'node_modules' || entry.startsWith('.')) {
      continue;
    }
    
    const stat = statSync(fullPath);
    
    if (stat.isDirectory()) {
      findMarkdownFiles(fullPath, files);
    } else if (extname(entry) === '.md') {
      files.push(fullPath);
    }
  }
  
  return files;
}

function fixTypography(filePath) {
  let content = readFileSync(filePath, 'utf8');
  let modified = false;
  const fixes = [];
  
  for (const { pattern, replacement, name } of replacements) {
    if (pattern.test(content)) {
      content = content.replace(pattern, replacement);
      modified = true;
      fixes.push(name);
    }
  }
  
  if (modified) {
    writeFileSync(filePath, content, 'utf8');
    console.log(`Fixed ${fixes.join(', ')} in: ${filePath}`);
  }
  
  return modified;
}

// Main execution
const args = process.argv.slice(2);
const files = args.length > 0 ? args : findMarkdownFiles('.');

console.log('Fixing typography in markdown files...');

let fixedCount = 0;
for (const file of files) {
  if (fixTypography(file)) {
    fixedCount++;
  }
}

console.log(`Done! Fixed ${fixedCount} file(s).`);
