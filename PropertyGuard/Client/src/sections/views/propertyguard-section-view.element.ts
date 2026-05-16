import {
  css,
  customElement,
  html,
  nothing,
  repeat,
  state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSectionViewElement } from '@umbraco-cms/backoffice/section';
import { PROPERTYGUARD_CONTEXT } from '../../global-context/propertyguard-context';
import { PropertyGuardDto } from '../../api/types.gen';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_MODAL } from '@umbraco-cms/backoffice/document';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';

@customElement('propertyguard-section-view')
export class PropertyGuardSectionViewElement
  extends UmbLitElement
  implements UmbSectionViewElement
{
  @state() private _propertyGuards: PropertyGuardDto[] = [];
  @state() private _selectedFeatureKey: string = '';
  @state() private _filteredPropertyGuards: PropertyGuardDto[] = [];
  @state() private _selectedFeatureGroup: string = '';
  @state() private _featureKeys: string[] = [];
  @state() private _featureGroups: string[] = [];

  constructor() {
    super();

    this.consumeContext(PROPERTYGUARD_CONTEXT, (propertyGuardContext) => {
      if (!propertyGuardContext) return;

      propertyGuardContext.observe(propertyGuardContext.propertyGuards, (propertyGuards) => {
        this._propertyGuards = propertyGuards;
        this._featureKeys = this._getFeatureKeys();

        if (this._featureKeys.length > 0) {
          this._selectedFeatureKey = this._featureKeys[0];
          this._featureGroups = this._getGroupsForFeature(this._selectedFeatureKey);
          if (this._featureGroups.length > 0) this._selectedFeatureGroup = this._featureGroups[0];
        }
      });
    });
  }

  updated(changedProperties: Map<string, unknown>) {
    if (
      changedProperties.has('_selectedFeatureKey') ||
      changedProperties.has('_selectedFeatureGroup') ||
      changedProperties.has('_propertyGuards')
    ) {
      this._updateFilteredPropertyGuards();
    }
  }

  private _getFeatureKeys() {
    const featureKeySet = new Set<string>();
    this._propertyGuards.forEach((propertyGuard) => {
      const featureKey = propertyGuard.featureKey.split('.')[0];
      featureKeySet.add(featureKey);
    });

    return Array.from(featureKeySet);
  }

  private _getGroupsForFeature(featureKey: string): string[] {
    const groupSet = new Set<string>();
    this._propertyGuards
      .filter((propertyGuard) => propertyGuard.featureKey.startsWith(`${featureKey}.`))
      .forEach((propertyGuard) =>
        groupSet.add(propertyGuard.featureKey.split('.')[1] || 'General'),
      );

    return Array.from(groupSet).sort();
  }

  private _updateFilteredPropertyGuards() {
    this._filteredPropertyGuards = this._propertyGuards.filter((propertyGuard) => {
      const featureKeyMatch = propertyGuard.featureKey.startsWith(`${this._selectedFeatureKey}`);
      const groupMatch =
        (propertyGuard.featureKey.split('.')[1] || 'General') === this._selectedFeatureGroup;
      return featureKeyMatch && groupMatch;
    });
  }

  private createName(item: PropertyGuardDto) {
    const documentName = `${this.localize.string(item.documentTypeName ?? item.documentTypeAlias)}`;
    const propertyName = item.propertyTypeName
      ? `${this.localize.string(item.propertyTypeName)} (${item.propertyAlias})`
      : `${item.propertyAlias}`;

    return `${documentName}: ${propertyName}`;
  }

  async #addpropertyGuard() {
    const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
    if (!modalManager) {
      throw new Error('Could not open modal, no modal manager found');
    }

    const modal = modalManager.open(this, UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_MODAL, {
      data: {
        preset: {
          verbs: [...new Set(['Umb.Document.PropertyValue.Read'])],
        },
        pickablePropertyTypeFilter: (propertyType) =>
          !this._filteredPropertyGuards.some(
            (propertyGuard) => propertyGuard.propertyTypeUnique === propertyType.unique,
          ),
      },
    });

    try {
      const value = await modal?.onSubmit();

      const documentTypeRepository = new UmbDocumentTypeDetailRepository(this);
      const { data: documentType } = await documentTypeRepository.requestByUnique(
        value.documentType.unique,
      );

      if (!documentType) return;

      const propertyType = documentType?.properties.find(
        (p) => p.unique === value.propertyType.unique,
      );

      if (!propertyType) return;

      const propertyGuard: PropertyGuardDto = {
        documentTypeAlias: documentType.alias,
        documentTypeName: documentType.name,
        documentTypeUnique: documentType.unique,
        propertyAlias: propertyType.alias,
        propertyTypeName: propertyType.name,
        propertyTypeUnique: propertyType.unique,
        icon: documentType.icon,
        featureKey: `${this._selectedFeatureKey}.${this._selectedFeatureGroup}`,
        permissions: value.verbs,
        message: '',
      };

      this._propertyGuards = [...this._propertyGuards, propertyGuard];
    } catch (error) {
      console.error(error);
    }
  }

  render() {
    if (!this._propertyGuards.length) return this.#renderNoPropertyGuards();

    return html`
      <uui-box headline="Property Guards">
        <div class="container">
          ${this.#renderSidebar()}
          <div class="content">
            ${this.#renderTabs()}
            <div class="property-guards">${this.#renderPropertyGuards()}</div>
          </div>
        </div>
      </uui-box>
    `;
  }

  #renderPropertyGuards() {
    if (!this._filteredPropertyGuards) return;

    return html`
      <div slot="editor">
        <uui-ref-list>
          ${repeat(
            this._filteredPropertyGuards,
            (propertyGuard) => `${propertyGuard.documentTypeAlias}-${propertyGuard.propertyAlias}`,
            (propertyGuard) => this.#renderPropertyGuard(propertyGuard),
          )}
        </uui-ref-list>
        ${this.#renderAddButton()}
      </div>
    `;
  }

  #renderAddButton() {
    return html`
      <uui-button
        id="btn-add"
        look="placeholder"
        label=${this.localize.term('general_add')}
        @click=${this.#addpropertyGuard}
      >
        <uui-icon name="add"></uui-icon>
      </uui-button>
    `;
  }

  #renderPropertyGuard(propertyGuard: PropertyGuardDto) {
    let icon = propertyGuard.propertyTypeUnique ? propertyGuard.icon : 'alert color-red';
    let detail = propertyGuard.propertyTypeUnique
      ? propertyGuard.permissions.join(',')
      : 'Property not found!';

    let name = this.createName(propertyGuard);

    return html`
      <uui-ref-node .name=${name} .detail=${detail} readonly>
        ${icon ? html`<umb-icon slot="icon" name=${icon}></umb-icon>` : nothing}
      </uui-ref-node>
    `;
  }

  #renderTabs(): unknown {
    const groups = this._featureGroups;

    return html`<uui-tab-group>
      ${groups.map(
        (group) => html`
          <uui-tab
            label=${group}
            ?active=${this._selectedFeatureGroup === group}
            @click=${() => (this._selectedFeatureGroup = group)}
          ></uui-tab>
        `,
      )}
    </uui-tab-group>`;
  }

  #renderSidebar() {
    return html`
      <div class="sidebar">
        ${this._featureKeys.map(
          (featureKey) => html`
            <uui-menu-item
              label=${featureKey.replace(/([A-Z])/g, ' $1').trim()}
              ?active=${this._selectedFeatureKey === featureKey}
              @click=${() => {
                this._selectedFeatureKey = featureKey;
                this._featureGroups = this._getGroupsForFeature(featureKey);
                this._selectedFeatureGroup = this._featureGroups[0] || '';
              }}
            >
            </uui-menu-item>
          `,
        )}
      </div>
    `;
  }

  #renderNoPropertyGuards() {
    return html`<h2 class="no-property-guards">No property guards have been created yet</h2>`;
  }

  static override styles = [
    css`
      :host {
        display: block;
        padding: var(--uui-size-layout-1);
      }

      .no-property-guards {
        display: flex;
        justify-content: space-around;
      }

      uui-box {
        --uui-box-default-padding: 0;
      }

      .container {
        display: grid;
        grid-template-areas: 'sidebar content';
        grid-template-columns: 1fr 3fr;
      }

      .sidebar {
        border-right: 1px solid var(--uui-color-divider-standalone, #e9e9eb);
      }

      uui-tab-group {
        max-height: var(--uui-size-12);
      }

      uui-tab {
        height: fit-content;
      }

      .property-guards {
        display: block;
        padding: var(--uui-size-space-5, 18px);
      }

      #btn-add {
        width: 100%;
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
