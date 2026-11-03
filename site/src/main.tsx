import React from "react";
import ReactDOM from "react-dom/client";
import { LangProvider } from "@/lib/i18n";
import {
  Header,
  Hero,
  Features,
  Screens,
  How,
  ArabicFirst,
  Export,
  Faq,
  FinalCta,
  Footer,
} from "@/components/webo/Sections";
import "./styles.css";

function App() {
  return (
    <LangProvider>
      <Header />
      <main id="main">
        <Hero />
        <Features />
        <Screens />
        <How />
        <ArabicFirst />
        <Export />
        <Faq />
        <FinalCta />
      </main>
      <Footer />
    </LangProvider>
  );
}

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
);
