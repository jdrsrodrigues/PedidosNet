using PedidosNet.Models;

namespace PedidosNet.Domain.Criacao;

public class KitPromocionalBuilder
{
    private readonly PedidoFactory _pedidoFactory;

    private Cliente? _cliente;
    private readonly List<ItemPedido> _itens = new();
    private decimal _pesoAcumulado;
    private decimal _valorAcumulado;
    private string? _cupomDesconto;

    private const decimal PesoMaximoKit = 5.0m;
    private const decimal ValorMinimoParaCupom = 300m;
    private const int MinimoProdutosDistintos = 2;
    private const decimal PercentualCupom = 0.10m;

    public KitPromocionalBuilder(PedidoFactory pedidoFactory)
    {
        _pedidoFactory = pedidoFactory;
    }

    public KitPromocionalBuilder ParaCliente(Cliente cliente)
    {
        _cliente = cliente;
        return this;
    }

    public KitPromocionalBuilder AddProduto(Produto produto, int quantidade)
    {
        var pesoItem = produto.Peso * quantidade;
        var pesoResultante = _pesoAcumulado + pesoItem;

        if (pesoResultante > PesoMaximoKit)
            throw new InvalidOperationException(
                $"Adicionar '{produto.Nome}' excederia o peso máximo do kit promocional ({PesoMaximoKit} kg)." +
                $" Peso atual: {_pesoAcumulado} kg, peso do item: {pesoItem} kg, peso resultante: {pesoResultante} kg."
            );

        _itens.Add(new ItemPedido
        {
            Id = Guid.NewGuid(),
            ProdutoId = produto.Id,
            NomeProduto = produto.Nome,
            Preco = produto.Preco,
            Quantidade = quantidade,
            Peso = produto.Peso
        });

        _pesoAcumulado = pesoResultante;
        _valorAcumulado += produto.Preco * quantidade;

        return this;
    }

    public KitPromocionalBuilder AplicarCupom(string cupomDesconto)
    {
        if (_valorAcumulado < ValorMinimoParaCupom)
            throw new InvalidOperationException(
                $"Não é possível aplicar o cupom '{cupomDesconto}'. O valor acumulado do kit promocional ({_valorAcumulado:C}) " +
                $"não atinge o mínimo necessário para aplicar o cupom ({ValorMinimoParaCupom:C})."
            );
        
        _cupomDesconto = cupomDesconto;

        return this;
    }
    public Pedido Build()
    {
        if (_cliente is null)
            throw new InvalidOperationException("Cliente não definido para o kit promocional.");

        if (_itens.Select(i => i.ProdutoId).Distinct().Count() < MinimoProdutosDistintos)
            throw new InvalidOperationException($"O kit promocional deve conter pelo menos {MinimoProdutosDistintos} produtos distintos. ");

        var pedido = _pedidoFactory.Criar(_cliente, _itens);

        if (!string.IsNullOrEmpty(_cupomDesconto))
            pedido.CupomDesconto = _cupomDesconto;

        return pedido;
    }
}