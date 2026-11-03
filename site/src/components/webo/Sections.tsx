import { useEffect, useRef, useState, type ReactNode, type RefObject } from "react";
import { useLang } from "@/lib/i18n";
import { useReveal } from "@/hooks/use-motion";
import { WEBO_DOWNLOAD_URL, WEBO_PLATFORM_LABEL } from "@/config/site";
import { HeroArt } from "./HeroArt";

const container = "mx-auto w-full max-w-7xl px-5 sm:px-8";

function Kicker({ children }: { children: ReactNode }) {
  return (
    <p className="flex items-center gap-3 text-sm font-medium text-cyan">
      <span className="h-px w-8 bg-cyan/60" aria-hidden />
      {children}
    </p>
  );
}

function WindowsIcon() {
  return (
    <svg viewBox="0 0 24 24" className="size-4" aria-hidden fill="currentColor">
      <path d="M3 5.5l7.5-1v7H3zM11.5 4.4L21 3v8.5h-9.5zM3 12.5h7.5v7L3 18.5zM11.5 12.5H21V21l-9.5-1.4z" />
    </svg>
  );
}

export function DownloadButton({ label, big = false }: { label: string; big?: boolean }) {
  const { t } = useLang();
  const href = WEBO_DOWNLOAD_URL || "#download";
  return (
    <a
      href={href}
      {...(WEBO_DOWNLOAD_URL ? { rel: "nofollow", "aria-label": `${label} — ${WEBO_PLATFORM_LABEL} — ${t.cta.drive}` } : {})}
      className={`group inline-flex items-center gap-3 rounded-md bg-gradient-primary font-semibold text-primary-foreground shadow-cta transition-all duration-300 hover:-translate-y-0.5 hover:brightness-110 ${big ? "px-7 py-4 text-base" : "px-5 py-3 text-sm"}`}
    >
      <WindowsIcon />
      <span className="flex flex-col items-start leading-tight">
        <span>{label}</span>
        <span className="text-[11px] font-medium opacity-75" dir="ltr">{WEBO_PLATFORM_LABEL}</span>
      </span>
    </a>
  );
}

/* ---------------- Scroll progress ---------------- */
function ScrollProgress() {
  const bar = useRef<HTMLSpanElement>(null);
  useEffect(() => {
    let raf = 0;
    const update = () => {
      raf = 0;
      const el = document.documentElement;
      const max = el.scrollHeight - el.clientHeight;
      const p = max > 0 ? Math.min(1, Math.max(0, window.scrollY / max)) : 0;
      if (bar.current) bar.current.style.transform = `scaleX(${p})`;
    };
    const on = () => { if (!raf) raf = requestAnimationFrame(update); };
    update();
    window.addEventListener("scroll", on, { passive: true });
    window.addEventListener("resize", on);
    return () => { window.removeEventListener("scroll", on); window.removeEventListener("resize", on); cancelAnimationFrame(raf); };
  }, []);
  return (
    <span aria-hidden className="pointer-events-none absolute inset-x-0 -bottom-px h-[2px] overflow-hidden">
      <span ref={bar} data-progress className="scroll-progress block h-full origin-left rtl:origin-right" style={{ transform: "scaleX(0)" }} />
    </span>
  );
}

/** Sets --mx/--my on the hovered .liquid-card (fine pointer, motion allowed only). */
function usePointerLight(ref: RefObject<HTMLElement | null>) {
  useEffect(() => {
    const root = ref.current;
    if (!root || !window.matchMedia("(hover: hover) and (pointer: fine) and (prefers-reduced-motion: no-preference)").matches) return;
    const move = (e: PointerEvent) => {
      const c = (e.target as HTMLElement).closest<HTMLElement>(".liquid-card");
      if (!c) return;
      const r = c.getBoundingClientRect();
      c.style.setProperty("--mx", `${e.clientX - r.left}px`);
      c.style.setProperty("--my", `${e.clientY - r.top}px`);
    };
    root.addEventListener("pointermove", move, { passive: true });
    return () => root.removeEventListener("pointermove", move);
  }, [ref]);
}

