export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'backofficeEntryPoint',
    alias: 'PropertyGuard.Entrypoint',
    name: 'Property Guard Entrypoint',
    js: () => import('./entrypoint'),
  },
];
