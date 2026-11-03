import { createContext, useContext, useEffect, useState, type ReactNode } from "react";

export type Lang = "ar" | "en";
const KEY = "webo-lang";

const ar = {
  skip: "تخطَّ إلى المحتوى",
  nav: { features: "المزايا", how: "كيف يعمل", faq: "الأسئلة", download: "تحميل", menu: "القائمة", close: "إغلاق القائمة", main: "التنقل الرئيسي", toggle: "Switch to English", toggleShort: "EN" },
  hero: {
    badge: "إصدار تجريبي مبكر · مجاني لفترة الإطلاق",
    title1: "من أول فكرة…",
    title2: "لموقع جاهز للنشر.",
    sub: "Webo محرّر مواقع مرئي لسطح المكتب. صمّم صفحات متعددة، شاهدها على كل شاشة، وصدّر ملفات HTML و CSS نظيفة — دون كتابة سطر برمجي.",
    cta: "حمّل Webo",
    explore: "استكشف المزايا",
    note: "يعمل على Windows 10 و 11 · x64",
    art: "رسم توضيحي لمحرر Webo: لوحة تصميم وأجهزة وملفات تصدير",
  },
  strip: ["متعدد الصفحات", "معاينة متجاوبة", "أقسام مرئية", "هوية العلامة", "ملفات محلية", "تصدير HTML/CSS"],
  features: {
    kicker: "المزايا",
    title: "كل ما تحتاجه لبناء موقع حقيقي، في مساحة واحدة.",
    pages: { t: "بنية متعددة الصفحات", d: "أنشئ الرئيسية، من نحن، الخدمات والتواصل — وتنقّل بينها بنقرة. كل صفحة مستقلة ومترابطة." },
    visual: { t: "تحرير مرئي حقيقي", d: "انقر أي عنصر وعدّله مباشرة. ما تراه هو ما سيُنشر." },
    sections: { t: "أقسام جاهزة للتركيب", d: "رتّب الأقسام كما ترتّب الأفكار: واجهة، مزايا، شهادات، تواصل." },
    brand: { t: "هوية علامتك", d: "ألوان، خطوط، وزوايا موحّدة تنعكس على الموقع كاملاً." },
    local: { t: "محلي أولاً", d: "مشاريعك ملفات على جهازك. لا حساب ولا رفع إلزامي للسحابة." },
    export: { t: "تصدير نظيف", d: "HTML و CSS مقروءة، جاهزة لأي استضافة ثابتة." },
  },
  screens: {
    kicker: "مساحة واحدة، كل الشاشات",
    title: "صمّم مرة، وتأكد من كل مقاس.",
    sub: "بدّل بين سطح المكتب واللوحي والجوال فوراً، وشاهد تخطيطك يتكيّف قبل أن يراه زوارك.",
    desktop: "سطح المكتب", tablet: "لوحي", mobile: "جوال",
    group: "اختر مقاس المعاينة",
  },
  how: {
    kicker: "كيف يعمل",
    title: "ثلاث خطوات من الفكرة إلى الملفات.",
    steps: [
      { t: "ابدأ مشروعاً", d: "أنشئ مشروعاً محلياً وأضف صفحاتك الأساسية." },
      { t: "ركّب وصمّم", d: "أضف الأقسام، عدّل النصوص، وطبّق هوية علامتك." },
      { t: "صدّر وانشر", d: "عاين على كل الشاشات ثم صدّر HTML و CSS للنشر." },
    ],
  },
  rtl: {
    kicker: "عربي أولاً",
    title: "العربية ليست ترجمة لاحقة.",
    sub: "اتجاه من اليمين لليسار، محاذاة صحيحة، وخطوط تحترم جمال الحرف العربي — جنباً إلى جنب مع الإنجليزية.",
    sample: "صمّم بلغتك",
    sampleSub: "اتجاه · محاذاة · إيقاع",
    points: ["دعم كامل للاتجاه RTL و LTR", "واجهة ثنائية اللغة", "معاينة النص العربي كما سيظهر"],
  },
  exp: {
    kicker: "التصدير",
    title: "ملفاتك، بلا قيود.",
    sub: "صدّر موقعك كملفات ثابتة منظمة يمكنك رفعها إلى أي استضافة أو فتحها في أي محرر.",
    tree: "شجرة ملفات التصدير",
  },
  faq: {
    kicker: "الأسئلة الشائعة",
    title: "إجابات واضحة قبل التحميل.",
    items: [
      { q: "هل Webo مجاني؟", a: "Webo مجاني حالياً خلال فترة إطلاق محدودة للإصدار التجريبي المبكر. قد تتغير الخطة لاحقاً، وسنعلن ذلك بوضوح." },
      { q: "ما أنظمة التشغيل المدعومة؟", a: "Windows 10 و Windows 11 بمعمارية x64. لا تتوفر نسخ لأنظمة أخرى في الوقت الحالي." },
      { q: "هل أحتاج إلى خبرة برمجية؟", a: "لا. Webo مصمم للتحرير المرئي، ومناسب للمبتدئين وأصحاب الأعمال والمصممين." },
      { q: "أين تُحفظ مشاريعي؟", a: "على جهازك كملفات محلية. أنت من يقرر متى وأين تنشرها." },
      { q: "ماذا يعني «إصدار تجريبي مبكر»؟", a: "أن المنتج قيد التطوير النشط. قد تواجه بعض القيود أو الأخطاء، وملاحظاتك تساعدنا على تحسينه." },
      { q: "كيف أنشر موقعي؟", a: "صدّر ملفات HTML و CSS وارفعها إلى أي استضافة ثابتة تختارها." },
    ],
  },
  cta: {
    title: "موقعك القادم يبدأ من سطح مكتبك.",
    sub: "حمّل الإصدار التجريبي المبكر مجاناً خلال فترة الإطلاق.",
    btn: "حمّل لـ Windows",
    soon: "رابط التحميل سيتوفر قريباً.",
    drive: "يفتح صفحة الملف على Google Drive · Windows 10/11 · x64",
  },
  footer: { tag: "محرّر مواقع مرئي لسطح المكتب.", rights: "جميع الحقوق محفوظة.", preview: "إصدار تجريبي مبكر" },
};