/* ---------------- Header ---------------- */
export function Header() {
  const { t, toggle } = useLang();
  const [open, setOpen] = useState(false);
  const links = [
    ["#features", t.nav.features],
    ["#how", t.nav.how],
    ["#faq", t.nav.faq],
  ] as const;
  return (
    <header className="sticky top-0 z-50 border-b border-border bg-background/70 backdrop-blur-xl">
      <a href="#main" className="sr-only focus:not-sr-only focus:absolute focus:start-4 focus:top-3 focus:rounded focus:bg-primary focus:px-3 focus:py-2 focus:text-primary-foreground">{t.skip}</a>
      <ScrollProgress />
      <div className={`${container} flex h-16 items-center justify-between gap-4`}>
        <a href="#top" className="flex items-center gap-2 text-lg font-bold tracking-tight" dir="ltr" aria-label="Webo">
          <span className="grid size-7 place-items-center rounded-md bg-gradient-primary text-sm text-primary-foreground">W</span>
          Webo
        </a>
        <nav aria-label={t.nav.main} className="hidden md:block">
          <ul className="flex items-center gap-8 text-sm text-muted-foreground">
            {links.map(([h, l]) => (
              <li key={h}><a href={h} className="transition-colors hover:text-foreground">{l}</a></li>
            ))}
          </ul>
        </nav>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={toggle}
            aria-label={t.nav.toggle}
            className="liquid h-9 min-w-9 rounded-md px-3 text-sm font-semibold transition-colors hover:text-primary"
          >
            {t.nav.toggleShort}
          </button>
          <a href={WEBO_DOWNLOAD_URL || "#download"} {...(WEBO_DOWNLOAD_URL ? { rel: "nofollow" } : {})} className="hidden h-9 items-center rounded-md bg-foreground px-4 text-sm font-semibold text-background transition-opacity hover:opacity-90 sm:inline-flex">
            {t.nav.download}
          </a>
          <button
            type="button"
            className="grid size-9 place-items-center rounded-md border border-border md:hidden"
            aria-expanded={open}
            aria-controls="mobile-nav"
            aria-label={open ? t.nav.close : t.nav.menu}
            onClick={() => setOpen((o) => !o)}
          >
            <svg viewBox="0 0 24 24" className="size-4" stroke="currentColor" strokeWidth="2" fill="none" aria-hidden>
              {open ? <path d="M6 6l12 12M18 6L6 18" /> : <path d="M4 7h16M4 12h16M4 17h16" />}
            </svg>
          </button>
        </div>
      </div>
      {open && (
        <nav id="mobile-nav" aria-label={t.nav.main} className="border-t border-border md:hidden">
          <ul className={`${container} flex flex-col py-3`}>
            {links.map(([h, l]) => (
              <li key={h}><a href={h} onClick={() => setOpen(false)} className="block py-3 text-base">{l}</a></li>
            ))}
            <li><a href={WEBO_DOWNLOAD_URL || "#download"} {...(WEBO_DOWNLOAD_URL ? { rel: "nofollow" } : {})} onClick={() => setOpen(false)} className="block py-3 text-base text-primary">{t.nav.download}</a></li>
          </ul>
        </nav>
      )}
    </header>
  );
}

