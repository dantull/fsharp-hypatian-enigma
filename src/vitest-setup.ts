import '@testing-library/jest-dom/vitest';

if (typeof process !== 'undefined') {
  process.env.NODE_ENV = 'test'
}

const localStorageMock = (() => {
  let store: Record<string, string> = {}
  return {
    getItem(key: string) {
      return Object.prototype.hasOwnProperty.call(store, key) ? store[key] : null
    },
    setItem(key: string, value: string) {
      store[key] = String(value)
    },
    removeItem(key: string) {
      delete store[key]
    },
    clear() {
      store = {}
    },
  }
})()

if (typeof globalThis.localStorage === 'undefined') {
  Object.defineProperty(globalThis, 'localStorage', {
    value: localStorageMock,
    writable: true,
  })
}

if (typeof (globalThis as any).window !== 'undefined' && typeof (globalThis as any).window.localStorage === 'undefined') {
  Object.defineProperty((globalThis as any).window, 'localStorage', {
    value: localStorageMock,
    writable: true,
  })
}
