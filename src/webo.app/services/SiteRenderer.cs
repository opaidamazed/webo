using System.Net;
using System.Text;
using Webo.App.Models;

namespace Webo.App;

public sealed class RenderOptions
{
    /// <summary>Editor preview: sections are clickable, links do not navigate, a tiny bridge talks to the app.</summary>
    public bool EditMode { get; init; }
    public string? SelectedSectionId { get; init; }
    /// <summary>Text shown (edit mode only) when the page has no sections yet.</summary>
    public string EmptyHint { get; init; } = "";
    /// <summary>Preview only: @font-face rules with the fonts inlined, so the preview works offline.</summary>
    public string FontCss { get; init; } = "";
    /// <summary>Export: link to this shared stylesheet (relative URL) instead of inlining the CSS in every page.</summary>
    public string? ExternalCssPath { get; init; }
}

/// <summary>
/// Turns the project model into HTML. The model is the single source of truth: the editor preview and the
/// export both come from here, so what you see in the preview is what gets exported.
/// </summary>
public static class SiteRenderer
{
    /// <summary>"index.html" for the home page, "&lt;slug&gt;.html" for the others (a non-home page called "index" is renamed so it can never replace the home page).</summary>
    public static string PageFileName(PageDoc p)
    {
        if (p.IsHome) return "index.html";
        var slug = string.IsNullOrWhiteSpace(p.Slug) ? "page" : p.Slug;
        if (slug.Equals("index", StringComparison.OrdinalIgnoreCase)) slug = "index-page";
        return slug + ".html";
    }

    private static string H(string? s) => WebUtility.HtmlEncode(s ?? "");

    // ---- stylesheet ---------------------------------------------------------------------------------

    /// <summary>The small, theme-dependent part of the CSS. The editor swaps just this when the identity changes.</summary>
    public static string BuildThemeCss(ThemeDoc? theme)
    {
        var t = theme ?? new ThemeDoc();
        var (accent, dark) = ThemeCatalog.Resolve(t);
        return ":root{--accent:" + accent +
               ";--accent-ink:" + ThemeCatalog.InkFor(accent) +
               ";--dark:" + dark +
               ";--radius:" + ThemeCatalog.RadiusPx(t.Radius) +
               ";--font:" + ThemeCatalog.FontStack(t.Font) +
               ";--soft:color-mix(in srgb,var(--accent) 8%,#fff)" +
               ";--line:color-mix(in srgb,var(--accent) 22%,#fff)}";
    }

    /// <summary>The complete stylesheet for an exported site (fonts + theme + layout).</summary>
    public static string BuildStylesheet(ProjectDoc doc, string fontCss) =>
        fontCss + "\n" + BuildThemeCss(doc.Theme) + "\n" + BaseCss;

    // ---- documents ----------------------------------------------------------------------------------

    /// <summary>A complete HTML document for one page.</summary>
    public static string RenderDocument(ProjectDoc doc, IReadOnlyList<PageDoc> pages, PageDoc page, RenderOptions opt)
    {
        var lang = doc.Language == "en" ? "en" : "ar";
        var dir = doc.Direction == "ltr" ? "ltr" : "rtl";

        var sb = new StringBuilder(8192);
        sb.Append("<!DOCTYPE html>\n<html lang=\"").Append(lang).Append("\" dir=\"").Append(dir).Append("\">\n<head>\n");
        sb.Append("<meta charset=\"utf-8\">\n");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n");
        if (opt.EditMode)
        {
            // The preview never needs the network: block everything except its own inline style/script and data: images/fonts.
            sb.Append("<meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src 'unsafe-inline'; script-src 'unsafe-inline'; img-src data:; font-src data:\">\n");
        }
        sb.Append("<title>").Append(H(page.Title)).Append(" — ").Append(H(doc.Name)).Append("</title>\n");

        if (opt.ExternalCssPath != null)
        {
            sb.Append("<link rel=\"stylesheet\" href=\"").Append(H(opt.ExternalCssPath)).Append("\">\n");
        }
        else
        {
            sb.Append("<style id=\"webo-fonts\">").Append(opt.FontCss).Append("</style>\n");
            sb.Append("<style id=\"webo-theme\">").Append(BuildThemeCss(doc.Theme)).Append("</style>\n");
            sb.Append("<style id=\"webo-base\">").Append(BaseCss);
            if (opt.EditMode) sb.Append(EditCss);
            sb.Append("</style>\n");
        }

        sb.Append("</head>\n<body>\n<div id=\"webo-root\">");
        sb.Append(RenderBody(doc, pages, page, opt));
        sb.Append("</div>\n");
        if (opt.EditMode) sb.Append("<script>").Append(EditScript).Append("</script>\n");
        sb.Append("</body>\n</html>\n");
        return sb.ToString();
    }

