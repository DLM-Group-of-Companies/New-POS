(() => {
  "use strict";

  const searchInput = document.getElementById("guide-search");
  const results = document.getElementById("search-results");
  const chapterPrefix = document.body.dataset.chapterPrefix || "chapters/";

  if (searchInput && results && Array.isArray(window.POSGuideSearchIndex)) {
    searchInput.addEventListener("input", () => {
      const query = searchInput.value.trim().toLocaleLowerCase();
      results.replaceChildren();
      if (!query) return;

      const matches = window.POSGuideSearchIndex.filter(chapter =>
        `${chapter.title} ${chapter.text}`.toLocaleLowerCase().includes(query)
      ).slice(0, 8);

      if (!matches.length) {
        const empty = document.createElement("p");
        empty.className = "search-empty";
        empty.textContent = "No matching chapters.";
        results.append(empty);
        return;
      }

      const list = document.createElement("ul");
      matches.forEach(chapter => {
        const item = document.createElement("li");
        const link = document.createElement("a");
        link.href = `${chapterPrefix}${chapter.filename}`;
        link.textContent = chapter.title;
        item.append(link);
        list.append(item);
      });
      results.append(list);
    });
  }

  const menuButton = document.querySelector(".menu-toggle");
  const sidebar = document.getElementById("sidebar");
  if (menuButton && sidebar) {
    menuButton.addEventListener("click", () => {
      const expanded = menuButton.getAttribute("aria-expanded") === "true";
      menuButton.setAttribute("aria-expanded", String(!expanded));
      sidebar.classList.toggle("open", !expanded);
    });
  }

  const viewer = document.querySelector(".image-viewer");
  if (viewer) {
    const largeImage = viewer.querySelector("img");
    const closeButton = viewer.querySelector(".image-viewer-close");
    document.querySelectorAll(".chapter img").forEach(image => {
      image.addEventListener("click", () => {
        largeImage.src = image.src;
        largeImage.alt = image.alt;
        viewer.hidden = false;
        closeButton.focus();
      });
    });

    const close = () => {
      viewer.hidden = true;
      largeImage.removeAttribute("src");
    };
    closeButton.addEventListener("click", close);
    viewer.addEventListener("click", event => {
      if (event.target === viewer) close();
    });
    document.addEventListener("keydown", event => {
      if (event.key === "Escape" && !viewer.hidden) close();
    });
  }
})();
