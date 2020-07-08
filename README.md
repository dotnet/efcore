Repository
==========

[![build status](https://img.shields.io/azure-devops/build/dnceng/public/51/master)](https://dev.azure.com/dnceng/public/_build?definitionId=51) [![test results](https://img.shields.io/azure-devops/tests/dnceng/public/51/master)](https://dev.azure.com/dnceng/public/_build?definitionId=51)

### Usage

#### Step 1:
Extends you models from `Microsoft.EntityFrameworkCore.Infrastructure.ModelExtension`
and implement `LoadRelationsAsync`, `LoadRelations`, `OnSoftDelete` and `OnSoftDeleteAsync`
methods for relations that you want to delete on soft deleting entity.
#### Step 2:
For soft delete entities use `Remove` and `RemoveAsync`.
For force delete entities from database use `ForceRemove`.

### Proposal
Please check [the proposal](https://1drv.ms/b/s!AirwjkMOI-BwkAzedA6E6YVkZqjQ?e=vfV2hq) for this pull request.
