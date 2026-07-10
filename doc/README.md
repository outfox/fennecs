# fennecs ECS documentation
This is a [VitePress](https://vitepress.dev) static site generator to create the website and documentation seen on [fennecs.net](https://fennecs.net)

The site is hosted on [statichost.eu](https://statichost.eu), which pulls this repo and runs `npm run build` to produce `.vitepress/dist` as the served site.

## Developing
```bash
npm run dev
```

### Building locally
```bash
npm run build
npm run preview
```

# Important
To build the site, you also need [**fenn**ecs](https://github.com/outfox/fennecs) checked out in the same parent directory as this project, i.e. the relative path `../fennecs`.

This is necessary because the site includes source code snippets from fennecs during static sitegeneration.