/* ---------------- Hero ---------------- */
export function Hero() {
  const { t } = useLang();
  return (
    <section id="top" className="bg-noise relative overflow-hidden" aria-labelledby="hero-title">
      <div aria-hidden className="bg-grid absolute inset-0 [mask-image:radial-gradient(ellipse_at_60%_30%,black,transparent_70%)]" />
      <div aria-hidden className="absolute -top-40 end-[-10%] h-[520px] w-[720px] rounded-full bg-primary/15 blur-[120px]" />
      <div className={`${container} relative grid items-center gap-14 pb-24 pt-16 lg:grid-cols-[1.05fr_1fr] lg:pb-32 lg:pt-24`}>
        <div>
          <p className="anim-rise inline-flex items-center gap-2 rounded-full border border-primary/30 bg-primary/10 px-3 py-1 text-xs font-medium text-primary">
            <span className="size-1.5 rounded-full bg-cyan" aria-hidden /> {t.hero.badge}
          </p>
          <h1 id="hero-title" className="anim-rise mt-7 text-5xl font-bold leading-[1.1] tracking-tight sm:text-6xl lg:text-7xl" style={{ ["--d" as string]: "120ms" }}>
            <span className="block">{t.hero.title1}</span>
            <span className="text-ink block pb-2">{t.hero.title2}</span>
          </h1>
          <p className="anim-rise mt-6 max-w-xl text-lg leading-relaxed text-muted-foreground" style={{ ["--d" as string]: "240ms" }}>{t.hero.sub}</p>
          <div className="anim-rise mt-9 flex flex-wrap items-center gap-4" style={{ ["--d" as string]: "360ms" }}>
            <DownloadButton label={t.hero.cta} />
            <a href="#features" className="group inline-flex items-center gap-2 rounded-md px-4 py-3 text-sm font-semibold text-foreground transition-colors hover:text-primary">
              {t.hero.explore}
              <span aria-hidden className="transition-transform group-hover:translate-y-0.5">↓</span>
            </a>
          </div>
          <p className="anim-rise mt-6 text-xs text-muted-foreground" style={{ ["--d" as string]: "460ms" }}>{t.hero.note}</p>
        </div>
        <HeroArt />
      </div>
      <div className="relative border-y border-border">
        <ul className={`${container} flex flex-wrap justify-between gap-x-8 gap-y-3 py-5 text-sm text-muted-foreground`}>
          {t.strip.map((s, i) => (
            <li key={s} className="flex items-center gap-2"><span className="text-xs text-primary/70" dir="ltr">0{i + 1}</span>{s}</li>
          ))}
        </ul>
      </div>
    </section>
  );
}

