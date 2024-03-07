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
      copyright: '<b>fenn</b>ecs is made with love & foxes, copyright © 2024 <a href="https://github.com/thygrrr/fennecs/graphs/contributors"> its contributors</a>'
    },

    sidebar: generateSidebar([
      {
        sortMenusByName: true,
        useFolderLinkFromIndexFile: true,
        useTitleFromFrontmatter: true,
        useFolderTitleFromIndexFile: true,
        documentRootPath: '/',
        scanStartPath: '/',
        resolvePath: '/',
        excludeFiles: ['nuget.md'],
        includeRootIndexFile: false,
        collapseDepth: 1,
        excludeFolders: [".", "node_modules", "dist", "public", "src", "vitepress", "vitepress-sidebar"],
      }
    ]),

    socialLinks: [
      { icon: 'github', link: 'https://github.com/thygrrr/fennecs/' }
    ],
  },

  async transformHead(context) {
    head: [
      ['link', { rel: 'preconnect', href: 'https://fonts.googleapis.com' }],
      ['link', { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' }],
      ['link', { href: 'https://fonts.googleapis.com/css2?family=Bai+Jamjuree:ital,wght@0,200;0,300;0,400;0,500;0,600;0,700;1,200;1,300;1,400;1,500;1,600;1,700&display=swap', rel: 'stylesheet' }],
      ['link', { href: 'https://mastodon.gamedev.place/@jupiter', rel: 'me' }],
      ['link', { rel: "apple-touch-icon", sizes: "180x180", href: "/apple-touch-icon.png" }],
      ['link', { rel: "icon", type: "image/png", sizes: "32x32", href: "/favicon-32x32.png" }],
      ['link', { rel: "icon", type: "image/png", sizes: "16x16", href: "/favicon-16x16.png" }],
      ['link', { rel: "manifest", href: "/site.webmanifest" }],
      ['link', { rel: "mask-icon", href: "/safari-pinned-tab.svg", color: "#142458" }],
      ['meta', { name: "msapplication-TileColor", content: "#142458" }],
      ['meta', { name: "theme-color", content: "#ffffff" }],
    ]
  }
})
