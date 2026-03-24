import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { css, customElement, html, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { PROPERTYGUARD_WORKSPACE_CONTEXT } from '../workspaceContexts/propertyguard-workspace-context';
import { PropertyGuardWorkspaceContext } from '../workspaceContexts/propertyguard-workspace-context';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { PropertyGuardDto } from '../api/types.gen';

@customElement('propertyguard-workspace-view')
export class PropertyGuardWorkspaceViewElement extends UmbElementMixin(LitElement) {
  private _propertyGuardContext: PropertyGuardWorkspaceContext | undefined;

  private _propertyGuards: PropertyGuardDto[] = [];

  constructor() {
    super();

    this.consumeContext(PROPERTYGUARD_WORKSPACE_CONTEXT, (propertyGuardContext) => {
      if (!propertyGuardContext) return;

      this._propertyGuardContext = propertyGuardContext;

      this._propertyGuardContext.observe(
        observeMultiple([
          this._propertyGuardContext.hasPropertyGuards,
          this._propertyGuardContext.propertyGuards,
        ]),
        ([hasPropertyGuards, propertyGuards]) => {
          if (hasPropertyGuards) {
            this._propertyGuards = propertyGuards;
          }
        },
      );
    });
  }

  render() {
    return html`
      <uui-box>
        <uui-table>
          <uui-table-head>
            <uui-table-head-cell>${this.localize.term('general_alias')}</uui-table-head-cell>
            <uui-table-head-cell>${this.localize.term('general_message')}</uui-table-head-cell>
          </uui-table-head>
          ${this._propertyGuards.map(
            (propertyGuards) => html`
              <uui-table-row>
                <uui-table-cell>${propertyGuards.propertyAlias}</uui-table-cell>
                <uui-table-cell>${propertyGuards.message}</uui-table-cell>
              </uui-table-row>
            `,
          )}
        </uui-table>
      </uui-box>
    `;
  }

  static override styles = [
    css`
      :host {
        display: block;
        padding: var(--uui-size-layout-1);
      }

      uui-box {
        --uui-box-default-padding: 0;
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
