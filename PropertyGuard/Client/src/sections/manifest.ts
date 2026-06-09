import {
  UMB_SECTION_ALIAS_CONDITION_ALIAS,
  UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/section';

const sectionAlias = 'PropertyGuard.Section';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'section',
    name: 'Property Guard Section',
    alias: sectionAlias,
    weight: 400,
    meta: {
      label: 'Guards',
      pathname: 'guards',
    },
    conditions: [
      {
        alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
        match: sectionAlias,
      },
    ],
  },
  {
    type: 'sectionView',
    name: 'Property Guard Section View',
    alias: `${sectionAlias}.Overview`,
    element: () => import('./views/propertyguard-section-view.element'),
    meta: {
      label: 'Guards',
      pathname: 'guards',
      icon: 'icon-shield',
    },
    conditions: [
      {
        alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
        match: sectionAlias,
      },
    ],
  },
];
