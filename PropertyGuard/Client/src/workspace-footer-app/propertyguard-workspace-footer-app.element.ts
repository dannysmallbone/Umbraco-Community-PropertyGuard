import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { css, customElement, html, LitElement, state } from '@umbraco-cms/backoffice/external/lit';
import { PROPERTYGUARD_WORKSPACE_CONTEXT } from '../workspace-contexts/propertyguard-workspace-context';

@customElement('propertyguard-footer-app')
export class PropertyGuardFooterAppElement extends UmbElementMixin(LitElement) {
  @state() _hasPropertyGuards: boolean = false;

  constructor() {
    super();

    this.consumeContext(PROPERTYGUARD_WORKSPACE_CONTEXT, (propertyGuardContext) => {
      if (!propertyGuardContext) return;

      propertyGuardContext.observe(propertyGuardContext.hasPropertyGuards, (hasPropertyGuards) => {
        this._hasPropertyGuards = hasPropertyGuards;
      });
    });
  }

  render() {
    if (this._hasPropertyGuards) {
      return html`
        <div id="info">
          Some properties are protected by Property Guard
          <uui-badge look="primary">
            <uui-icon name="icon-shield"></uui-icon>
          </uui-badge>
        </div>
      `;
    }
  }

  static styles = [
    css`
      :host {
        display: block;
        padding: var(--uui-size-3);
      }

      #info {
        padding: 5px 20px 5px 10px;
        font-size: var(--uui-type-small-size, 12px);
        position: relative;
      }

      uui-badge {
        margin-top: var(--uui-size-3);
      }
    `,
  ];
}

export default PropertyGuardFooterAppElement;

declare global {
  interface HTMLElementTagNameMap {
    'propertyguard-footer-app': PropertyGuardFooterAppElement;
  }
}
