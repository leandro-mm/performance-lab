# Dashboard de Performance com .NET

## Contexto
Analisar e comparar o impacto de diferentes abordagens de codificação no desempenho de aplicações é uma tarefa extremanemte importante em engenharia de software. Para tanto, foi desenvolvido um projeto utilizando DotNet8 com um ambiente controlado para:
- Visualizar métricas de performance em tempo real através de um dashboard interativo;
- Comparar métodos otimizados vs não otimizados com resultados mensuráveis;
- Coletar e analisar dados de GC (Garbage Collector) e uso de memória;
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
