# Webo Landing Page

Static React + Vite build of the Webo bilingual landing page.

## Local development

```bash
npm install
npm run dev
```

## Production build

```bash
npm run build
```

GitHub Pages is deployed from this folder by `.github/workflows/deploy-pages.yml`.
The Lovable editor badge is not part of these source files; it is only injected into Lovable's own preview/editor surface.


Deployment: every update to `site/` triggers the GitHub Pages workflow. The deployed site is served from the repository's GitHub Pages URL; no Lovable editor badge is included in the production build.
