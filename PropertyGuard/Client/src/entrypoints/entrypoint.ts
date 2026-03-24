import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { client } from '../api/client.gen';

export const onInit: UmbEntryPointOnInit = (_host, _extensionRegistry) => {
  _host.consumeContext(UMB_AUTH_CONTEXT, async (authContext) => {
    const config = authContext?.getOpenApiConfiguration();

    if (!config) {
      console.warn(
        'No OpenAPI configuration in auth context, Property Guard API will not be initialized',
      );
      return;
    }

    client.setConfig({
      baseUrl: config.base,
      credentials: config.credentials,
      auth: () => config.token(),
    });
  });
};
