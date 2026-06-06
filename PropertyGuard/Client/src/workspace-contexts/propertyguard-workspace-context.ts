import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { PropertyGuardDto } from '../api';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT, UmbDocumentWorkspaceContext } from '@umbraco-cms/backoffice/document';
import { PROPERTYGUARD_CONTEXT, PropertyGuardContext } from '../global-context/propertyguard-context';
import { UmbPropertyGuardRule } from '@umbraco-cms/backoffice/property';

export class PropertyGuardWorkspaceContext extends UmbContextBase {
  #propertyGuardContext?: PropertyGuardContext;
  #documentWorkspaceContext?: UmbDocumentWorkspaceContext;
  #currentContentTypeAliases: string[] = [];

  #hasPropertyGuards = new UmbBooleanState(false);
  hasPropertyGuards = this.#hasPropertyGuards.asObservable();

  #documentPropertyGuards = new UmbArrayState<PropertyGuardDto>([], (g) => g.propertyAlias);
  documentPropertyGuards = this.#documentPropertyGuards.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, PROPERTYGUARD_WORKSPACE_CONTEXT);

    this.consumeContext(PROPERTYGUARD_CONTEXT, (propertyGuardContext) => {
      if (!propertyGuardContext) return;
      this.#propertyGuardContext = propertyGuardContext;

      this.observe(propertyGuardContext.propertyGuards, () => this.#applyGuards(), '_propertyGuardsObserver');
    });

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, async (documentWorkspaceContext) => {
      if (!documentWorkspaceContext) return;
      this.#documentWorkspaceContext = documentWorkspaceContext;

      await documentWorkspaceContext.structure.whenLoaded();

      this.observe(
        documentWorkspaceContext.structure.contentTypeAliases,
        (aliases) => {
          this.#currentContentTypeAliases = aliases;
          this.#applyGuards();
        },
        '_contentTypeAliasesObserver',
      );
    });
  }

  #applyGuards() {
    if (!this.#documentWorkspaceContext || !this.#propertyGuardContext) return;
    if (!this.#currentContentTypeAliases.length) return;

    const guards = this.#propertyGuardContext.getPropertyGuardsForDocumentTypes(this.#currentContentTypeAliases);

    this.#documentPropertyGuards.setValue(guards);
    this.#hasPropertyGuards.setValue(guards.length > 0);

    for (const guard of guards) {
      if (!guard.propertyTypeUnique) continue;

      const permissions = guard.permissions ?? ['Read'];

      if (!permissions.includes('Read')) {
        const viewRule: UmbPropertyGuardRule = {
          unique: `propertyguard-view-${guard.documentTypeAlias}-${guard.propertyAlias}`,
          permitted: false,
          message: guard.message,
          propertyType: { unique: guard.propertyTypeUnique },
        };
        this.#documentWorkspaceContext.propertyViewGuard.addRule(viewRule);
      }

      if (!permissions.includes('Write')) {
        const writeRule: UmbPropertyGuardRule = {
          unique: `propertyguard-write-${guard.documentTypeAlias}-${guard.propertyAlias}`,
          permitted: false,
          message: guard.message,
          propertyType: { unique: guard.propertyTypeUnique },
        };
        this.#documentWorkspaceContext.propertyWriteGuard.addRule(writeRule);
      }
    }
  }
}

export const api = PropertyGuardWorkspaceContext;

export const PROPERTYGUARD_WORKSPACE_CONTEXT = new UmbContextToken<PropertyGuardWorkspaceContext>(
  'UmbWorkspaceContext',
  'propertyguard.workspacecontext',
);
