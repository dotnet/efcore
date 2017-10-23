# Query Filters

This sample demonstrates Query Filters. A simple Blog and Post model is created and two query filters are defined:

1) The Blog entity is configured with a multi-tenancy pattern. The query filter is based on a ```_tenantId``` field on the context.
2) The Post entity is set up for soft-delete, with an explicit ```IsDeleted``` property.

