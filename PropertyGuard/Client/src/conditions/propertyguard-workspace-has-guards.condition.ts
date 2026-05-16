import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
  UmbConditionConfigBase,
  UmbConditionControllerArguments,
  UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { PROPERTYGUARD_WORKSPACE_CONTEXT } from '../workspace-contexts/propertyguard-workspace-context';

export default class PropertyGuardWorkspaceHasGuardsCondition
  extends UmbConditionBase<UmbConditionConfigBase>
  implements UmbExtensionCondition
{
  constructor(
    host: UmbControllerHost,
    args: UmbConditionControllerArguments<UmbConditionConfigBase>,
  ) {
    super(host, args);

    this.consumeContext(PROPERTYGUARD_WORKSPACE_CONTEXT, (propertyGuardWorkspaceContext) => {
      if (!propertyGuardWorkspaceContext) return;

      this.observe(propertyGuardWorkspaceContext.hasPropertyGuards, (hasPropertyGuards) => {
        this.permitted = hasPropertyGuards;
      });
    });
  }
}

export const PROPERTYGUARD_WORKSPACE_HAS_GUARDS_CONDITION_ALIAS =
  'PropertyGuard.Condition.WorkspaceHasGuards';
