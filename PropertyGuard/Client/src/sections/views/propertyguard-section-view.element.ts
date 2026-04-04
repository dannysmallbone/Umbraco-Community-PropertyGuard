import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
  UMB_NOTIFICATION_CONTEXT,
  UmbNotificationContext,
} from '@umbraco-cms/backoffice/notification';
import { UmbSectionViewElement } from '@umbraco-cms/backoffice/section';
import { ProblemDetails, PropertyGuardDto, PropertyGuardService } from '../../api';

@customElement('propertyguard-section-view')
export class PropertyGuardSectionViewElement
  extends UmbLitElement
  implements UmbSectionViewElement
{
  #notificationContext?: UmbNotificationContext;

  @state() private _isLoading: boolean = true;
  @state() private _propertyGuards: PropertyGuardDto[] = [];

  constructor() {
    super();
    this._isLoading = true;
    console.log('🛡️');

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
      this.#notificationContext = notificationContext;
    });

    this.#getPropertyGuards();
  }

  async #getPropertyGuards() {
    const { data, error } = await PropertyGuardService.getPropertyGuards();

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
      this._propertyGuards = data;
      this._isLoading = false;
    }
  }

  render() {
    if (this._isLoading) {
      return html` <div id="loader"><uui-loader></uui-loader></div> `;
    }

    return html`
      <uui-box headline="Property Guards">
          <uui-table>
          <uui-table-head>
            <uui-table-head-cell>
              ${this.localize.term('general_document')}: [${this.localize.term('general_alias')}]
            </uui-table-head-cell>
            <uui-table-head-cell>${this.localize.term('general_message')}</uui-table-head-cell>
          </uui-table-head>
          ${this._propertyGuards.map(
            (propertyGuards) => html`
              <uui-table-row>
                <uui-table-cell>
                  ${propertyGuards.contentTypeAlias}: (${propertyGuards.propertyAlias})
                </uui-table-cell>
                <uui-table-cell>${propertyGuards.message}</uui-table-cell>
              </uui-table-row>
            `,
          )}
        </uui-table>
        </div>
      </uui-box>
    `;
  }

  static override styles = [
    css`
      :host {
        display: block;
        height: 100%;
        padding: var(--uui-size-layout-1);
      }

      #loader {
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
      }

      uui-box {
        --uui-box-default-padding: 0;
      }
    `,
  ];
}

export default PropertyGuardSectionViewElement;

declare global {
  interface HTMLElementTagNameMap {
    'propertyguard-section-view': PropertyGuardSectionViewElement;
  }
}
