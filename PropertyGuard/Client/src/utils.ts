import { PropertyGuardDto, PropertyGuardMode } from './api/types.gen';

export function permissionsToMode(permissions: string[]): PropertyGuardMode {
  return permissions.includes('Read') ? 'ReadOnly' : 'Hidden';
}

export function modeLabel(mode: PropertyGuardMode): string {
  return mode === 'ReadOnly' ? 'Read Only' : 'Hidden';
}

export function guardDisplayName(item: PropertyGuardDto, localize: (key: string) => string): string {
  const documentName = localize(item.documentTypeName ?? item.documentTypeAlias);
  const propertyName = item.propertyTypeName
    ? `${localize(item.propertyTypeName)} (${item.propertyAlias})`
    : item.propertyAlias;
  return `${documentName}: ${propertyName}`;
}
