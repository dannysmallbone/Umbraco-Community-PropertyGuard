export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'globalContext',
    name: 'Property Guard  Context',
    alias: 'PropertyGuard.Context',
    api: () => import('./propertyguard-context'),
  },
];
