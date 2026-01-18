# AudioUploadApi

API para upload e gerenciamento de arquivos de áudio usando ASP.NET Core, Entity Framework e Azure Blob Storage com compressão automática para AAC.

## Tecnologias

- **ASP.NET Core 9.0** - Framework web
- **Entity Framework Core** - ORM para banco de dados
- **SQL Server** - Banco de dados
- **Azure Blob Storage** - Armazenamento de arquivos
- **FFmpeg** - Compressão de áudio para AAC
- **Docker** - Containerização dos serviços

## Funcionalidades

- Upload de arquivos de áudio
- Compressão automática para formato AAC (128kbps)
- Armazenamento em Azure Blob Storage
- Logs de tempo de compressão
- Listagem de arquivos enviados

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
│   ├── AzureBlobStorageService.cs # Implementação Azure Blob
│   ├── IAudioCompressionService.cs # Interface de compressão
│   └── FFmpegAudioCompressionService.cs # Implementação FFmpeg
├── AudioUploadApi.Tests/
│   └── AudioCompressionServiceTests.cs # Testes de compressão
├── docker-compose.yml           # Configuração Docker
└── Program.cs                   # Configuração da aplicação
```

## Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [FFmpeg](https://ffmpeg.org/download.html) - Instalado no sistema

### Instalação do FFmpeg

**Windows:**
```bash
winget install FFmpeg
```

**Linux:**
```bash
sudo apt update
sudo apt install ffmpeg
```

**macOS:**
```bash
brew install ffmpeg
```

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

## Testes

Para executar os testes de compressão de áudio:

```bash
dotnet test
```

Os testes verificam:
- Se o áudio está sendo comprimido corretamente
- Se os logs de tempo estão sendo registrados
- Se o arquivo comprimido é menor que o original

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
- `POST /api/audio/upload` - Upload de arquivo de áudio (comprime automaticamente para AAC)
- `GET /api/audio` - Listar todos os arquivos de áudio

### Documentação
- Swagger UI disponível em `/swagger` para documentação e teste dos endpoints

## Processo de Upload

1. Cliente envia arquivo de áudio (qualquer formato suportado pelo FFmpeg)
2. API comprime o áudio para AAC (128kbps)
3. Logs registram o tempo de compressão
4. Arquivo comprimido é salvo no Azure Blob Storage
5. Metadados são salvos no banco de dados SQL Server
6. API retorna informações do arquivo processado

## Configuração

As configurações estão no `appsettings.json`:
- Connection string do SQL Server
- Connection string do Azure Storage (Azurite)

## Parar os Serviços

```bash
docker-compose down
```