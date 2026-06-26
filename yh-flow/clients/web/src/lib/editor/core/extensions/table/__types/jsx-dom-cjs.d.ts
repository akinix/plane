// FLOW: Type declaration for jsx-dom-cjs (used by table views)
declare module "jsx-dom-cjs" {
  function h<T extends keyof HTMLElementTagNameMap>(tag: T): HTMLElementTagNameMap[T];
  function h<T extends keyof HTMLElementTagNameMap>(tag: T, attrs: Record<string, any> | null): HTMLElementTagNameMap[T];
  function h<T extends keyof HTMLElementTagNameMap>(tag: T, attrs: Record<string, any> | null, ...children: any[]): HTMLElementTagNameMap[T];
  export { h };
}
