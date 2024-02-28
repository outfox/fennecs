import { defineConfig } from 'vitepress'
import { generateSidebar } from 'vitepress-sidebar';

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "fennecs — tiny ECS for C#",
  description: "fennecs ...the tiny, tiny, high-energy Entity Component System!",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Documentation', link: '/docs/index' },
      { text: 'Examples', link: '/examples/index' },
    ],

    sidebar: generateSidebar([
      { sortMenusByName: true,
        useFolderLinkFromIndexFile: true,
        useTitleFromFrontmatter: true,
        useFolderTitleFromIndexFile: true,
        documentRootPath: '/',
        scanStartPath: '/',
        resolvePath: '/',
        includeRootIndexFile: true,
        collapseDepth: 2,
        excludeFolders: [".", "node_modules", "dist", "public", "src", "vitepress", "vitepress-sidebar"],
      }
    ]),

    socialLinks: [
      { icon: 'github', link: 'https://github.com/thygrrr/fennecs/' }
    ]
  }
})
