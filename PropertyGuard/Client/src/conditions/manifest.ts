import PropertyGuardWorkspaceHasGuardsCondition, {
  PROPERTYGUARD_WORKSPACE_HAS_GUARDS_CONDITION_ALIAS,
} from './propertyguard-workspace-has-guards.condition';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'condition',
    name: 'Property Guard Workspace Has Guards',
    alias: PROPERTYGUARD_WORKSPACE_HAS_GUARDS_CONDITION_ALIAS,
    api: PropertyGuardWorkspaceHasGuardsCondition,
  },
];
