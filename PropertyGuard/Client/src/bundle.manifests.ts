import { manifests as entrypoints } from './entrypoints/manifest.js';
import { manifests as conditions } from './conditions/manifest.js';
import { manifests as workspaceContexts } from './workspaceContexts/manifest.js';
import { manifests as workspaces } from './workspaces/manifest.js';
import { manifests as workspaceFooterApp } from './workspaceFooterApp/manifest.js';

// Job of the bundle is to collate all the manifests from different parts of the extension and load other manifests
// We load this bundle from umbraco-package.json
export const manifests: Array<UmbExtensionManifest> = [
  ...entrypoints,
  ...conditions,
  ...workspaceContexts,
  ...workspaces,
  ...workspaceFooterApp,
];
