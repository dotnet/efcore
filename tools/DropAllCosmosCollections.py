# This script drops all collection from azure cosmos emulator exception Northwind
# Script prerequisites
# Install Python 3.7+
# Run pip install azure-cosmos

import azure.cosmos.cosmos_client as cosmos_client

config = {
    'DEFAULTCONNECTION': 'https://localhost:8081',
    'AUTHTOKEN': 'C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=='
}

# Initialize the Cosmos client
client = cosmos_client.CosmosClient(url_connection=config['DEFAULTCONNECTION'], auth={'masterKey': config['AUTHTOKEN']})

databases = client.ReadDatabases()

for a in databases:
    if (a['id'] == "Northwind"):
        continue
    client.DeleteDatabase(a['_self'])