/* ---------------- Features (editorial bento) ---------------- */
export function Features() {
  const { t } = useLang();
  const ref = useReveal<HTMLElement>();
  usePointerLight(ref);
  const f = t.features;
  const card = "reveal group liquid liquid-card overflow-hidden rounded-xl p-7 transition-[translate,border-color] duration-500 ease-out hover:-translate-y-1 hover:border-primary/40";
  return (
    <section id="features" ref={ref} className="py-24 lg:py-36" aria-labelledby="features-title">
      <div className={container}>
        <div className="reveal grid gap-6 lg:grid-cols-[1fr_1.4fr] lg:items-end">
          <Kicker>{f.kicker}</Kicker>
          <h2 id="features-title" className="text-3xl font-bold leading-tight tracking-tight sm:text-5xl">{f.title}</h2>
        </div>
        <div className="mt-16 grid gap-4 md:grid-cols-6">
          <article className={`${card} md:col-span-4 md:row-span-2`}>
            <span className="text-xs text-primary" dir="ltr">01</span>
            <h3 className="mt-3 text-2xl font-semibold">{f.pages.t}</h3>
            <p className="mt-3 max-w-md text-muted-foreground">{f.pages.d}</p>
            <div className="mt-10 flex items-end gap-3" dir="ltr" aria-hidden>
              {["Home", "About", "Services", "Contact"].map((p, i) => (
                <div key={p} className="flex-1 rounded-lg border border-border bg-background/60 p-3 transition-transform duration-500 group-hover:-translate-y-1" style={{ transitionDelay: `${i * 60}ms`, height: `${150 - i * 18}px` }}>
                  <div className="text-[10px] text-muted-foreground">/{p.toLowerCase()}</div>
                  <div className="mt-2 h-1.5 w-3/4 rounded bg-foreground/70" />
                  <div className="mt-1.5 h-1 w-1/2 rounded bg-muted-foreground/30" />
                </div>
              ))}
            </div>
          </article>
          <article className={`${card} md:col-span-2`} style={{ ["--d" as string]: "80ms" }}>
            <span className="text-xs text-primary" dir="ltr">02</span>
            <h3 className="mt-3 text-xl font-semibold">{f.visual.t}</h3>
            <p className="mt-2 text-sm text-muted-foreground">{f.visual.d}</p>
          </article>
          <article className={`${card} md:col-span-2`} style={{ ["--d" as string]: "160ms" }}>
            <span className="text-xs text-primary" dir="ltr">03</span>
            <h3 className="mt-3 text-xl font-semibold">{f.sections.t}</h3>
            <p className="mt-2 text-sm text-muted-foreground">{f.sections.d}</p>
          </article>
          <article className={`${card} md:col-span-3`} style={{ ["--d" as string]: "80ms" }}>
            <span className="text-xs text-primary" dir="ltr">04</span>
            <h3 className="mt-3 text-xl font-semibold">{f.brand.t}</h3>
            <p className="mt-2 text-sm text-muted-foreground">{f.brand.d}</p>
            <div className="mt-6 flex items-center gap-3" aria-hidden>
              <span className="size-9 rounded-md bg-primary" /><span className="size-9 rounded-md bg-cyan" /><span className="size-9 rounded-md bg-foreground" />
              <span className="ms-4 text-3xl font-bold">Aa</span><span className="text-3xl font-bold">أب</span>
            </div>
          </article>
          <article className={`${card} md:col-span-3`} style={{ ["--d" as string]: "160ms" }}>
            <span className="text-xs text-primary" dir="ltr">05</span>
            <h3 className="mt-3 text-xl font-semibold">{f.local.t}</h3>
            <p className="mt-2 text-sm text-muted-foreground">{f.local.d}</p>
            <div className="mt-6 inline-flex items-center gap-2 rounded-md border border-border px-3 py-2 text-xs text-muted-foreground" dir="ltr" aria-hidden>
              <span className="size-1.5 rounded-full bg-cyan" /> C:\Users\you\Webo\my-site.webo
            </div>
          </article>
          <article className={`${card} md:col-span-6 flex flex-col gap-4 md:flex-row md:items-center md:justify-between`}>
            <div>
              <span className="text-xs text-primary" dir="ltr">06</span>
              <h3 className="mt-3 text-xl font-semibold">{f.export.t}</h3>
              <p className="mt-2 text-sm text-muted-foreground">{f.export.d}</p>
            </div>
            <div className="flex gap-2 text-sm" dir="ltr" aria-hidden>
              {[".html", ".css", "/assets"].map((x) => <span key={x} className="rounded-md border border-border bg-background/60 px-3 py-1.5">{x}</span>)}
            </div>
          </article>
        </div>
      </div>
    </section>
  );
}

