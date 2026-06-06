import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSectionViewElement } from '@umbraco-cms/backoffice/section';
import { PROPERTYGUARD_CONTEXT } from '../../global-context/propertyguard-context';
import { PropertyGuardDto } from '../../api/types.gen';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_MODAL } from '@umbraco-cms/backoffice/document';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { DEFAULT_FEATURE, DEFAULT_GROUP } from '../../constants';

const SOURCE_COLORS: Record<string, 'default' | 'positive' | 'warning' | 'danger' | 'invalid' | ''> = {
  Code: 'default',
  Config: 'positive',
  Ui: 'danger',
};

@customElement('propertyguard-section-view')
export class PropertyGuardSectionViewElement extends UmbLitElement implements UmbSectionViewElement {
  @state() private _propertyGuards: PropertyGuardDto[] = [];
  @state() private _selectedFeatureKey: string = DEFAULT_FEATURE;
  @state() private _filteredPropertyGuards: PropertyGuardDto[] = [];
  @state() private _selectedFeatureGroup: string = '';
  @state() private _featureKeys: string[] = [];
  @state() private _featureGroups: string[] = [];
  @state() private _addingFeature = false;
  @state() private _pendingFeatureName = '';
  @state() private _addingGroup = false;
  @state() private _pendingGroupName = '';
  @state() private _filterQuery = '';

