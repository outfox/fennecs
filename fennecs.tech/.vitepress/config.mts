import { defineConfig } from 'vitepress'
import { generateSidebar } from 'vitepress-sidebar';

import { neofoxPlugin } from './plugin-neofox';
import { hyperlinkPlugin } from './plugin-hyperlink';

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "fennecs — tiny ECS",
  description: "fennecs ...the tiny, tiny, high-energy Entity Component System!",

  markdown: {
    config: (md) => {
      md.use(neofoxPlugin, {});
      md.use(hyperlinkPlugin, {});
    }
  },

  themeConfig: {
    logo: 'https://fennecs.tech/img/fennecs-logo-small.svg',
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Documentation', link: '/docs/index' },
      { text: 'Examples', link: '/demos/index' },
    ],

    footer: {
      message: '<a href="https://github.com/thygrrr/fennecs/?tab=MIT-1-ov-file#readme"><b>fenn</b>ecs</a> is released under the MIT License. <a href="https://volpeon.ink/emojis/neofox/">Neofox</a> is released under the CC BY-NC-SA 4.0 License.',
      copyright: '<b>fenn</b>ecs is copyright © 2024 Tiger Blue, 2022 Aaron Winter'
    },

    sidebar: generateSidebar([
      { sortMenusByName: true,
        useFolderLinkFromIndexFile: true,
        useTitleFromFrontmatter: true,
        useFolderTitleFromIndexFile: true,
        documentRootPath: '/',
        scanStartPath: '/',
        resolvePath: '/',
        includeRootIndexFile: false,
        collapseDepth: 1,
        excludeFolders: [".", "node_modules", "dist", "public", "src", "vitepress", "vitepress-sidebar"],
      }
    ]),

    socialLinks: [
      { icon: 'github', link: 'https://github.com/thygrrr/fennecs/' }
    ]
  }
})