    /// <summary>Header + the page's sections. The editor swaps this fragment in place so scrolling is not lost.</summary>
    public static string RenderBody(ProjectDoc doc, IReadOnlyList<PageDoc> pages, PageDoc page, RenderOptions opt)
    {
        var ar = doc.Language != "en";
        var sb = new StringBuilder(4096);

        sb.Append("<header class=\"site-header\"><div class=\"wrap bar\"><span class=\"brand\">").Append(H(doc.Name)).Append("</span><nav>");
        foreach (var p in pages)
        {
            var current = p.Id == page.Id ? " class=\"current\"" : "";
            if (opt.EditMode)
                sb.Append("<a href=\"#\" data-page-id=\"").Append(H(p.Id)).Append('"').Append(current).Append('>').Append(H(p.Title)).Append("</a>");
            else
                sb.Append("<a href=\"").Append(H(PageFileName(p))).Append('"').Append(current).Append('>').Append(H(p.Title)).Append("</a>");
        }
        sb.Append("</nav></div></header>\n<main>\n");

        if (page.Sections.Count == 0 && opt.EditMode)
            sb.Append("<div class=\"webo-empty\">").Append(H(opt.EmptyHint)).Append("</div>\n");

        foreach (var s in page.Sections) AppendSection(sb, s, opt, ar);

        sb.Append("</main>\n");
        return sb.ToString();
    }