  constructor() {
    super();

    this.consumeContext(PROPERTYGUARD_CONTEXT, (propertyGuardContext) => {
      if (!propertyGuardContext) return;

      this.observe(
        propertyGuardContext.propertyGuards,
        (propertyGuards) => {
          const uiGuards = this._propertyGuards.filter((g) => g.source === 'Ui');
          this._propertyGuards = [...propertyGuards, ...uiGuards];
          this._featureKeys = this._getFeatureKeys();

          if (!this._featureKeys.includes(this._selectedFeatureKey) && this._featureKeys.length > 0) {
            this._selectedFeatureKey = this._featureKeys[0];
          }

          this._featureGroups = this._getGroupsForFeature(this._selectedFeatureKey);

          if (!this._featureGroups.includes(this._selectedFeatureGroup)) {
            this._selectedFeatureGroup = this._featureGroups[0] ?? '';
          }
        },
        '_propertyGuardsObserver',
      );
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

    if (changedProperties.has('_filterQuery')) {
      const visibleFeatures = this._visibleFeatureKeys;
      if (visibleFeatures.length > 0 && !visibleFeatures.includes(this._selectedFeatureKey)) {
        this._selectedFeatureKey = visibleFeatures[0];
        this._featureGroups = this._getGroupsForFeature(this._selectedFeatureKey);
        this._selectedFeatureGroup = this._featureGroups[0] ?? '';
      }

      const visibleGroups = this._visibleGroups;
      if (visibleGroups.length > 0 && !visibleGroups.includes(this._selectedFeatureGroup)) {
        this._selectedFeatureGroup = visibleGroups[0];
      }
    }

    if (changedProperties.has('_addingFeature') && this._addingFeature) {
      requestAnimationFrame(() => this.shadowRoot?.querySelector<HTMLElement>('.feature-input')?.focus());
    }

    if (changedProperties.has('_addingGroup') && this._addingGroup) {
      requestAnimationFrame(() => this.shadowRoot?.querySelector<HTMLInputElement>('.group-input')?.focus());
    }
  }

  private _getFeatureKeys() {
    return [...new Set(this._propertyGuards.map((g) => g.featureKey.split('.')[0]))];
  }

  private _getGroupsForFeature(featureKey: string): string[] {
    return [
      ...new Set(
        this._propertyGuards
          .filter((g) => g.featureKey.startsWith(`${featureKey}.`))
          .map((g) => g.featureKey.split('.')[1] || 'General'),
      ),
    ].sort();
  }

  private _updateFilteredPropertyGuards() {
    this._filteredPropertyGuards = this._propertyGuards.filter((propertyGuard) => {
      const featureKeyMatch = propertyGuard.featureKey.startsWith(`${this._selectedFeatureKey}.`);
      const groupMatch = (propertyGuard.featureKey.split('.')[1] || 'General') === this._selectedFeatureGroup;
      return featureKeyMatch && groupMatch;
    });
  }

  #matchesQuery(guard: PropertyGuardDto): boolean {
    const query = this._filterQuery.toLowerCase();
    if (!query) return true;
    return (
      this.#createName(guard).toLowerCase().includes(query) ||
      guard.propertyAlias.toLowerCase().includes(query) ||
      guard.featureKey.toLowerCase().includes(query)
    );
  }

  private get _visibleFeatureKeys() {
    return this._filterQuery
      ? this._featureKeys.filter((key) =>
          this._propertyGuards.some((g) => g.featureKey.startsWith(`${key}.`) && this.#matchesQuery(g)),
        )
      : this._featureKeys;
  }

  private get _visibleGroups() {
    return this._filterQuery
      ? this._featureGroups.filter((group) =>
          this._propertyGuards.some(
            (g) =>
              g.featureKey.startsWith(`${this._selectedFeatureKey}.`) &&
              (g.featureKey.split('.')[1] || 'General') === group &&
              this.#matchesQuery(g),
          ),
        )
      : this._featureGroups;
  }

  #createName(item: PropertyGuardDto) {
    const documentName = `${this.localize.string(item.documentTypeName ?? item.documentTypeAlias)}`;
    const propertyName = item.propertyTypeName
      ? `${this.localize.string(item.propertyTypeName)} (${item.propertyAlias})`
      : `${item.propertyAlias}`;

    return `${documentName}: ${propertyName}`;
  }

  async #addPropertyGuard() {
    const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
    if (!modalManager) {
      throw new Error('Could not open modal, no modal manager found');
    }

    const modal = modalManager.open(this, UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_MODAL, {
      data: {
        preset: {
          verbs: ['Umb.Document.PropertyValue.Read'],
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
      const { data: documentType } = await documentTypeRepository.requestByUnique(value.documentType.unique);

      if (!documentType) return;

      const propertyType = documentType?.properties.find((p) => p.unique === value.propertyType.unique);

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
        permissions: value.verbs.map((v) => v.replace('Umb.Document.PropertyValue.', '')),
        message: 'Property is protected by Property Guard',
        source: 'Ui',
      };

      this._propertyGuards = [...this._propertyGuards, propertyGuard];
    } catch {
      // user cancelled
    }
  }

  private get _hasUiGuards() {
    return this._propertyGuards.some((g) => g.source === 'Ui');
  }

  render() {
    return html`
      <umb-workspace-editor>
        <uui-box headline="Property Guards">
          <div slot="header-actions" class="header-filter">
            <uui-input
              label="Filter"
              placeholder="Type to filter..."
              .value=${this._filterQuery}
              @input=${(e: InputEvent) => (this._filterQuery = (e.target as HTMLInputElement).value)}
            ></uui-input>
          </div>
          <div class="container">
            ${this.#renderSidebar()}
            <div class="content">
              ${this.#renderTabs()}
              <div class="property-guards">${this.#renderPropertyGuards()}</div>
            </div>
          </div>
        </uui-box>
        <div slot="actions">
          <uui-button
            look="secondary"
            color="positive"
            label="Copy config"
            ?disabled=${!this._hasUiGuards}
            @click=${this.#copyConfig}
          >
            Copy config
          </uui-button>
          <uui-button look="primary" color="positive" label="Save" ?disabled=${!this._hasUiGuards}>Save</uui-button>
        </div>
      </umb-workspace-editor>
    `;
  }

  #renderPropertyGuards() {
    const visible = this._filteredPropertyGuards.filter((g) => this.#matchesQuery(g));

    return html`
      <uui-ref-list>
        ${repeat(
          visible,
          (propertyGuard) => `${propertyGuard.documentTypeAlias}-${propertyGuard.propertyAlias}`,
          (propertyGuard) => this.#renderPropertyGuard(propertyGuard),
        )}
      </uui-ref-list>
      ${this.#renderAddButton()}
    `;
  }

  #renderAddButton() {
    if (this._visibleGroups.length === 0) return nothing;
    return html`
      <uui-button
        class="btn-add"
        look="placeholder"
        label=${this.localize.term('general_add')}
        @click=${this.#addPropertyGuard}
      >
        <uui-icon name="add"></uui-icon>
      </uui-button>
    `;
  }

  #modeLabel(permissions: string[]): string {
    return permissions.includes('Read') ? 'Read Only' : 'Hidden';
  }

