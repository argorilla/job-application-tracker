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
      const modeRadios = Array.from(
        document.querySelectorAll(
          'input[name="appearance-mode"]'
        )
      );

      if (modeRadios.length > 0) {
        const synchronizeModeRadios = selectedPreference => {
          const matchingRadio = modeRadios.find(
            radio => radio.value === selectedPreference
          );

          if (matchingRadio !== undefined) {
            matchingRadio.checked = true;
          }
        };

        synchronizeModeRadios(preference);

        for (const radio of modeRadios) {
          radio.addEventListener("change", () => {
            if (!radio.checked) {
              return;
            }

            const selectedPreference = normalizePreference(radio.value);

            if (selectedPreference === null) {
              synchronizeModeRadios(preference);
              return;
            }

            applyPreference(selectedPreference);

            try {
              window.localStorage.setItem(
                storageKey,
                selectedPreference
              );
            } catch {
              // The selected preference still applies to the current page.
            }
          });
        }

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

          synchronizeModeRadios(synchronizedPreference);
          applyPreference(synchronizedPreference);
        });
      }
    }

    if (paletteSelectionEnabled) {
      const paletteRadios = Array.from(
        document.querySelectorAll(
          'input[name="appearance-palette"]'
        )
      );

      if (paletteRadios.length > 0) {
        const synchronizePaletteRadios = selectedPalette => {
          const matchingRadio = paletteRadios.find(
            radio => radio.value === selectedPalette
          );

          if (matchingRadio !== undefined) {
            matchingRadio.checked = true;
          }
        };

        synchronizePaletteRadios(palette);

        for (const radio of paletteRadios) {
          radio.addEventListener("change", () => {
            if (!radio.checked) {
              return;
            }

            const selectedPalette = normalizePalette(radio.value);

            if (selectedPalette === null) {
              synchronizePaletteRadios(palette);
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
        }

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

          synchronizePaletteRadios(synchronizedPalette);
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
