

param name string
param location string
param lawname string

@description('Number of CPU cores the container can use. Can be with a maximum of two decimals.')
param cpuCore string = '1'

@description('Amount of memory (in gibibytes, GiB) allocated to the container up to 4GiB. Can be with a maximum of two decimals. Ratio with CPU cores must be equal to 2.')
param memorySize string = '2'

@description('Minimum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param minReplicas int = 1

@description('Maximum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param maxReplicas int = 1

param storageAccountName string

param fileshareName string

resource law 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: lawname
}

resource env 'Microsoft.App/managedEnvironments@2022-03-01'= {
  name: 'containerapp-env-${name}'
  location: location
  properties: {   
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: law.properties.customerId
        sharedKey: law.listKeys().primarySharedKey
      }
    }    
  }
}

var storageName = 'acastorage'

resource stg 'Microsoft.Storage/storageAccounts@2021-02-01' existing = {
  name: storageAccountName
}

resource envStorage 'Microsoft.App/managedEnvironments/storages@2022-03-01' = {
  parent: env
  name: storageName
  properties: {
    azureFile: {
      accessMode: 'ReadWrite'
      accountKey: stg.listKeys().keys[0].value
      accountName: storageAccountName
      shareName: fileshareName
    }
  }
}

// qdrant container
resource containerApp 'Microsoft.App/containerApps@2023-11-02-preview' = {
  name: 'ca-${name}-mcp'
  dependsOn: [
    envStorage
  ]
  location: location  
  properties: {
    managedEnvironmentId: env.id    
    configuration: {      
      ingress: {        
        external: true
        targetPort: 8080
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100            
          }
        ]
      }
    }
    template: {
      volumes: [
        {
          name: 'externalstorage'
          storageName: storageName
          storageType: 'AzureFile'          
        }
      ]      
      containers: [
        {          
          name: 'ca-${name}-mcp'
          image: 'ghcr.io/sjkp/bolagsverket:latest'
          resources: {
            cpu: json(cpuCore)
            memory: '${memorySize}Gi'
          }
          volumeMounts: [
            {
              mountPath: '/app/data'
              volumeName: 'externalstorage'
            }
          ]
          env: [
            {             
              name: 'SqliteDatabasePath'
              value: '/app/data/bolagsverket.db'
            }
            {
              name: 'CloudZipFilePath' 
              value: '/usr/local/bin/cz'
            }
            {
              name: 'LookupTableFilePath'
              value: '/app/data/database.txt'
            }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }    
  }
}


