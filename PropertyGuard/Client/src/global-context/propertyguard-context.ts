import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ProblemDetails, PropertyGuardDto, PropertyGuardService } from '../api';
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { client } from '../api/client.gen';

export class PropertyGuardContext extends UmbContextBase {
  #notificationContext?: UmbNotificationContext;

  #propertyGuards = new UmbArrayState<PropertyGuardDto>([], (propertyGuard) => this.getPropertyGuardKey(propertyGuard));
  propertyGuards = this.#propertyGuards.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, PROPERTYGUARD_CONTEXT);
    console.log('Property Guard enabled! 🛡️');

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
      this.#notificationContext = notificationContext;
    });

    this.consumeContext(UMB_AUTH_CONTEXT, async (authContext) => {
      if (!authContext) {
        console.warn('PropertyGuardContext: Auth context not available');
        return;
      }

      const config = authContext.getOpenApiConfiguration();

      if (!config) {
        console.warn('No OpenAPI configuration in auth context, Property Guard API will not be initialized');
        return;
      }

      client.setConfig({
        baseUrl: config.base,
        credentials: config.credentials,
        auth: () => config.token(),
      });

      await this.#loadPropertyGuards();
    });
  }

  private getPropertyGuardKey(propertyGuard: PropertyGuardDto): string {
    return `${propertyGuard.documentTypeAlias}-${propertyGuard.propertyAlias}`.toLocaleLowerCase();
  }

  async #loadPropertyGuards() {
    try {
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
        this.#propertyGuards.setValue(data);
      }
    } catch (error) {
      const message = 'Failed to load property guards!';
      console.error(`PropertyGuardContext: ${message}`, error);
      this.#notificationContext?.peek('danger', { data: { headline: 'Error', message: message } });
    }
  }

  public getPropertyGuardsForDocumentTypes(documentTypeAliases: string[]) {
    return this.#propertyGuards
      .getValue()
      .filter((propertyGuard) => documentTypeAliases.includes(propertyGuard.documentTypeAlias));
  }
}

export const api = PropertyGuardContext;

export const PROPERTYGUARD_CONTEXT = new UmbContextToken<PropertyGuardContext>(
  'PropertyGuardContext',
  'propertyguard.context',
);