type Dict = typeof ar;

const en: Dict = {
  skip: "Skip to content",
  nav: { features: "Features", how: "How it works", faq: "FAQ", download: "Download", menu: "Menu", close: "Close menu", main: "Main navigation", toggle: "التبديل إلى العربية", toggleShort: "ع" },
  hero: {
    badge: "Early Preview · Free during launch",
    title1: "From first idea",
    title2: "to a website ready to go.",
    sub: "Webo is a visual website builder for your desktop. Design multi-page sites, preview them on every screen, and export clean HTML & CSS — no code required.",
    cta: "Download Webo",
    explore: "Explore features",
    note: "Runs on Windows 10 & 11 · x64",
    art: "Illustration of the Webo editor: a design canvas, devices and export files",
  },
  strip: ["Multi-page", "Responsive preview", "Visual sections", "Brand identity", "Local files", "HTML/CSS export"],
  features: {
    kicker: "Features",
    title: "Everything a real website needs, in one workspace.",
    pages: { t: "Multi-page architecture", d: "Build Home, About, Services and Contact — and jump between them in one click. Every page independent, all connected." },
    visual: { t: "Real visual editing", d: "Click any element and change it in place. What you see is what ships." },
    sections: { t: "Composable sections", d: "Arrange sections like ideas: hero, features, testimonials, contact." },
    brand: { t: "Your brand identity", d: "Colors, fonts and corner radius, applied consistently across the site." },
    local: { t: "Local-first", d: "Projects are files on your machine. No account, no forced cloud upload." },
    export: { t: "Clean export", d: "Readable HTML & CSS, ready for any static host." },
  },
  screens: {
    kicker: "One workspace, every screen",
    title: "Design once. Check every size.",
    sub: "Switch instantly between desktop, tablet and mobile and watch your layout adapt before visitors ever see it.",
    desktop: "Desktop", tablet: "Tablet", mobile: "Mobile",
    group: "Choose preview size",
  },
  how: {
    kicker: "How it works",
    title: "Three steps from idea to files.",
    steps: [
      { t: "Start a project", d: "Create a local project and add your core pages." },
      { t: "Compose & style", d: "Add sections, edit copy and apply your brand." },
      { t: "Export & publish", d: "Preview on every screen, then export HTML & CSS." },
    ],
  },
  rtl: {
    kicker: "Arabic-first",
    title: "Arabic is not an afterthought.",
    sub: "Right-to-left direction, correct alignment and type that respects Arabic letterforms — side by side with English.",
    sample: "صمّم بلغتك",
    sampleSub: "Direction · Alignment · Rhythm",
    points: ["Full RTL and LTR support", "Bilingual interface", "Preview Arabic text as it will ship"],
  },
  exp: {
    kicker: "Export",
    title: "Your files, no lock-in.",
    sub: "Export your site as organised static files you can upload to any host or open in any editor.",
    tree: "Export file tree",
  },
  faq: {
    kicker: "FAQ",
    title: "Clear answers before you download.",
    items: [
      { q: "Is Webo free?", a: "Webo is currently free during a limited launch period for the Early Preview. Plans may change later, and we will announce that clearly." },
      { q: "Which operating systems are supported?", a: "Windows 10 and Windows 11 on x64. Other platforms are not available at this time." },
      { q: "Do I need coding experience?", a: "No. Webo is built for visual editing and suits beginners, business owners and designers alike." },
      { q: "Where are my projects saved?", a: "On your computer as local files. You decide when and where to publish." },
      { q: "What does “Early Preview” mean?", a: "The product is in active development. You may hit some limits or bugs, and your feedback helps us improve it." },
      { q: "How do I publish my site?", a: "Export the HTML & CSS files and upload them to any static host you choose." },
    ],
  },
  cta: {
    title: "Your next website starts on your desktop.",
    sub: "Download the Early Preview, free during the launch period.",
    btn: "Download for Windows",
    soon: "Download link coming soon.",
    drive: "Opens the file page on Google Drive · Windows 10/11 · x64",
  },
  footer: { tag: "A visual website builder for the desktop.", rights: "All rights reserved.", preview: "Early Preview" },
};

export const dictionaries = { ar, en };

type Ctx = { lang: Lang; t: Dict; dir: "rtl" | "ltr"; setLang: (l: Lang) => void; toggle: () => void };
const LangContext = createContext<Ctx | null>(null);

export function LangProvider({ children }: { children: ReactNode }) {
  const [lang, setLangState] = useState<Lang>("ar");

  useEffect(() => {
    const saved = localStorage.getItem(KEY);
    if (saved === "en" || saved === "ar") setLangState(saved);
  }, []);

  useEffect(() => {
    document.documentElement.lang = lang;
    document.documentElement.dir = lang === "ar" ? "rtl" : "ltr";
  }, [lang]);

  const setLang = (l: Lang) => {
    setLangState(l);
    localStorage.setItem(KEY, l);
  };

  return (
    <LangContext.Provider
      value={{ lang, t: dictionaries[lang], dir: lang === "ar" ? "rtl" : "ltr", setLang, toggle: () => setLang(lang === "ar" ? "en" : "ar") }}
    >
      {children}
    </LangContext.Provider>
  );
}

export function useLang() {
  const c = useContext(LangContext);
  if (!c) throw new Error("useLang must be inside LangProvider");
  return c;
}