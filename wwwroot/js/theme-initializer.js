(() => {
  "use strict";

  const storageKey = "jobApplicationTracker.theme";
  const validPreferences = new Set(["light", "dark", "system"]);
  const paletteStorageKey = "jobApplicationTracker.palette";
  const validPalettes = new Set([
    "ocean",
    "fall",
    "coffee",
    "sakura",
    "forest"
  ]);
  const root = document.documentElement;
  const selectionEnabled =
    root.dataset.themeSelectionEnabled === "true";
  const paletteSelectionEnabled =
    root.dataset.paletteSelectionEnabled === "true";

  const normalizePreference = value =>
    validPreferences.has(value) ? value : null;

  const configuredDefault =
    normalizePreference(root.dataset.themeDefault) ?? "system";

  const normalizePalette = value =>
    validPalettes.has(value) ? value : null;

  const configuredPalette =
    normalizePalette(root.dataset.paletteDefault) ?? "ocean";

  let palette = configuredPalette;

  if (paletteSelectionEnabled) {
    try {
      const storedPalette = window.localStorage.getItem(
        paletteStorageKey
      );
      const normalizedStoredPalette = normalizePalette(storedPalette);

      if (normalizedStoredPalette !== null) {
        palette = normalizedStoredPalette;
      }
    } catch {
      // Storage can be unavailable in restricted browser contexts.
    }
  }

  const applyPalette = selectedPalette => {
    palette = selectedPalette;
    root.setAttribute("data-theme-palette", selectedPalette);
  };

  applyPalette(palette);

  let colorSchemeQuery = null;

  if (typeof window.matchMedia === "function") {
    try {
      colorSchemeQuery = window.matchMedia(
        "(prefers-color-scheme: dark)"
      );
    } catch {
      colorSchemeQuery = null;
    }
  }

  let preference = configuredDefault;

  if (selectionEnabled) {
    try {
      const storedPreference = window.localStorage.getItem(storageKey);
      const normalizedStoredPreference =
        normalizePreference(storedPreference);

      if (normalizedStoredPreference !== null) {
        preference = normalizedStoredPreference;
      }
    } catch {
      // Storage can be unavailable in restricted browser contexts.
    }
  }

  const resolveTheme = selectedPreference => {
    if (selectedPreference === "dark") {
      return "dark";
    }

    if (selectedPreference === "system" && colorSchemeQuery !== null) {
      return colorSchemeQuery.matches ? "dark" : "light";
    }

    return "light";
  };

  const applyPreference = selectedPreference => {
    preference = selectedPreference;
    root.setAttribute(
      "data-bs-theme",
      resolveTheme(selectedPreference)
    );
  };

  applyPreference(preference);

  if (colorSchemeQuery !== null) {
    const handleColorSchemeChange = () => {
      if (preference === "system") {
        applyPreference(preference);
      }
    };

    if (typeof colorSchemeQuery.addEventListener === "function") {
      colorSchemeQuery.addEventListener(
        "change",
        handleColorSchemeChange
      );
    } else if (typeof colorSchemeQuery.addListener === "function") {
      colorSchemeQuery.addListener(handleColorSchemeChange);
    }
  }

  const initializePreferenceControls = () => {
    if (selectionEnabled) {
      const selector = document.getElementById("theme-preference");

      if (selector !== null) {
        selector.value = preference;

        selector.addEventListener("change", () => {
          const selectedPreference = normalizePreference(selector.value);

          if (selectedPreference === null) {
            selector.value = preference;
            return;
          }

          applyPreference(selectedPreference);

          try {
            window.localStorage.setItem(storageKey, selectedPreference);
          } catch {
            // The selected preference still applies to the current page.
          }
        });

        window.addEventListener("storage", event => {
          if (event.key !== storageKey) {
            return;
          }

          const synchronizedPreference = event.newValue === null
            ? configuredDefault
            : normalizePreference(event.newValue);

          if (synchronizedPreference === null) {
            return;
          }

          selector.value = synchronizedPreference;
          applyPreference(synchronizedPreference);
        });
      }
    }

    if (paletteSelectionEnabled) {
      const paletteSelector = document.getElementById(
        "palette-preference"
      );

      if (paletteSelector !== null) {
        paletteSelector.value = palette;

        paletteSelector.addEventListener("change", () => {
          const selectedPalette = normalizePalette(
            paletteSelector.value
          );

          if (selectedPalette === null) {
            paletteSelector.value = palette;
            return;
          }

          applyPalette(selectedPalette);

          try {
            window.localStorage.setItem(
              paletteStorageKey,
              selectedPalette
            );
          } catch {
            // The selected palette still applies to the current page.
          }
        });

        window.addEventListener("storage", event => {
          if (event.key !== paletteStorageKey) {
            return;
          }

          const synchronizedPalette = event.newValue === null
            ? configuredPalette
            : normalizePalette(event.newValue);

          if (synchronizedPalette === null) {
            return;
          }

          paletteSelector.value = synchronizedPalette;
          applyPalette(synchronizedPalette);
        });
      }
    }
  };

  if (document.readyState === "loading") {
    document.addEventListener(
      "DOMContentLoaded",
      initializePreferenceControls,
      { once: true }
    );
  } else {
    initializePreferenceControls();
  }
})();
