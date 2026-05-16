import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import {
  css,
  customElement,
  html,
  LitElement,
  nothing,
  repeat,
} from '@umbraco-cms/backoffice/external/lit';
import { PROPERTYGUARD_WORKSPACE_CONTEXT } from '../workspace-contexts/propertyguard-workspace-context';
import { PropertyGuardDto } from '../api/types.gen';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

@customElement('propertyguard-workspace-view')
export class PropertyGuardWorkspaceViewElement extends UmbElementMixin(LitElement) {
  private _propertyGuards: PropertyGuardDto[] = [];

  constructor() {
    super();

    this.consumeContext(PROPERTYGUARD_WORKSPACE_CONTEXT, (propertyGuardWorkspaceContext) => {
      if (!propertyGuardWorkspaceContext) return;

      propertyGuardWorkspaceContext.observe(
        observeMultiple([
          propertyGuardWorkspaceContext.hasPropertyGuards,
          propertyGuardWorkspaceContext.documentPropertyGuards,
        ]),
        ([hasPropertyGuards, propertyGuards]) => {
          if (hasPropertyGuards) {
            this._propertyGuards = propertyGuards;
          }
        },
      );
    });
  }

  private createName(item: PropertyGuardDto) {
    const documentName = `${this.localize.string(item.documentTypeName ?? item.documentTypeAlias)}`;
    const propertyName = item.propertyTypeName
      ? `${this.localize.string(item.propertyTypeName)} (${item.propertyAlias})`
      : `${item.propertyAlias}`;

    return `${documentName}: ${propertyName}`;
  }

  render() {
    return html`
      <uui-box><div class="property-guards">${this.#renderPropertyGuards()}</div></uui-box>
    `;
  }

  #renderPropertyGuards() {
    if (!this._propertyGuards) return;

    return html`
      <div slot="editor">
        <uui-ref-list>
          ${repeat(
            this._propertyGuards,
            (propertyGuard) => `${propertyGuard.documentTypeAlias}-${propertyGuard.propertyAlias}`,
            (propertyGuard) => this.#renderPropertyGuard(propertyGuard),
          )}
        </uui-ref-list>
      </div>
    `;
  }

  #renderPropertyGuard(propertyGuard: PropertyGuardDto) {
    let icon = propertyGuard.propertyTypeUnique ? propertyGuard.icon : 'alert color-red';
    let detail = propertyGuard.propertyTypeUnique
      ? propertyGuard.permissions.join(',')
      : 'Property not found!';

    let name = this.createName(propertyGuard);

    return html`
      <uui-ref-node-document-type
        name=${name}
        alias=${detail}
        .detail=${propertyGuard.propertyTypeUnique ? propertyGuard.message : ''}
        readonly
      >
        ${icon ? html`<umb-icon slot="icon" name=${icon}></umb-icon>` : nothing}
        <h1 slot="default">Help</h1>
        <uui-tag slot="tag" color=${propertyGuard.propertyTypeUnique ? 'default' : 'danger'}
          >${propertyGuard.featureKey}</uui-tag
        >
      </uui-ref-node-document-type>
    `;
  }

  static override styles = [
    css`
      :host {
        display: block;
        padding: var(--uui-size-layout-1);
      }
    `,
  ];
}

export default PropertyGuardWorkspaceViewElement;

declare global {
  interface HTMLElementTagNameMap {
    'propertyguard-workspace-view': PropertyGuardWorkspaceViewElement;
  }
}
