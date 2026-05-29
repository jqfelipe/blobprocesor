# Blob Processor Microservice (.NET + Azure Functions)

Este microservicio en .NET se ejecuta cuando se crea un archivo en un contenedor de Azure Blob Storage, procesa el contenido y publica el resultado en un topico de Azure Service Bus.

## Arquitectura

- Trigger: `BlobTrigger` en Azure Functions (`dotnet-isolated`)
- Procesamiento: normaliza texto (trim + uppercase), cuenta lineas y calcula SHA-256
- Publicacion: envia un mensaje JSON al topico de Service Bus configurado

## Estructura

- `src/BlobProcessor.Function/Functions/BlobIngestionFunction.cs`: trigger y orquestacion
- `src/BlobProcessor.Function/Services/FileProcessor.cs`: procesamiento de contenido
- `src/BlobProcessor.Function/Services/ServiceBusTopicPublisher.cs`: publicacion en topico

## Configuracion

### Variables requeridas

En `local.settings.json` (desarrollo) y en Application Settings (Azure):

- `AzureWebJobsStorage`: cuenta de almacenamiento para runtime
- `BlobStorageConnection`: conexion al storage donde llegan blobs
- `BlobContainerName`: contenedor a monitorear
- `ServiceBusConnection`: conexion al namespace de Service Bus
- `ServiceBus:TopicName`: nombre del topico destino

## Ejecutar localmente

Prerequisitos:

- .NET SDK 8
- Azure Functions Core Tools v4
- Azurite (opcional para emulacion local de Blob)

Comandos:

```bash
cd src/BlobProcessor.Function
dotnet restore
dotnet build
func start
```

## Despliegue sugerido en Azure

1. Crear una Function App (.NET isolated, runtime v4).
2. Configurar las Application Settings listadas arriba.
3. Dar permisos a la identidad de la Function App sobre Service Bus (si usas identidad administrada) o configurar `ServiceBusConnection` con SAS.
4. Publicar con CI/CD o `func azure functionapp publish`.

## Contrato del mensaje publicado

Ejemplo de payload enviado al topico:

```json
{
  "blobName": "archivo1.txt",
  "processedAtUtc": "2026-05-28T18:30:00.0000000Z",
  "contentLength": 1024,
  "lineCount": 35,
  "sha256": "A1B2C3...",
  "processedContent": "CONTENIDO PROCESADO"
}
```

Propiedades adicionales del mensaje Service Bus:

- `Subject = blob.processed`
- `ApplicationProperties["blobName"]`
- `ApplicationProperties["processedAtUtc"]`
