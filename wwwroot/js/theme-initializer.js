(() => {
  "use strict";

  const storageKey = "jobApplicationTracker.theme";
  const validPreferences = new Set(["light", "dark", "system"]);
  const root = document.documentElement;
  const selectionEnabled =
    root.dataset.themeSelectionEnabled === "true";

  const normalizePreference = value =>
    validPreferences.has(value) ? value : null;

  const configuredDefault =
    normalizePreference(root.dataset.themeDefault) ?? "system";

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

  if (!selectionEnabled) {
    return;
  }

  const initializePreferenceControl = () => {
    const selector = document.getElementById("theme-preference");

    if (selector === null) {
      return;
    }

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
  };

  if (document.readyState === "loading") {
    document.addEventListener(
      "DOMContentLoaded",
      initializePreferenceControl,
      { once: true }
    );
  } else {
    initializePreferenceControl();
  }
})();