/* ---------------- Screens ---------------- */
export function Screens() {
  const { t } = useLang();
  const ref = useReveal<HTMLElement>();
  const [mode, setMode] = useState<"desktop" | "tablet" | "mobile">("desktop");
  const widths = { desktop: "100%", tablet: "62%", mobile: "32%" };
  const cols = { desktop: "grid-cols-3", tablet: "grid-cols-2", mobile: "grid-cols-1" };
  return (
    <section ref={ref} className="relative border-y border-border bg-surface/30 py-24 lg:py-36" aria-labelledby="screens-title">
      <div className={`${container} grid gap-14 lg:grid-cols-[1fr_1.6fr] lg:items-center`}>
        <div className="reveal">
          <Kicker>{t.screens.kicker}</Kicker>
          <h2 id="screens-title" className="mt-5 text-3xl font-bold leading-tight tracking-tight sm:text-5xl">{t.screens.title}</h2>
          <p className="mt-5 text-muted-foreground">{t.screens.sub}</p>
          <div role="radiogroup" aria-label={t.screens.group} className="mt-8 inline-flex rounded-md border border-border p-1">
            {(["desktop", "tablet", "mobile"] as const).map((m) => (
              <button
                key={m}
                role="radio"
                aria-checked={mode === m}
                onClick={() => setMode(m)}
                className={`rounded px-4 py-2 text-sm font-medium transition-colors ${mode === m ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:text-foreground"}`}
              >
                {t.screens[m]}
              </button>
            ))}
          </div>
        </div>
        <div className="reveal" style={{ ["--d" as string]: "150ms" }} aria-hidden>
          <div className="bg-grid flex h-[420px] items-center justify-center rounded-xl border border-border p-6">
            <div className="shadow-float h-full overflow-hidden rounded-lg border border-border bg-background transition-[width] duration-700 ease-[var(--ease-out)]" style={{ width: widths[mode] }}>
              <div className="flex items-center justify-between border-b border-border px-4 py-3">
                <div className="h-2 w-12 rounded bg-foreground/80" />
                <div className="flex gap-2">{mode !== "mobile" ? [0, 1, 2].map((i) => <div key={i} className="h-1.5 w-8 rounded bg-muted-foreground/40" />) : <div className="h-2 w-4 rounded bg-muted-foreground/40" />}</div>
              </div>
              <div className="space-y-4 p-5">
                <div className="h-3 w-3/5 rounded bg-foreground/80" />
                <div className="h-2 w-4/5 rounded bg-muted-foreground/30" />
                <div className="h-6 w-24 rounded bg-gradient-primary" />
                <div className={`grid gap-3 pt-3 ${cols[mode]}`}>
                  {[0, 1, 2].map((i) => (
                    <div key={i} className="h-24 rounded-md border border-border bg-surface p-3">
                      <div className="size-4 rounded bg-primary/50" />
                      <div className="mt-3 h-1.5 w-3/4 rounded bg-muted-foreground/40" />
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

/* ---------------- How ---------------- */
export function How() {
  const { t } = useLang();
  const ref = useReveal<HTMLElement>();
  return (
    <section id="how" ref={ref} className="py-24 lg:py-36" aria-labelledby="how-title">
      <div className={container}>
        <div className="reveal max-w-2xl">
          <Kicker>{t.how.kicker}</Kicker>
          <h2 id="how-title" className="mt-5 text-3xl font-bold leading-tight tracking-tight sm:text-5xl">{t.how.title}</h2>
        </div>
        <ol className="mt-16 grid gap-px overflow-hidden rounded-xl border border-border bg-border md:grid-cols-3">
          {t.how.steps.map((s, i) => (
            <li key={s.t} className="reveal group bg-background p-8 transition-colors hover:bg-surface/60" style={{ ["--d" as string]: `${i * 120}ms` }}>
              <span className="text-6xl font-bold text-primary/25 transition-colors group-hover:text-primary/60" dir="ltr">{i + 1}</span>
              <h3 className="mt-6 text-xl font-semibold">{s.t}</h3>
              <p className="mt-2 text-muted-foreground">{s.d}</p>
            </li>
          ))}
        </ol>
      </div>
    </section>
  );
}

/* ---------------- Arabic-first ---------------- */
export function ArabicFirst() {
  const { t } = useLang();
  const ref = useReveal<HTMLElement>();
  return (
    <section ref={ref} className="bg-noise relative overflow-hidden border-y border-border py-24 lg:py-36" aria-labelledby="rtl-title">
      <div className={`${container} grid gap-14 lg:grid-cols-2 lg:items-center`}>
        <div className="reveal relative rounded-xl border border-border bg-surface/50 p-10" lang="ar" dir="rtl" aria-hidden>
          <div className="text-6xl font-bold leading-tight sm:text-8xl text-ink">{t.rtl.sample}</div>
          <div className="mt-6 flex items-center justify-between border-t border-border pt-4 text-xs text-muted-foreground">
            <span>→ RTL</span><span dir="ltr">IBM Plex Sans Arabic · 700</span>
          </div>
          <div className="mt-6 flex gap-4 text-3xl text-muted-foreground">
            <span className="font-normal">أ</span><span className="font-medium">ب</span><span className="font-semibold">ج</span><span className="font-bold text-foreground">د</span>
          </div>
        </div>
        <div className="reveal" style={{ ["--d" as string]: "120ms" }}>
          <Kicker>{t.rtl.kicker}</Kicker>
          <h2 id="rtl-title" className="mt-5 text-3xl font-bold leading-tight tracking-tight sm:text-5xl">{t.rtl.title}</h2>
          <p className="mt-5 text-muted-foreground">{t.rtl.sub}</p>
          <ul className="mt-8 space-y-3">
            {t.rtl.points.map((p) => (
              <li key={p} className="flex items-center gap-3"><span className="size-1.5 rounded-full bg-cyan" aria-hidden />{p}</li>
            ))}
          </ul>
        </div>
      </div>
    </section>
  );
}

/* ---------------- Export ---------------- */
export function Export() {
  const { t } = useLang();
  const ref = useReveal<HTMLElement>();
  const files: [string, number, string][] = [
    ["my-site/", 0, "dir"], ["index.html", 1, "html"], ["about.html", 1, "html"], ["services.html", 1, "html"],
    ["contact.html", 1, "html"], ["css/", 1, "dir"], ["styles.css", 2, "css"], ["assets/", 1, "dir"], ["logo.svg", 2, "img"],
  ];
  const color = { dir: "text-muted-foreground", html: "text-primary", css: "text-cyan", img: "text-foreground" } as const;
  return (
    <section ref={ref} className="py-24 lg:py-36" aria-labelledby="export-title">
      <div className={`${container} grid gap-14 lg:grid-cols-[1fr_1.1fr] lg:items-center`}>
        <div className="reveal">
          <Kicker>{t.exp.kicker}</Kicker>
          <h2 id="export-title" className="mt-5 text-3xl font-bold leading-tight tracking-tight sm:text-5xl">{t.exp.title}</h2>
          <p className="mt-5 max-w-md text-muted-foreground">{t.exp.sub}</p>
        </div>
        <div className="reveal grid gap-4 sm:grid-cols-[0.9fr_1.1fr]" style={{ ["--d" as string]: "120ms" }} dir="ltr">
          <ul aria-label={t.exp.tree} className="rounded-xl border border-border bg-surface/60 p-5 text-sm">
            {files.map(([n, d, k]) => (
              <li key={n} className={`py-1 ${color[k as keyof typeof color]}`} style={{ paddingInlineStart: d * 16 }}>{n}</li>
            ))}
          </ul>
          <pre aria-hidden className="overflow-x-auto rounded-xl border border-border bg-background p-5 text-xs leading-6 text-muted-foreground">
{`<section class="hero">
  <h1>`}<span className="text-foreground">From idea…</span>{`</h1>
  <a class="btn">Start</a>
</section>

`}<span className="text-cyan">.hero</span>{` {
  display: grid;
  gap: var(--space-6);
  color: var(--brand);
}`}
          </pre>
        </div>
      </div>
    </section>
  );
}

/* ---------------- FAQ ---------------- */
export function Faq() {
  const { t } = useLang();
  const ref = useReveal<HTMLElement>();
  const [open, setOpen] = useState<number | null>(0);
  return (
    <section id="faq" ref={ref} className="border-t border-border py-24 lg:py-36" aria-labelledby="faq-title">
      <div className={`${container} grid gap-14 lg:grid-cols-[1fr_1.6fr]`}>
        <div className="reveal">
          <Kicker>{t.faq.kicker}</Kicker>
          <h2 id="faq-title" className="mt-5 text-3xl font-bold leading-tight tracking-tight sm:text-5xl">{t.faq.title}</h2>
        </div>
        <div className="reveal divide-y divide-border border-y border-border" style={{ ["--d" as string]: "120ms" }}>
          {t.faq.items.map((it, i) => {
            const isOpen = open === i;
            return (
              <div key={i}>
                <h3>
                  <button
                    type="button"
                    id={`faq-q-${i}`}
                    aria-expanded={isOpen}
                    aria-controls={`faq-a-${i}`}
                    onClick={() => setOpen(isOpen ? null : i)}
                    className="flex w-full items-center justify-between gap-6 py-6 text-start text-lg font-medium transition-colors hover:text-primary"
                  >
                    {it.q}
                    <span aria-hidden className={`grid size-7 shrink-0 place-items-center rounded-full border border-border transition-transform duration-300 ${isOpen ? "rotate-45 border-primary text-primary" : ""}`}>+</span>
                  </button>
                </h3>
                <div
                  id={`faq-a-${i}`}
                  role="region"
                  aria-labelledby={`faq-q-${i}`}
                  className={`grid transition-[grid-template-rows] duration-500 ease-[var(--ease-out)] ${isOpen ? "grid-rows-[1fr]" : "grid-rows-[0fr]"}`}
                >
                  <div className="overflow-hidden" hidden={!isOpen}>
                    <p className="max-w-2xl pb-6 leading-relaxed text-muted-foreground">{it.a}</p>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}

/* ---------------- CTA + Footer ---------------- */
export function FinalCta() {
  const { t } = useLang();
  const ref = useReveal<HTMLElement>();
  return (
    <section id="download" ref={ref} className="px-5 pb-24 sm:px-8" aria-labelledby="cta-title">
      <div className="reveal bg-noise relative mx-auto max-w-7xl overflow-hidden rounded-2xl border border-primary/25 bg-surface px-8 py-20 text-center sm:px-16">
        <div aria-hidden className="bg-grid absolute inset-0 [mask-image:radial-gradient(ellipse_at_center,black,transparent_70%)]" />
        <div aria-hidden className="absolute start-1/2 top-0 h-64 w-[600px] -translate-x-1/2 rounded-full bg-primary/20 blur-[100px] rtl:translate-x-1/2" />
        <div className="relative">
          <h2 id="cta-title" className="mx-auto max-w-3xl text-3xl font-bold leading-tight tracking-tight sm:text-5xl">{t.cta.title}</h2>
          <p className="mx-auto mt-5 max-w-lg text-muted-foreground">{t.cta.sub}</p>
          <div className="mt-10 flex justify-center"><DownloadButton label={t.cta.btn} big /></div>
          <p className="mt-4 text-xs text-muted-foreground" role="note">{WEBO_DOWNLOAD_URL ? t.cta.drive : t.cta.soon}</p>
        </div>
      </div>
    </section>
  );
}

export function Footer() {
  const { t } = useLang();
  return (
    <footer className="border-t border-border">
      <div className={`${container} flex flex-col gap-6 py-10 text-sm text-muted-foreground md:flex-row md:items-center md:justify-between`}>
        <div className="flex items-center gap-3">
          <span className="font-bold text-foreground" dir="ltr">Webo</span>
          <span>{t.footer.tag}</span>
        </div>
        <div className="flex flex-wrap items-center gap-4">
          <span className="rounded-full border border-border px-2.5 py-0.5 text-xs">{t.footer.preview}</span>
          <span dir="ltr">{WEBO_PLATFORM_LABEL}</span>
          <span>© 2026 Webo. {t.footer.rights}</span>
        </div>
      </div>
    </footer>
  );
}