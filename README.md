# Dashboard de Performance com .NET

## Objetivo do Projeto
Este projeto foi desenvolvido para demonstrar, analisar e comparar o impacto de diferentes abordagens de codificação no desempenho de aplicações. Ele serve como um ambiente controlado para:
- Visualizar métricas de performance em tempo real através de um dashboard interativo;
- Comparar métodos otimizados vs não otimizados com resultados mensuráveis;
- Coletar e analisar dados de GC (Garbage Collector) e uso de memória;

## O que o Projeto Faz
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

## Arquitetura
<img width="626" height="158" alt="image" src="https://github.com/user-attachments/assets/434ed5a5-ea9e-4b7a-911a-efec0ac1d717" />

## Benefícios Demonstrados
- Economia de Memória: Até 90% menos alocações com StringBuilder;
- Redução de GC: Menos coleções = melhor performance;
- Boxing Elimination: Operações matemáticas 3x mais rápidas;
- Visualização em Tempo Real: Identificação imediata de problemas
