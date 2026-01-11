# AudioUploadApi

API para upload e gerenciamento de arquivos de áudio usando ASP.NET Core, Entity Framework e Azure Blob Storage.

## Tecnologias

- **ASP.NET Core 9.0** - Framework web
- **Entity Framework Core** - ORM para banco de dados
- **SQL Server** - Banco de dados
- **Azure Blob Storage** - Armazenamento de arquivos
- **Docker** - Containerização dos serviços

## Estrutura do Projeto

```
AudioUploadApi/
├── Controllers/
│   └── AudioController.cs       # Controller para upload de áudio
├── Data/
│   └── AppDbContext.cs          # Contexto do Entity Framework
├── Entities/
│   └── AudioFile.cs             # Entidade do arquivo de áudio
├── Services/
│   ├── IAudioStorageService.cs  # Interface do serviço de storage
│   └── AzureBlobStorageService.cs # Implementação Azure Blob
├── docker-compose.yml           # Configuração Docker
└── Program.cs                   # Configuração da aplicação
```

## Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Como Executar

### 1. Clonar o repositório
```bash
git clone <url-do-repositorio>
cd AudioUploadApi
```

### 2. Iniciar os serviços Docker
```bash
docker-compose up -d
```

### 3. Restaurar pacotes NuGet
```bash
dotnet restore
```

### 4. Criar e aplicar migrações do banco
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Executar a aplicação
```bash
dotnet run
```

A API estará disponível em:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger**: https://localhost:5001/swagger

## Serviços Docker

O `docker-compose.yml` configura:

- **SQL Server** (porta 1433)
  - Usuário: `sa`
  - Senha: `YourStrong@Passw0rd`
  
- **Azurite** (emulador Azure Storage)
  - Blob Service: porta 10000
  - Queue Service: porta 10001
  - Table Service: porta 10002

## Endpoints

### Audio Controller
- `POST /api/audio/upload` - Upload de arquivo de áudio
- `GET /api/audio` - Listar todos os arquivos de áudio

### Documentação
- Swagger UI disponível em `/swagger` para documentação e teste dos endpoints

## Configuração

As configurações estão no `appsettings.json`:
- Connection string do SQL Server
- Connection string do Azure Storage (Azurite)

## Parar os Serviços

```bash
docker-compose down
```