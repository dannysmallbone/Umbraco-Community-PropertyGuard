import { PropertyGuardDto } from './api/types.gen';

export function modeLabel(permissions: string[]): string {
  return permissions.includes('Read') ? 'Read Only' : 'Hidden';
}

export function guardDisplayName(item: PropertyGuardDto, localize: (key: string) => string): string {
  const documentName = localize(item.documentTypeName ?? item.documentTypeAlias);
  const propertyName = item.propertyTypeName
    ? `${localize(item.propertyTypeName)} (${item.propertyAlias})`
    : item.propertyAlias;
  return `${documentName}: ${propertyName}`;
}
