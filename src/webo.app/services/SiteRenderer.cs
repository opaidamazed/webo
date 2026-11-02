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
}

/// <summary>
/// Turns the project model into HTML. The model is the single source of truth: the editor preview and the
/// future export both come from here, so what you see in the preview is what gets exported.
/// </summary>
public static class SiteRenderer
{
    public static string PageFileName(PageDoc p) => p.IsHome ? "index.html" : p.Slug + ".html";

    private static string H(string? s) => WebUtility.HtmlEncode(s ?? "");

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
            // The preview never needs the network: block everything except its own inline style/script and data: images.
            sb.Append("<meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src 'unsafe-inline'; script-src 'unsafe-inline'; img-src data:\">\n");
        }
        sb.Append("<title>").Append(H(page.Title)).Append(" — ").Append(H(doc.Name)).Append("</title>\n");
        sb.Append("<style>").Append(Css);
        if (opt.EditMode) sb.Append(EditCss);
        sb.Append("</style>\n</head>\n<body>\n<div id=\"webo-root\">");
        sb.Append(RenderBody(doc, pages, page, opt));
        sb.Append("</div>\n");
        if (opt.EditMode) sb.Append("<script>").Append(EditScript).Append("</script>\n");
        sb.Append("</body>\n</html>\n");
        return sb.ToString();
    }

    /// <summary>Header + the page's sections. The editor swaps this fragment in place so scrolling is not lost.</summary>
    public static string RenderBody(ProjectDoc doc, IReadOnlyList<PageDoc> pages, PageDoc page, RenderOptions opt)
    {
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

        foreach (var s in page.Sections) AppendSection(sb, s, opt);

        sb.Append("</main>\n");
        return sb.ToString();
    }

    private static void AppendSection(StringBuilder sb, SectionDoc s, RenderOptions opt)
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
                if (s.Get("heading").Length > 0) sb.Append("<h2>").Append(H(s.Get("heading"))).Append("</h2>");
                AppendParagraphs(sb, s.Get("body"));
                sb.Append("</div>");
                break;

            case "features":
                sb.Append("<div class=\"wrap\">");
                if (s.Get("heading").Length > 0) sb.Append("<h2>").Append(H(s.Get("heading"))).Append("</h2>");
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

            case "cta":
                sb.Append("<div class=\"wrap\">");
                if (s.Get("heading").Length > 0) sb.Append("<h2>").Append(H(s.Get("heading"))).Append("</h2>");
                AppendParagraphs(sb, s.Get("text"));
                AppendButton(sb, s.Get("btn"), s.Get("link"), opt, "light");
                sb.Append("</div>");
                break;

            case "footer":
                sb.Append("<div class=\"wrap\">");
                AppendParagraphs(sb, s.Get("text"));
                sb.Append("</div>");
                break;
        }

        sb.Append("</").Append(tag).Append(">\n");
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
        var href = opt.EditMode ? "#" : SafeHref(link);
        sb.Append("<a class=\"btn");
        if (extraClass.Length > 0) sb.Append(' ').Append(extraClass);
        sb.Append("\" href=\"").Append(H(href)).Append("\">").Append(H(text)).Append("</a>");
    }

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

    // ---- styles and the editor bridge --------------------------------------------------------------

    private const string Css = """
        :root{--accent:#2090F0;--accent-ink:#fff;--ink:#0B1033;--muted:#5A6482;--bg:#fff;--soft:#F1F6FD;--line:#D6E6FA;--dark:#0B1033}
        *{box-sizing:border-box}
        body{margin:0;background:var(--bg);color:var(--ink);font-family:"IBM Plex Sans Arabic","Segoe UI",Tahoma,Arial,sans-serif;line-height:1.7;font-size:16px}
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
        .sec p{margin:0 0 14px;color:var(--muted);font-size:17px}
        .sec-hero{text-align:center;background:linear-gradient(180deg,var(--soft),#fff)}
        .sec-hero p{max-width:640px;margin:0 auto 22px}
        .sec-text .wrap{max-width:760px}
        .btn{display:inline-block;padding:12px 26px;border-radius:10px;background:var(--accent);color:var(--accent-ink);text-decoration:none;font-weight:600}
        .btn.light{background:#fff;color:var(--accent)}
        .grid{display:grid;grid-template-columns:repeat(3,1fr);gap:18px;margin-top:8px}
        .card{border:1px solid var(--line);border-radius:14px;padding:22px;background:#fff}
        .card h3{margin:0 0 8px;font-size:19px}
        .card p{margin:0;font-size:15px}
        .sec-cta{background:var(--accent);text-align:center}
        .sec-cta h2{color:#fff}
        .sec-cta p{color:#fff;opacity:.92;max-width:620px;margin:0 auto 22px}
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
