import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { css, customElement, html, LitElement, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { PROPERTYGUARD_WORKSPACE_CONTEXT } from '../workspace-contexts/propertyguard-workspace-context';
import { PropertyGuardDto } from '../api/types.gen';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { modeLabel, guardDisplayName } from '../utils';

@customElement('propertyguard-workspace-view')
export class PropertyGuardWorkspaceViewElement extends UmbElementMixin(LitElement) {
  @state() private _propertyGuards: PropertyGuardDto[] = [];

  constructor() {
    super();

    this.consumeContext(PROPERTYGUARD_WORKSPACE_CONTEXT, (propertyGuardWorkspaceContext) => {
      if (!propertyGuardWorkspaceContext) return;

      this.observe(
        observeMultiple([
          propertyGuardWorkspaceContext.hasPropertyGuards,
          propertyGuardWorkspaceContext.documentPropertyGuards,
        ]),
        ([hasPropertyGuards, propertyGuards]) => {
          this._propertyGuards = hasPropertyGuards ? propertyGuards : [];
        },
        '_workspaceGuardsObserver',
      );
    });
  }

  render() {
    return html` <uui-box><div class="property-guards">${this.#renderPropertyGuards()}</div></uui-box> `;
  }

  #renderPropertyGuards() {
    if (!this._propertyGuards.length) return;

    return html`
      <uui-ref-list>
        ${repeat(
          this._propertyGuards,
          (propertyGuard) => `${propertyGuard.documentTypeAlias}-${propertyGuard.propertyAlias}`,
          (propertyGuard) => this.#renderPropertyGuard(propertyGuard),
        )}
      </uui-ref-list>
    `;
  }

  #renderPropertyGuard(propertyGuard: PropertyGuardDto) {
    const icon = propertyGuard.propertyTypeUnique ? propertyGuard.icon : 'alert color-red';
    const alias = propertyGuard.propertyTypeUnique ? modeLabel(propertyGuard.mode) : 'Property not found!';
    const name = guardDisplayName(propertyGuard, (key) => this.localize.string(key));

    return html`
      <uui-ref-node-document-type
        .name=${name}
        .alias=${alias}
        .detail=${propertyGuard.propertyTypeUnique ? propertyGuard.message : ''}
        readonly
      >
        ${icon ? html`<umb-icon slot="icon" name=${icon}></umb-icon>` : nothing}
        <uui-tag slot="tag" color=${propertyGuard.propertyTypeUnique ? 'default' : 'danger'}>
          ${propertyGuard.featureKey}
        </uui-tag>
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
