import { useTilt } from "@/hooks/use-motion";
import { useLang } from "@/lib/i18n";

/** Original CSS/SVG product composition. Rendered LTR internally as "UI artwork". */
export function HeroArt() {
  const ref = useTilt<HTMLDivElement>(5);
  const { t } = useLang();
  return (
    <div role="img" aria-label={t.hero.art} className="relative mx-auto w-full max-w-[620px] [perspective:1400px]">
      <div
        ref={ref}
        dir="ltr"
        className="relative aspect-[5/4] transition-transform duration-500 ease-out [transform-style:preserve-3d]"
      >
        {/* pointer glow */}
        <div aria-hidden className="pointer-events-none absolute -inset-10 opacity-60 [background:radial-gradient(400px_circle_at_var(--gx,60%)_var(--gy,40%),oklch(0.72_0.15_272/0.22),transparent_60%)]" />

        {/* Browser canvas */}
        <div className="anim-rise shadow-float absolute inset-x-[4%] border border-border bg-surface top-[6%] bottom-[14%] overflow-hidden rounded-xl" style={{ ["--d" as string]: "100ms" }}>
          <div className="flex items-center gap-1.5 border-b border-border px-3 py-2.5">
            <span className="size-2 rounded-full bg-muted-foreground/40" />
            <span className="size-2 rounded-full bg-muted-foreground/40" />
            <span className="size-2 rounded-full bg-muted-foreground/40" />
            <div className="mx-auto h-4 w-1/2 rounded bg-surface-2 text-[9px] leading-4 text-muted-foreground text-center">studio.webo / home</div>
          </div>
          <div className="grid h-full grid-cols-[22%_1fr]">
            <div className="space-y-2 border-e border-border p-3">
              {["Home", "About", "Services", "Contact"].map((p, i) => (
                <div key={p} className={`rounded px-2 py-1 text-[9px] ${i === 0 ? "bg-primary/20 text-primary" : "text-muted-foreground"}`}>{p}</div>
              ))}
              <div className="mt-4 space-y-1.5 pt-2">
                <div className="flex gap-1">
                  <span className="size-3 rounded-sm bg-primary" /><span className="size-3 rounded-sm bg-cyan" /><span className="size-3 rounded-sm bg-foreground/80" />
                </div>
                <div className="h-1 w-3/4 rounded bg-muted-foreground/25" />
              </div>
            </div>
            <div className="relative space-y-3 p-4 bg-grid">
              <div className="relative rounded-md border border-border bg-surface/80 p-3">
                <div className="h-2 w-2/5 rounded bg-foreground/80" />
                <div className="mt-2 h-1.5 w-3/5 rounded bg-muted-foreground/40" />
                <div className="mt-3 h-4 w-16 rounded bg-gradient-primary" />
                {/* selection outline */}
                <svg className="anim-sel pointer-events-none absolute -inset-1.5 h-[calc(100%+12px)] w-[calc(100%+12px)]" aria-hidden>
                  <rect x="1" y="1" width="calc(100% - 2px)" height="calc(100% - 2px)" rx="7" fill="none" stroke="var(--cyan)" strokeWidth="1.2" className="anim-dash" />
                </svg>
                <span className="absolute -top-4 start-0 rounded-sm bg-cyan px-1.5 text-[8px] font-semibold leading-3.5 text-accent-foreground">Hero</span>
              </div>
              <div className="grid grid-cols-3 gap-2">
                {[0, 1, 2].map((i) => (
                  <div key={i} className="anim-rise rounded-md border border-border bg-surface/70 p-2" style={{ ["--d" as string]: `${500 + i * 120}ms` }}>
                    <div className="size-3 rounded-sm bg-primary/60" />
                    <div className="mt-2 h-1 w-4/5 rounded bg-muted-foreground/40" />
                    <div className="mt-1 h-1 w-3/5 rounded bg-muted-foreground/25" />
                  </div>
                ))}
              </div>
              <div className="h-10 rounded-md border border-dashed border-primary/40" />
            </div>
          </div>
          {/* cursor */}
          <svg className="anim-cursor absolute end-[18%] top-[30%] size-5 drop-shadow" viewBox="0 0 24 24" aria-hidden>
            <path d="M4 2l16 9-7 2-3 7z" fill="var(--foreground)" stroke="var(--background)" strokeWidth="1.2" />
          </svg>
        </div>

        {/* Phone frame */}
        <div className="anim-rise absolute bottom-[2%] end-[0%] w-[22%] [transform:translateZ(60px)]" style={{ ["--d" as string]: "350ms" }}>
          <div className="anim-float liquid aspect-[9/18] rounded-[14px] p-1.5">
            <div className="h-full space-y-1.5 rounded-[10px] bg-surface p-2">
              <div className="h-1.5 w-3/5 rounded bg-foreground/80" />
              <div className="h-1 w-4/5 rounded bg-muted-foreground/40" />
              <div className="h-3 w-1/2 rounded bg-gradient-primary" />
              <div className="h-8 rounded bg-surface-2" />
              <div className="h-8 rounded bg-surface-2" />
            </div>
          </div>
        </div>

        {/* Tablet frame */}
        <div className="anim-rise absolute bottom-[0%] start-[0%] w-[30%] [transform:translateZ(40px)]" style={{ ["--d" as string]: "250ms" }}>
          <div className="anim-float liquid aspect-[4/3] rounded-[12px] p-1.5" style={{ animationDuration: "8s", animationDelay: "1s" }}>
            <div className="grid h-full grid-cols-2 gap-1.5 rounded-[8px] bg-surface p-2">
              <div className="col-span-2 h-2 w-1/2 rounded bg-foreground/80" />
              <div className="rounded bg-surface-2" /><div className="rounded bg-primary/30" />
            </div>
          </div>
        </div>

        {/* Export chips */}
        <div className="absolute -top-[2%] end-[2%] flex flex-col gap-2 [transform:translateZ(80px)]">
          {[["index.html", "var(--primary)"], ["styles.css", "var(--cyan)"]].map(([f, c], i) => (
            <div key={f} className="anim-rise liquid flex items-center gap-2 rounded-md px-2.5 py-1.5 text-[10px] font-medium" style={{ ["--d" as string]: `${800 + i * 150}ms` }}>
              <span className="size-1.5 rounded-full" style={{ background: c }} />
              {f}
            </div>
          ))}
        </div>

        {/* Brand token chip */}
        <div className="anim-rise liquid absolute start-[-2%] top-[22%] rounded-md px-2.5 py-2 [transform:translateZ(70px)]" style={{ ["--d" as string]: "1000ms" }}>
          <div className="text-[8px] uppercase tracking-wider text-muted-foreground">Radius</div>
          <div className="mt-1 flex items-end gap-1">
            <span className="size-3 rounded-none border border-primary" />
            <span className="size-3 rounded-sm border border-primary bg-primary/30" />
            <span className="size-3 rounded-full border border-primary" />
          </div>
        </div>
      </div>
    </div>
  );
}