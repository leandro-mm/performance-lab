# Dashboard de Performance com .NET

## Contexto
Projeto idealizado para analisar e comparar o impacto de diferentes abordagens de codificação no desempenho de aplicações. 
- Visualizar métricas de performance em tempo real através de um dashboard interativo;
- Comparar métodos otimizados vs não otimizados com resultados mensuráveis;
- Coletar e analisar dados de Garbage Collector (GC) e uso de memória;
---  
## Objetivo do Projeto 
### 1. Dashboard em Tempo Real
- Exibe métricas atualizadas a cada 2 segundos via SignalR;
- Mostra uso de memória, CPU, coleções de GC, contagem de threads;
- Interface intuitiva com Blazor Server

### 2. Benchmarking Integrado
- Testes de performance usando BenchmarkDotNet;
- Comparação direta entre métodos "ruins" e "bons";
- Métricas detalhadas de alocação de memória

### 3. Análise de Métricas
- Coleta automática de métricas do processo;
- Visualização de padrões de alocação e coleta de lixo;
- Identificação de gargalos de performance;

### 4. Simulação de Problemas
- Vazamentos de memória controlados;
- Alocações excessivas e boxing;
- Operações ineficientes com strings
---  

## Arquitetura
<img width="626" height="158" alt="image" src="https://github.com/user-attachments/assets/434ed5a5-ea9e-4b7a-911a-efec0ac1d717" />

---  
## Benefícios Demonstrados
- Economia de Memória: Até 90% menos alocações com StringBuilder;
- Redução de GC: Menos coleções = melhor performance;
- Boxing Elimination: Operações matemáticas 3x mais rápidas;
- Visualização em Tempo Real: Identificação imediata de problemas

---  
## O que é o Garbage Collector (GC)?
O Garbage Collector é o gerenciador automático de memória do .NET. Ele faz parte do CLR (Common Language Runtime) e sua função é liberar automaticamente a memória ocupada por objetos que não estão mais sendo utilizados pela aplicação, eliminando a necessidade de o desenvolvedor gerenciar manualmente a alocação e desalocação de memória

### Funcionamento (de forma simplificada)
- Alocação: Quando um objeto é criado com `new` o .NET aloca memória para ele na Managed Heap;
- Rastreamento: O GC mantém um grafo de referências para saber quais objetos estão "vivos" (acessíveis a partir das raízes da aplicação: variáveis locais, estáticas, registradores, etc.);
#### Coleta
- Periodicamente, o GC executa uma coleta;
- Marca (Mark): Identifica todos os objetos vivos;
- Varredura (Sweep): Reclama a memória dos objetos não marcados (mortos);
- Compactação (Compact): Opcionalmente, reorganiza os objetos vivos para reduzir a fragmentação e otimizar alocações futuras;

#### Gerações (Generations) – Otimização por idade
O GC divide a heap em 3 gerações para otimizar o desempenho, baseado no princípio de que objetos novos morrem rápido e objetos velhos tendem a viver mais
| Geração | Descrição| Frequência de Coleta|
| -------- | -------- | -------- |
| Gen 0   | Objetos recém-criados (ex: variáveis locais temporárias).     | Coletada com muita frequência (mais rápida).     |
| Gen 1    | Objetos que sobreviveram a uma coleta da Gen 0.     | Coletada com menos frequência.     |
| Gen 2    | Objetos de longa duração (ex: singletons, caches, pools).    | Coletada raramente (mais lenta e pesada).     |

#### Server Garbage Collector x Workstation Garbage Collector

| Característica  | Workstation GC  | Server GC  |
| -------- | -------- | -------- |
| Objetivo   | Baixa latência e responsividade (UI)    | Alto throughput e escalabilidade    |
| Cenário   | Aplicações Desktop, Console, UI    | APIs Web, Microsserviços, Backends    |
|  Modelo de Threads  |  Uma thread de coleta (a thread do usuário)   |  Uma thread de coleta por núcleo lógico da CPU   |
|  Heap (Memória)  |  Uma única heap para toda a aplicação   |   Uma heap por núcleo lógico da CPU  |
|   Performance |   Pausas curtas, mas coletas mais frequentes  |  Pausas potencialmente mais longas, mas mais rápidas e eficientes   |
| Uso de Recursos   |  Menos agressivo   |  Mais agressivo e intensivo   |
|  Padrão .NET |  Padrão para aplicações autônomas (exe)   |  Padrão para aplicações ASP.NET Core   |

---  

### Pro Tip
- Para liberar recursos não gerenciados (arquivos, conexões de banco, handles de SO), implemente IDisposable e use using.
- Evite criar muitos objetos de curta duração em loops críticos (isso sobrecarrega a Gen 0).
- Use GC.Collect() com moderação – forçar uma coleta manual geralmente prejudica a performance e só é útil em cenários muito específicos (ex: após testes de benchmark).- 

## **Executando o projeto**

- **Pré-requisitos:**
	- .NET SDK 8.0 ou superior (necessário para `TargetFramework` `net8.0`) — instale a partir de https://dotnet.microsoft.com/download
	- Git (para clonar o repositório)
	- Um navegador moderno (Chrome, Edge, Firefox)
	- Editor/IDE opcional: Visual Studio 2022/2023, Visual Studio Code com a extensão C# (recomendado)
	- Observação: não é necessário instalar Node.js para este projeto — o `Chart.js` é carregado via CDN.

- **Restaurar dependências (nuget):**

```bash
dotnet restore
```

- **Compilar o projeto:**

```bash
dotnet build --configuration Debug
```

- **Executar apenas o projeto web `PerformanceLab.Web`:**

```bash
dotnet run --project PerformanceLab.Web/PerformanceLab.Web.csproj
```

Isso iniciará o servidor Kestrel com as URLs padrão (HTTP e HTTPS). Para definir URLs explícitas:

```bash
dotnet run --project PerformanceLab.Web/PerformanceLab.Web.csproj --urls "http://localhost:5000;https://localhost:5001"
```

- **Executar com hot-reload (opcional):**

```bash
dotnet watch run --project PerformanceLab.Web/PerformanceLab.Web.csproj
```

- **Executar pela IDE:**
	- Abra `PerformanceLab.slnx` no Visual Studio ou abra a pasta no VS Code.
	- Defina `PerformanceLab.Web` como projeto de inicialização e execute (F5 ou `Run`).

- **Notas adicionais:**
	- O projeto usa SignalR para métricas em tempo real e carrega `Chart.js` via CDN; assegure acesso à internet para o CDN ou substitua por uma cópia local em `wwwroot` se necessário.
	- Se ocorrerem erros de restauração, verifique sua versão do .NET SDK com `dotnet --info` e atualize conforme necessário.