    private static void AppendSection(StringBuilder sb, SectionDoc s, RenderOptions opt, bool ar)
    {
        var tag = s.Type == "footer" ? "footer" : "section";
        sb.Append('<').Append(tag).Append(" class=\"sec sec-").Append(H(s.Type));
        if (opt.EditMode)
        {
            sb.Append(" webo-sec");
            if (s.Id == opt.SelectedSectionId) sb.Append(" selected");
        }
        sb.Append('"');
        if (opt.EditMode) sb.Append(" data-webo-id=\"").Append(H(s.Id)).Append('"');
        sb.Append('>');

        switch (s.Type)
        {
            case "hero":
                sb.Append("<div class=\"wrap\"><h1>").Append(H(s.Get("title"))).Append("</h1>");
                AppendParagraphs(sb, s.Get("subtitle"));
                AppendButton(sb, s.Get("btn"), s.Get("link"), opt, "");
                sb.Append("</div>");
                break;

            case "text":
                sb.Append("<div class=\"wrap\">");
                AppendHeading(sb, s.Get("heading"));
                AppendParagraphs(sb, s.Get("body"));
                sb.Append("</div>");
                break;

            case "features":
                sb.Append("<div class=\"wrap\">");
                AppendHeading(sb, s.Get("heading"));
                sb.Append("<div class=\"grid\">");
                for (var i = 1; i <= 3; i++)
                {
                    var t = s.Get("f" + i + "t");
                    var d = s.Get("f" + i + "d");
                    if (t.Length == 0 && d.Length == 0) continue;
                    sb.Append("<div class=\"card\"><h3>").Append(H(t)).Append("</h3>");
                    AppendParagraphs(sb, d);
                    sb.Append("</div>");
                }
                sb.Append("</div></div>");
                break;

            case "steps":
                sb.Append("<div class=\"wrap\">");
                AppendHeading(sb, s.Get("heading"));
                sb.Append("<ol class=\"steps\">");
                for (var i = 1; i <= SectionCatalog.StepSlots; i++)
                {
                    var t = s.Get("p" + i + "t");
                    var d = s.Get("p" + i + "d");
                    if (t.Length == 0 && d.Length == 0) continue;
                    sb.Append("<li><h3>").Append(H(t)).Append("</h3>");
                    AppendParagraphs(sb, d);
                    sb.Append("</li>");
                }
                sb.Append("</ol></div>");
                break;

            case "stats":
                sb.Append("<div class=\"wrap\"><div class=\"stats\">");
                for (var i = 1; i <= SectionCatalog.StatSlots; i++)
                {
                    var v = s.Get("s" + i + "v");
                    var l = s.Get("s" + i + "l");
                    if (v.Length == 0 && l.Length == 0) continue;
                    sb.Append("<div class=\"stat\"><b>").Append(H(v)).Append("</b><span>").Append(H(l)).Append("</span></div>");
                }
                sb.Append("</div></div>");
                break;

            case "testimonials":
                sb.Append("<div class=\"wrap\">");
                AppendHeading(sb, s.Get("heading"));
                sb.Append("<div class=\"grid\">");
                for (var i = 1; i <= SectionCatalog.QuoteSlots; i++)
                {
                    var q = s.Get("q" + i);
                    var n = s.Get("n" + i);
                    if (q.Length == 0 && n.Length == 0) continue;
                    sb.Append("<blockquote class=\"card quote\">");
                    AppendParagraphs(sb, q);
                    if (n.Length > 0) sb.Append("<div class=\"who\">— ").Append(H(n)).Append("</div>");
                    sb.Append("</blockquote>");
                }
                sb.Append("</div></div>");
                break;

            case "faq":
                sb.Append("<div class=\"wrap\">");
                AppendHeading(sb, s.Get("heading"));
                for (var i = 1; i <= SectionCatalog.FaqSlots; i++)
                {
                    var q = s.Get("a" + i + "q");
                    var a = s.Get("a" + i + "a");
                    if (q.Length == 0 && a.Length == 0) continue;
                    // <details> opens and closes without any JavaScript, so the exported site needs no script at all.
                    sb.Append("<details class=\"qa\"><summary>").Append(H(q)).Append("</summary>");
                    AppendParagraphs(sb, a);
                    sb.Append("</details>");
                }
                sb.Append("</div>");
                break;

            case "cta":
                sb.Append("<div class=\"wrap\">");
                AppendHeading(sb, s.Get("heading"));
                AppendParagraphs(sb, s.Get("text"));
                AppendButton(sb, s.Get("btn"), s.Get("link"), opt, "light");
                sb.Append("</div>");
                break;

            case "contact":
                sb.Append("<div class=\"wrap\">");
                AppendHeading(sb, s.Get("heading"));
                AppendParagraphs(sb, s.Get("text"));
                sb.Append("<div class=\"contact-btns\">");
                var wa = WhatsAppHref(s.Get("whatsapp"));
                var tel = PhoneHref(s.Get("phone"));
                var mail = EmailHref(s.Get("email"));
                if (wa != null) AppendLink(sb, ar ? "تواصل عبر واتساب" : "Chat on WhatsApp", opt.EditMode ? "#" : wa, "");
                if (tel != null) AppendLink(sb, ar ? "اتصل بنا" : "Call us", opt.EditMode ? "#" : tel, "outline");
                if (mail != null) AppendLink(sb, ar ? "راسلنا" : "Email us", opt.EditMode ? "#" : mail, "outline");
                sb.Append("</div></div>");
                break;

            case "footer":
                sb.Append("<div class=\"wrap\">");
                AppendParagraphs(sb, s.Get("text"));
                sb.Append("</div>");
                break;
        }

        sb.Append("</").Append(tag).Append(">\n");
    }

    private static void AppendHeading(StringBuilder sb, string heading)
    {
        if (heading.Length > 0) sb.Append("<h2>").Append(H(heading)).Append("</h2>");
    }

    /// <summary>Blank line = new paragraph; a single line break stays a line break. Everything is HTML-encoded.</summary>
    private static void AppendParagraphs(StringBuilder sb, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        // WinUI text boxes use '\r' for Enter, files may contain "\r\n": normalize first.
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        foreach (var block in normalized.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
        {
            var t = block.Trim();
            if (t.Length == 0) continue;
            sb.Append("<p>").Append(H(t).Replace("\n", "<br>")).Append("</p>");
        }
    }

    private static void AppendButton(StringBuilder sb, string text, string link, RenderOptions opt, string extraClass)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        AppendLink(sb, text, opt.EditMode ? "#" : SafeHref(link), extraClass);
    }

