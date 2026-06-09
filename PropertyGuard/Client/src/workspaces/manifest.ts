import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '@umbraco-cms/backoffice/document';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { PROPERTYGUARD_WORKSPACE_HAS_GUARDS_CONDITION_ALIAS } from '../conditions/propertyguard-workspace-has-guards.condition';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'workspaceView',
    name: 'Property Guard Workspace View',
    alias: `PropertyGuard.WorkspaceView`,
    element: () => import('./propertyguard-workspace-view.element'),
    meta: {
      label: 'Guards',
      pathname: 'guards',
      icon: 'icon-shield',
    },
    conditions: [
      {
        alias: UMB_WORKSPACE_CONDITION_ALIAS,
        match: UMB_DOCUMENT_WORKSPACE_ALIAS,
      },
      {
        alias: PROPERTYGUARD_WORKSPACE_HAS_GUARDS_CONDITION_ALIAS,
      },
    ],
  },
];
