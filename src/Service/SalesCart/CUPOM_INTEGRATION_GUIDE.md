# 📊 Integração de Validação de Cupom com Desconto em SalesCartUserUseCase

## 🎯 Objetivo
Implementar a validação de cupom no caso de uso `PutSalesCartUserAsync` e aplicar desconto automático no `PriceTotal` baseado na porcentagem de desconto do cupom.

## 🏗️ Arquitetura da Solução

```
SalesCartUserInput
	↓
	├─ IdUser
	├─ PriceTotal (preço original)
	├─ Items (itens do carrinho)
	└─ CuponName (nome do cupom a validar)

	↓ [PutSalesCartUserAsync]

	├─ Validação: IsValidCuponNameAsync(CuponName)
	│   ├─ Busca em Redis Cache
	│   └─ Compara com cupons (case-insensitive)
	│
	├─ Se válido: GetMatchingCuponAsync(CuponName)
	│   ├─ Obtém a entidade Cupon completa
	│   └─ Extrai PercentDesc (porcentagem de desconto)
	│
	├─ Cálculo: finalPrice = PriceTotal - (PriceTotal * (PercentDesc / 100))
	│
	└─ Atualiza SalesCartUser com o PriceTotal com desconto
```

## 📝 Modificações Realizadas

### 1. **SalesCartUser.cs** (Entidade)
```csharp
public class SalesCartUser
{
	public int IdUser { get; set; }
	public decimal PriceTotal { get; set; }
	public List<SalesCartItem> Items { get; set; } = new List<SalesCartItem>();
	public string CuponName { get; set; } = string.Empty;
}
```
- ✅ Propriedade `CuponName` já existe para armazenar o cupom

### 2. **SalesCartUserInput.cs** (Input de Dados)
```csharp
public class SalesCartUserInput: SalesCartUser
{
	// Herda todas as propriedades de SalesCartUser
	// Incluindo CuponName para receber o cupom do cliente
}
```

### 3. **SalesCartUserUseCase.cs** (Lógica de Negócio)

**Injeção de Dependência:**
```csharp
private readonly ICuponValidationService _cuponValidationService;

public SalesCartUserUseCase(
	ISalesCartUserRepository salesCartUserRepository,
	ICuponValidationService cuponValidationService,
	ILogger<SalesCartUserUseCase> logger
)
```

**Lógica em PutSalesCartUserAsync:**
```csharp
// Inicializar preço total com o valor fornecido
decimal finalPrice = input.PriceTotal;

// Validar e aplicar cupom se fornecido
if (!string.IsNullOrWhiteSpace(input.CuponName))
{
	_logger.LogInformation($"Validando cupom: {input.CuponName}");

	// 1. Validar cupom
	bool isCuponValid = await _cuponValidationService.IsValidCuponNameAsync(input.CuponName);

	if (isCuponValid)
	{
		// 2. Obter cupom com desconto
		var matchingCupon = await _cuponValidationService.GetMatchingCuponAsync(input.CuponName);

		if (matchingCupon != null && matchingCupon.PercentDesc > 0)
		{
			// 3. Calcular e aplicar desconto
			decimal discountAmount = finalPrice * (matchingCupon.PercentDesc / 100);
			finalPrice = finalPrice - discountAmount;

			_logger.LogInformation($"Cupom '{input.CuponName}' aplicado com sucesso. " +
				$"Desconto: {matchingCupon.PercentDesc}%. " +
				$"Preço original: {input.PriceTotal}, " +
				$"Preço com desconto: {finalPrice}");
		}
	}
	else
	{
		_logger.LogWarning($"Cupom '{input.CuponName}' não é válido ou não foi encontrado.");
	}
}

// 4. Salvar com o preço final (com ou sem desconto)
await _salesCartUserRepository.PutSalesCartUserAsync(new SalesCartUser
{
	IdUser = input.IdUser,
	PriceTotal = finalPrice,  // Preço com desconto aplicado
	Items = input.Items,
	CuponName = input.CuponName
});
```

## 🔄 Fluxo Passo a Passo

1. **Cliente envia Request:**
   ```json
   {
	 "idUser": 1,
	 "priceTotal": 100.00,
	 "cuponName": "BLACK_FRIDAY",
	 "items": [...]
   }
   ```

2. **Validação do Cupom:**
   - `IsValidCuponNameAsync("BLACK_FRIDAY")` retorna `true`
   - Se estiver em cache Redis, usa o cache
   - Caso contrário, busca do banco e coloca em cache

3. **Obtenção do Cupom:**
   - `GetMatchingCuponAsync("BLACK_FRIDAY")`
   - Retorna: `{ Name: "BLACK_FRIDAY", PercentDesc: 15.5, ... }`

4. **Cálculo do Desconto:**
   - Desconto = 100.00 × (15.5 / 100) = 15.50
   - Preço final = 100.00 - 15.50 = 84.50

5. **Salvamento:**
   - `PriceTotal` = 84.50 (com desconto)
   - `CuponName` = "BLACK_FRIDAY"

6. **Resposta ao Cliente:**
   ```json
   {
	 "result": true,
	 "message": "Carrinho de compras atualizado com sucesso!",
	 "exception": null
   }
   ```

## 📊 Exemplo de Cálculo

| Campo | Valor |
|-------|-------|
| Preço Original | R$ 1.000,00 |
| Percentual de Desconto (PercentDesc) | 20% |
| Valor do Desconto | R$ 200,00 |
| **Preço Final** | **R$ 800,00** |

## ⚠️ Casos de Tratamento

### ✅ Cupom Válido
- Cupom existe no cache Redis
- Nome corresponde (case-insensitive)
- Desconto é aplicado automaticamente

### ❌ Cupom Inválido
- Cupom não encontrado
- Nome não corresponde
- Preço final mantém o valor original (sem desconto)

### ❌ CuponName vazio
- Se não fornecido, ignora validação
- Preço final = Preço original
- Funciona normalmente sem cupom

## 🔐 Segurança & Performance

✅ **Cache Redis:**
- Cupons em cache por 1 hora (TTL)
- Reduz consultas ao banco de dados
- Comparação case-insensitive protege contra erros

✅ **Validação:**
- Verifica se PercentDesc > 0 antes de aplicar
- Trata valores nulos corretamente
- Logging completo de todas as operações

✅ **Tratamento de Erros:**
- Try-catch envolvendo toda a lógica
- Se erro: retorna `Result = false` com mensagem de erro
- Não interrompe o fluxo

## 📦 Dependências Injetadas

```csharp
ISalesCartUserRepository     // Persistência
ICuponValidationService      // Validação de cupom
ILogger<SalesCartUserUseCase> // Logging
```

## ✅ Status de Implementação

- ✅ Entidade Cupon criada
- ✅ CuponRepository com CRUD + Cache Redis
- ✅ CuponValidationService com validação
- ✅ SalesCartUserUseCase integrado
- ✅ Aplicação de desconto automática
- ✅ Logging detalhado
- ✅ Tratamento de erros
- ✅ Build bem-sucedido

## 🚀 Próximos Passos (Opcionais)

1. Adicionar endpoint da API para expor o caso de uso
2. Adicionar testes unitários
3. Adicionar validação de datas (data_start e data_end do cupom)
4. Adicionar validação de usage_limit e usage_quantity
5. Implementar invalidação de cupom após atingir limite de uso
