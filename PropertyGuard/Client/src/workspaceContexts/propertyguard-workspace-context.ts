import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
  UMB_DOCUMENT_WORKSPACE_CONTEXT,
  UmbDocumentWorkspaceContext,
} from '@umbraco-cms/backoffice/document';
import {
  UMB_NOTIFICATION_CONTEXT,
  UmbNotificationContext,
} from '@umbraco-cms/backoffice/notification';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbPropertyGuardRule } from '@umbraco-cms/backoffice/property';
import { PropertyGuardDto, PropertyGuardService } from '../api';
import { ProblemDetails } from '@umbraco-cms/backoffice/external/backend-api';

export class PropertyGuardWorkspaceContext extends UmbContextBase {
  #documentWorkspaceContext?: UmbDocumentWorkspaceContext;
  #notificationContext?: UmbNotificationContext;

  #propertyGuards = new UmbArrayState<PropertyGuardDto>([], (guard) => guard.propertyAlias);
  propertyGuards = this.#propertyGuards.asObservable();

  #hasPropertyGuards = new UmbBooleanState(false);
  hasPropertyGuards = this.#hasPropertyGuards.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, PROPERTYGUARD_WORKSPACE_CONTEXT);
    console.log('Property Guard enabled! 🛡️');

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
      this.#notificationContext = notificationContext;
    });

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (documentWorkspaceContext) => {
      this.#documentWorkspaceContext = documentWorkspaceContext;

      if (!this.#documentWorkspaceContext) return;

      const hasLoaded = this.#documentWorkspaceContext.structure.whenLoaded();
      hasLoaded.then(() => this.#observeContentType());
    });
  }

  async #observeContentType() {
    if (!this.#documentWorkspaceContext) return;

    this.#documentWorkspaceContext.observe(
      this.#documentWorkspaceContext.structure.contentTypeAliases,
      async (contentTypeAliases) => {
        if (contentTypeAliases.length === 0) return;
        const contentTypeAlias = contentTypeAliases[0];
        if (contentTypeAlias) {
          await this.#applyPropertyGuards(contentTypeAlias);
        }
      },
    );
  }

  async #getPropertyGuards(contentTypeAlias: string) {
    const { data, error } = await PropertyGuardService.getPropertyGuards({
      query: { contentTypeAlias: contentTypeAlias },
    });

    if (error) {
      const err = error as ProblemDetails;
      this.#notificationContext?.peek('danger', {
        data: {
          headline: err.title!,
          message: err.detail!,
        },
      });

      console.error(err);
      return;
    }

    if (data) {
      this.#propertyGuards.setValue(data);
    }
  }

  async #applyPropertyGuards(contentTypeAlias: string) {
    this.#propertyGuards.clear();
    this.#hasPropertyGuards.setValue(false);

    await this.#getPropertyGuards(contentTypeAlias);

    const propertyGuards = this.#propertyGuards.getValue();
    if (propertyGuards.length === 0) return;

    const propertyGuardRules: UmbPropertyGuardRule[] = [];
    for (const propertyGuard of propertyGuards) {
      const property = await this.#documentWorkspaceContext?.structure.getPropertyStructureByAlias(
        propertyGuard.propertyAlias,
      );

      if (!property) continue;

      const propertyGuardRule: UmbPropertyGuardRule = {
        unique: `propertyguard-${contentTypeAlias}-${propertyGuard.propertyAlias}`,
        permitted: false,
        message: propertyGuard.message,
        propertyType: { unique: property.unique },
      };

      propertyGuardRules.push(propertyGuardRule);
    }

    if (propertyGuardRules.length > 0) {
      this.#documentWorkspaceContext?.propertyWriteGuard.addRules(propertyGuardRules);
      this.#hasPropertyGuards.setValue(true);
    }
  }
}

export const api = PropertyGuardWorkspaceContext;

export const PROPERTYGUARD_WORKSPACE_CONTEXT = new UmbContextToken<PropertyGuardWorkspaceContext>(
  'UmbWorkspaceContext',
  'propertyguard.workspacecontext',
);
