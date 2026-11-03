(() => {
  const root = document.documentElement;
  const languageToggle = document.getElementById("languageToggle");
  const nav = document.getElementById("mainNav");
  const menuToggle = document.getElementById("menuToggle");
  const config = window.WEBO_SITE_CONFIG || {};
  const downloadUrl = config.DOWNLOAD_URL || "https://github.com/opaidamazed/webo/releases/latest";

  document.querySelectorAll("[data-download]").forEach((link) => {
    link.href = downloadUrl;
    link.target = "_blank";
    link.rel = "noopener noreferrer";
  });

  const applyLanguage = (language) => {
    const selected = language === "en" ? "en" : "ar";
    const isEnglish = selected === "en";
    root.lang = selected;
    root.dir = isEnglish ? "ltr" : "rtl";
    document.body.classList.toggle("lang-en", isEnglish);
    document.querySelectorAll("[data-ar][data-en]").forEach((element) => {
      const value = element.dataset[selected];
      if (value !== undefined) element.innerHTML = value;
    });
    languageToggle.textContent = isEnglish ? "العربية" : "English";
    languageToggle.setAttribute("aria-label", isEnglish ? "Switch to Arabic" : "Switch to English");
    document.title = isEnglish ? "Webo — Your idea, a real website" : "Webo — موقعك يبدأ من فكرة";
    const description = document.querySelector('meta[name="description"]');
    if (description) description.content = isEnglish
      ? "Webo — Build multi-page websites visually, without writing code. An Arabic-first website builder for Windows."
      : "Webo — أنشئ مواقعك متعددة الصفحات بصريًا، من غير ما تكتب كود. محرر مواقع لسطح مكتب Windows مع دعم عربي أصيل.";
    try { localStorage.setItem("webo-language", selected); } catch (_) {}
  };

  let savedLanguage = "ar";
  try { savedLanguage = localStorage.getItem("webo-language") || "ar"; } catch (_) {}
  applyLanguage(savedLanguage);

  languageToggle.addEventListener("click", () => {
    applyLanguage(root.lang === "ar" ? "en" : "ar");
  });

  menuToggle.addEventListener("click", () => {
    const open = nav.classList.toggle("open");
    menuToggle.setAttribute("aria-expanded", String(open));
  });
  nav.querySelectorAll("a").forEach((link) => link.addEventListener("click", () => {
    nav.classList.remove("open");
    menuToggle.setAttribute("aria-expanded", "false");
  }));

  // Subtle pointer spotlight and restrained 3D tilt for desktop pointers.
  const featureCards = document.querySelectorAll(".feature-card");
  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const finePointer = window.matchMedia("(hover: hover) and (pointer: fine)").matches;

  if (finePointer && !reducedMotion) {
    featureCards.forEach((card) => {
      card.addEventListener("pointermove", (event) => {
        const rect = card.getBoundingClientRect();
        const localX = event.clientX - rect.left;
        const localY = event.clientY - rect.top;
        const normalizedX = (localX / Math.max(rect.width, 1) - 0.5) * 2;
        const normalizedY = (localY / Math.max(rect.height, 1) - 0.5) * 2;

        card.style.setProperty("--spot-x", localX + "px");
        card.style.setProperty("--spot-y", localY + "px");
        card.style.setProperty("--tilt-x", (normalizedX * 1.15) + "deg");
        card.style.setProperty("--tilt-y", (normalizedY * -1.1) + "deg");
      }, { passive: true });

      card.addEventListener("pointerleave", () => {
        card.style.setProperty("--tilt-x", "0deg");
        card.style.setProperty("--tilt-y", "0deg");
        card.style.setProperty("--spot-x", "50%");
        card.style.setProperty("--spot-y", "50%");
      }, { passive: true });
    });
  }

  document.getElementById("year").textContent = new Date().getFullYear();

  const revealItems = document.querySelectorAll(".reveal");
  if ("IntersectionObserver" in window && !window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
    const observer = new IntersectionObserver((entries, currentObserver) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add("is-visible");
          currentObserver.unobserve(entry.target);
        }
      });
    }, { threshold: 0.12 });
    revealItems.forEach((item) => observer.observe(item));
  } else {
    revealItems.forEach((item) => item.classList.add("is-visible"));
  }
})();
