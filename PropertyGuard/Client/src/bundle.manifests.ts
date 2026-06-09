import { manifests as globalContext } from './global-context/manifest.js';
import { manifests as conditions } from './conditions/manifest.js';
import { manifests as workspaceContexts } from './workspace-contexts/manifest.js';
import { manifests as workspaces } from './workspaces/manifest.js';
import { manifests as workspaceFooterApp } from './workspace-footer-app/manifest.js';
import { manifests as sections } from './sections/manifest.js';

// Job of the bundle is to collate all the manifests from different parts of the extension and load other manifests
// We load this bundle from umbraco-package.json
export const manifests: Array<UmbExtensionManifest> = [
  ...globalContext,
  ...conditions,
  ...workspaceContexts,
  ...workspaces,
  ...workspaceFooterApp,
  ...sections,
];
