import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '@umbraco-cms/backoffice/document';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'workspaceFooterApp',
    name: 'Property Guard Workspace Footer App',
    alias: 'PropertyGuard.WorkspaceFooterApp',
    element: () => import('./propertyguard-workspace-footer-app.element'),
    conditions: [
      {
        alias: UMB_WORKSPACE_CONDITION_ALIAS,
        match: UMB_DOCUMENT_WORKSPACE_ALIAS,
      },
    ],
  },
];