  #renderPropertyGuard(propertyGuard: PropertyGuardDto) {
    const icon = propertyGuard.propertyTypeUnique ? propertyGuard.icon : 'alert color-red';
    const alias = propertyGuard.propertyTypeUnique ? this.#modeLabel(propertyGuard.permissions) : 'Property not found!';
    const name = this.#createName(propertyGuard);

    return html`
      <uui-ref-node-document-type
        name=${name}
        alias=${alias}
        .detail=${propertyGuard.propertyTypeUnique ? propertyGuard.message : ''}
        readonly
      >
        ${icon ? html`<umb-icon slot="icon" name=${icon}></umb-icon>` : nothing}
        <uui-tag slot="tag" look="primary" color=${SOURCE_COLORS[propertyGuard.source] ?? 'default'}>
          ${propertyGuard.source.toUpperCase()}
        </uui-tag>
        <uui-action-bar slot="actions">
          <uui-button
            label="Edit"
            look="secondary"
            compact
            ?disabled=${propertyGuard.source !== 'Ui'}
            @click=${() => this.#editPropertyGuard(propertyGuard)}
          >
            <uui-icon name="icon-edit" style="--uui-icon-color: currentColor;"></uui-icon>
          </uui-button>
          <uui-button
            label="Remove"
            look="secondary"
            compact
            ?disabled=${propertyGuard.source !== 'Ui'}
            @click=${() => this.#removePropertyGuard(propertyGuard)}
          >
            <uui-icon name="icon-trash" style="--uui-icon-color: currentColor;"></uui-icon>
          </uui-button>
        </uui-action-bar>
      </uui-ref-node-document-type>
    `;
  }

  #renderTabs(): unknown {
    return html`
      <div class="tab-bar">
        <uui-tab-group>
          ${this._visibleGroups.map(
            (group) => html`
              <uui-tab
                label=${group}
                ?active=${this._selectedFeatureGroup === group}
                @click=${() => (this._selectedFeatureGroup = group)}
              ></uui-tab>
            `,
          )}
          ${this._visibleFeatureKeys.length > 0
            ? this._addingGroup
              ? html`
                  <uui-tab class="tab-input" active>
                    <input
                      class="group-input"
                      type="text"
                      placeholder=""
                      .value=${this._pendingGroupName}
                      @input=${(e: InputEvent) => (this._pendingGroupName = (e.target as HTMLInputElement).value)}
                      @keydown=${(e: KeyboardEvent) => {
                        if (e.key === 'Enter') this.#confirmAddGroup();
                        if (e.key === 'Escape') {
                          this._addingGroup = false;
                          this._pendingGroupName = '';
                        }
                      }}
                      @blur=${() => this.#confirmAddGroup()}
                    />
                  </uui-tab>
                `
              : html`
                  <uui-tab class="btn-add" @click=${() => (this._addingGroup = true)}>
                    <uui-icon slot="icon" name="add"></uui-icon>
                  </uui-tab>
                `
            : nothing}
        </uui-tab-group>
      </div>
    `;
  }

  #confirmAddGroup() {
    const trimmed = this._pendingGroupName.trim();
    if (!trimmed && this._featureGroups.length > 0) {
      this._addingGroup = false;
      this._pendingGroupName = '';
      return;
    }
    const name = trimmed || DEFAULT_GROUP;
    this._addingGroup = false;
    this._pendingGroupName = '';
    if (this._featureGroups.includes(name)) {
      this._selectedFeatureGroup = name;
      return;
    }
    this._featureGroups = [...this._featureGroups, name];
    this._selectedFeatureGroup = name;
  }

  #renderSidebar() {
    return html`
      <div class="sidebar">
        ${this._visibleFeatureKeys.map(
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
        ${this._addingFeature
          ? html`
              <uui-input
                class="feature-input"
                type="text"
                placeholder="Feature name"
                .value=${this._pendingFeatureName}
                @input=${(e: InputEvent) => (this._pendingFeatureName = (e.target as HTMLInputElement).value)}
                @keydown=${(e: KeyboardEvent) => {
                  if (e.key === 'Enter') this.#confirmAddFeature();
                  if (e.key === 'Escape') {
                    this._addingFeature = false;
                    this._pendingFeatureName = '';
                  }
                }}
                @focusout=${() => this.#confirmAddFeature()}
              ></uui-input>
            `
          : html`
              <uui-button
                class="btn-add"
                look="placeholder"
                label="Add feature"
                @click=${() => (this._addingFeature = true)}
              >
                <uui-icon name="add"></uui-icon>
              </uui-button>
            `}
      </div>
    `;
  }

  async #editPropertyGuard(guard: PropertyGuardDto) {
    const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
    if (!modalManager) throw new Error('Could not open modal, no modal manager found');

    const modal = modalManager.open(this, UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_MODAL, {
      data: {
        preset: {
          documentType: guard.documentTypeUnique ? { unique: guard.documentTypeUnique } : undefined,
          propertyType: guard.propertyTypeUnique ? { unique: guard.propertyTypeUnique } : undefined,
          verbs: guard.permissions.map((p) => `Umb.Document.PropertyValue.${p}`),
        },
        pickablePropertyTypeFilter: (propertyType) =>
          propertyType.unique === guard.propertyTypeUnique ||
          !this._filteredPropertyGuards.some((g) => g.propertyTypeUnique === propertyType.unique),
      },
    });

    try {
      const value = await modal?.onSubmit();
      this._propertyGuards = this._propertyGuards.map((g) =>
        g === guard ? { ...g, permissions: value.verbs.map((v) => v.replace('Umb.Document.PropertyValue.', '')) } : g,
      );
    } catch {
      // user cancelled
    }
  }

  #removePropertyGuard(guard: PropertyGuardDto) {
    this._propertyGuards = this._propertyGuards.filter((g) => g !== guard);
  }

  async #copyConfig() {
    const uiGuards = this._propertyGuards.filter((g) => g.source === 'Ui');
    const definitions = uiGuards.map((g) => {
      const mode = g.permissions.includes('Read') ? 'ReadOnly' : 'Hidden';
      const entry: Record<string, unknown> = {
        DocumentTypeAlias: g.documentTypeAlias,
        PropertyAlias: g.propertyAlias,
        FeatureKey: g.featureKey,
        Message: g.message,
      };
      if (mode !== 'ReadOnly') {
        entry['Mode'] = mode;
      }
      return entry;
    });

    const snippet = JSON.stringify({ PropertyGuard: { Definitions: definitions } }, null, 2);
    await navigator.clipboard.writeText(snippet);

    const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
    notificationContext?.peek('positive', { data: { headline: 'Config snippet copied to clipboard', message: '' } });
  }

  #confirmAddFeature() {
    const name = this._pendingFeatureName.trim();
    this._addingFeature = false;
    this._pendingFeatureName = '';
    if (!name) return;

    if (this._featureKeys.includes(name)) {
      this._selectedFeatureKey = name;
      this._featureGroups = this._getGroupsForFeature(name);
      this._selectedFeatureGroup = this._featureGroups[0] ?? '';
      return;
    }

    this._featureKeys = [...this._featureKeys, name];
    this._selectedFeatureKey = name;
    this._featureGroups = [];
    this._selectedFeatureGroup = '';
    this._addingGroup = true;
  }

  static override styles = [
    css`
      :host {
        display: block;
        height: 100%;
      }

      .header-filter {
        display: flex;
        align-items: center;
        padding: 0 var(--uui-size-space-3);
      }

      uui-box {
        margin: var(--uui-size-layout-2);
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

      .feature-input {
        width: 100%;
      }

      uui-tab.tab-input {
        min-width: 120px;
      }

      .group-input {
        background: transparent;
        border: none;
        font: inherit;
        color: inherit;
        outline: none;
        padding: 0 var(--uui-size-space-2);
        width: 100%;
        box-sizing: border-box;
        text-align: center;
      }

      .btn-add {
        width: 100%;

        uui-icon {
          font-size: var(--uui-font-size);
          margin: 0;
        }
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