    private static void AppendLink(StringBuilder sb, string text, string href, string extraClass)
    {
        sb.Append("<a class=\"btn");
        if (extraClass.Length > 0) sb.Append(' ').Append(extraClass);
        sb.Append("\" href=\"").Append(H(href)).Append("\">").Append(H(text)).Append("</a>");
    }

    // ---- links --------------------------------------------------------------------------------------

    /// <summary>Only http(s), mailto, tel, in-page anchors and plain relative links are allowed. Anything else (javascript:, data:…) becomes "#".</summary>
    public static string SafeHref(string? link)
    {
        var l = (link ?? "").Trim();
        if (l.Length == 0) return "#";
        if (l.StartsWith('#')) return l;
        if (l.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            l.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            l.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
            l.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            return l;
        if (!l.Contains(':') && !l.StartsWith("//")) return l; // relative, e.g. about.html
        return "#";
    }

    /// <summary>Digits only, Arabic-Indic digits converted to ASCII ("٠١٢" → "012"); other characters are dropped.</summary>
    private static string Digits(string? text, bool keepPlus)
    {
        var sb = new StringBuilder();
        foreach (var c in text ?? "")
        {
            if (c is >= '0' and <= '9') sb.Append(c);
            else if (c is >= '\u0660' and <= '\u0669') sb.Append((char)('0' + (c - '\u0660')));
            else if (c is >= '\u06F0' and <= '\u06F9') sb.Append((char)('0' + (c - '\u06F0')));
            else if (keepPlus && c == '+' && sb.Length == 0) sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>wa.me link from an international number (country code, no "+", no zeros in front); null when the number is unusable.</summary>
    public static string? WhatsAppHref(string? number)
    {
        var d = Digits(number, false);
        return d.Length >= 8 && d.Length <= 15 ? "https://wa.me/" + d : null;
    }

    public static string? PhoneHref(string? number)
    {
        var d = Digits(number, true);
        return d.TrimStart('+').Length >= 5 ? "tel:" + d : null;
    }

    public static string? EmailHref(string? email)
    {
        var e = (email ?? "").Trim();
        var at = e.IndexOf('@');
        if (at < 1 || e.IndexOf('.', at) < at + 2 || e.EndsWith('.')) return null;
        foreach (var c in e)
            if (char.IsWhiteSpace(c) || c is '"' or '<' or '>' or ',' or ';') return null;
        return "mailto:" + e;
    }

    // ---- styles and the editor bridge --------------------------------------------------------------

    private const string BaseCss = """
        :root{--ink:#0B1033;--muted:#5A6482;--bg:#fff}
        *{box-sizing:border-box}
        body{margin:0;background:var(--bg);color:var(--ink);font-family:var(--font);line-height:1.7;font-size:16px}
        .wrap{max-width:1040px;margin:0 auto;padding:0 24px}
        a{color:var(--accent)}
        .site-header{border-bottom:1px solid var(--line);background:#fff;position:sticky;top:0;z-index:5}
        .bar{display:flex;align-items:center;justify-content:space-between;gap:16px;flex-wrap:wrap;padding-top:14px;padding-bottom:14px}
        .brand{font-weight:700;font-size:20px}
        nav{display:flex;gap:18px;flex-wrap:wrap}
        nav a{text-decoration:none;color:var(--muted);font-size:15px}
        nav a.current{color:var(--accent);font-weight:600}
        .sec{padding:64px 0}
        .sec h1{font-size:44px;line-height:1.25;margin:0 0 14px}
        .sec h2{font-size:30px;line-height:1.3;margin:0 0 18px}
        .sec h3{margin:0 0 8px;font-size:19px}
        .sec p{margin:0 0 14px;color:var(--muted);font-size:17px}
        .sec-hero{text-align:center;background:linear-gradient(180deg,var(--soft),#fff)}
        .sec-hero p{max-width:640px;margin:0 auto 22px}
        .sec-text .wrap{max-width:760px}
        .btn{display:inline-block;padding:12px 26px;border-radius:var(--radius);background:var(--accent);color:var(--accent-ink);text-decoration:none;font-weight:600;border:2px solid var(--accent)}
        .btn.light{background:var(--accent-ink);color:var(--accent);border-color:var(--accent-ink)}
        .btn.outline{background:transparent;color:var(--accent)}
        .grid{display:grid;grid-template-columns:repeat(3,1fr);gap:18px;margin-top:8px}
        .card{border:1px solid var(--line);border-radius:var(--radius);padding:22px;background:#fff}
        .card p{margin:0;font-size:15px}
        .quote{margin:0}
        .quote p{color:var(--ink);font-size:16px}
        .quote .who{color:var(--muted);font-size:14px;margin-top:10px}
        .sec-steps .steps{counter-reset:s;list-style:none;padding:0;margin:14px 0 0;display:grid;grid-template-columns:repeat(auto-fit,minmax(210px,1fr));gap:26px 18px}
        .sec-steps li{counter-increment:s;position:relative;border:1px solid var(--line);border-radius:var(--radius);padding:30px 22px 22px;background:#fff}
        .sec-steps li::before{content:counter(s);position:absolute;top:-16px;inset-inline-start:20px;width:34px;height:34px;border-radius:50%;background:var(--accent);color:var(--accent-ink);display:grid;place-items:center;font-weight:700}
        .sec-steps li p{margin:0;font-size:15px}
        .sec-stats{background:var(--soft)}
        .stats{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:18px;text-align:center}
        .stat b{display:block;font-size:40px;line-height:1.2;color:var(--accent)}
        .stat span{color:var(--muted)}
        .sec-faq .wrap{max-width:820px}
        .qa{border:1px solid var(--line);border-radius:var(--radius);padding:14px 18px;margin-bottom:10px;background:#fff}
        .qa summary{cursor:pointer;font-weight:600}
        .qa p{margin:10px 0 0;font-size:16px}
        .sec-cta{background:var(--accent);text-align:center}
        .sec-cta h2{color:var(--accent-ink)}
        .sec-cta p{color:var(--accent-ink);opacity:.92;max-width:620px;margin:0 auto 22px}
        .sec-contact{background:var(--soft);text-align:center}
        .sec-contact p{max-width:620px;margin:0 auto 18px}
        .contact-btns{display:flex;gap:12px;justify-content:center;flex-wrap:wrap}
        .sec-footer{background:var(--dark);padding:28px 0;text-align:center}
        .sec-footer p{margin:0;color:#cfd6f5;font-size:14px}
        @media (max-width:820px){.grid{grid-template-columns:1fr}.sec{padding:44px 0}.sec h1{font-size:32px}.sec h2{font-size:25px}}
        """;

    private const string EditCss = """
        .webo-sec{cursor:pointer}
        .webo-sec:hover{outline:2px dashed rgba(32,144,240,.65);outline-offset:-2px}
        .webo-sec.selected{outline:3px solid #2090F0;outline-offset:-3px}
        .webo-empty{margin:48px auto;max-width:520px;padding:28px;text-align:center;border:2px dashed #b9d4f5;border-radius:14px;color:#5A6482}
        """;

    // Messages to the app are plain strings: "select:<sectionId>", "select:" (nothing), "page:<pageId>".
    private const string EditScript = """
        (function () {
          function post(m) { try { window.chrome.webview.postMessage(m); } catch (e) {} }
          document.addEventListener('click', function (e) {
            var a = e.target.closest ? e.target.closest('a') : null;
            if (a) e.preventDefault();
            var pg = e.target.closest ? e.target.closest('[data-page-id]') : null;
            if (pg) { post('page:' + pg.getAttribute('data-page-id')); return; }
            var s = e.target.closest ? e.target.closest('[data-webo-id]') : null;
            post('select:' + (s ? s.getAttribute('data-webo-id') : ''));
          }, true);
          window.weboSetTheme = function (css) {
            var el = document.getElementById('webo-theme');
            if (el) el.textContent = css;
          };
          window.weboUpdate = function (html, reset, scrollToId) {
            var y = window.scrollY;
            document.getElementById('webo-root').innerHTML = html;
            if (reset) { window.scrollTo(0, 0); }
            else { window.scrollTo(0, y); }
            if (scrollToId && !reset) {
              var el = document.querySelector('[data-webo-id="' + scrollToId + '"]');
              if (el && el.scrollIntoView) el.scrollIntoView({ block: 'nearest' });
            }
          };
        })();
        """;
}
