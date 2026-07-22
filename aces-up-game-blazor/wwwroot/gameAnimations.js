export function getBoundingRectById(elementId) {
  const element = document.getElementById(elementId);
  if (!element) {
    return null;
  }

  const rect = element.getBoundingClientRect();
  return {
    left: rect.left,
    top: rect.top,
    width: rect.width,
    height: rect.height,
  };
}

export function readJson(storageKey) {
  try {
    const rawValue = window.localStorage.getItem(storageKey);
    if (!rawValue) {
      return null;
    }

    return JSON.parse(rawValue);
  } catch {
    return null;
  }
}

export function writeJson(storageKey, value) {
  window.localStorage.setItem(storageKey, JSON.stringify(value));
}
