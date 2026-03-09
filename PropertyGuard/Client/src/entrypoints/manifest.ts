export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Property Guard Entrypoint",
    alias: "PropertyGuard.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.js"),
  },
];
