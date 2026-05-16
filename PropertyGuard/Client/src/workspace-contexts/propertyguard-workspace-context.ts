import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { PropertyGuardDto } from '../api';
import {
  UMB_DOCUMENT_WORKSPACE_CONTEXT,
  UmbDocumentWorkspaceContext,
} from '@umbraco-cms/backoffice/document';
import {
  PROPERTYGUARD_CONTEXT,
  PropertyGuardContext,
} from '../global-context/propertyguard-context';
import { UmbPropertyGuardRule } from '@umbraco-cms/backoffice/property';

export class PropertyGuardWorkspaceContext extends UmbContextBase {
  #propertyGuardContextContext?: PropertyGuardContext;
  #documentWorkspaceContext?: UmbDocumentWorkspaceContext;

  #hasPropertyGuards = new UmbBooleanState(false);
  hasPropertyGuards = this.#hasPropertyGuards.asObservable();

  #documentPropertyGuards = new UmbArrayState<PropertyGuardDto>(
    [],
    (propertyGuard) => propertyGuard.propertyTypeName,
  );
  documentPropertyGuards = this.#documentPropertyGuards.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, PROPERTYGUARD_WORKSPACE_CONTEXT);
    console.log('Property Guards enabled! 🛡️');

    this.consumeContext(PROPERTYGUARD_CONTEXT, (propertyGuardContextContext) => {
      this.#propertyGuardContextContext = propertyGuardContextContext;
    });

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (documentWorkspaceContext) => {
      this.#documentWorkspaceContext = documentWorkspaceContext;

      if (!this.#documentWorkspaceContext) return;

      this.#documentWorkspaceContext.structure
        .whenLoaded()
        .then(() => this.#updateAndApplyDocumentPropertyGuards())
        .catch((err) => {
          console.error('Failed to load workspace structure for Property Guard:', err);
        });
    });
  }

  #updateAndApplyDocumentPropertyGuards() {
    if (!this.#documentWorkspaceContext) return;

    this.#documentWorkspaceContext.observe(
      this.#documentWorkspaceContext.structure.contentTypeAliases,
      (contentTypeAliases) => {
        if (contentTypeAliases.length > 0) {
          if (!this.#propertyGuardContextContext) return;

          const documentPropertyGuards =
            this.#propertyGuardContextContext.getPropertyGuardsForDocumentTypes(contentTypeAliases);

          this.#documentPropertyGuards.setValue(documentPropertyGuards);
          this.#hasPropertyGuards.setValue(documentPropertyGuards.length > 0);

          this.#applyDocumentPropertyGuards();
        }
      },
    );
  }

  #applyDocumentPropertyGuards() {
    if (!this.#documentWorkspaceContext) return;

    for (const propertyGuard of this.#documentPropertyGuards.getValue()) {
      if (propertyGuard.propertyTypeUnique) {
        const propertyGuardRule: UmbPropertyGuardRule = {
          unique: `propertyguard-${propertyGuard.documentTypeAlias}-${propertyGuard.propertyTypeName}`,
          permitted: false,
          message: propertyGuard.message,
          propertyType: { unique: propertyGuard.propertyTypeUnique },
        };

        this.#documentWorkspaceContext.propertyWriteGuard.addRule(propertyGuardRule);
      }
    }
  }
}

export const api = PropertyGuardWorkspaceContext;

export const PROPERTYGUARD_WORKSPACE_CONTEXT = new UmbContextToken<PropertyGuardWorkspaceContext>(
  'UmbWorkspaceContext',
  'propertyguard.workspacecontext',
);
