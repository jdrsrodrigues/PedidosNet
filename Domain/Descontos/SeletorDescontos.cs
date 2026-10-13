using PedidosNet.Models;

namespace PedidosNet.Domain.Descontos;

public class SeletorDescontos
{
    private readonly IReadOnlyList<IDescontoStrategy> _strategies;

    public SeletorDescontos(IEnumerable<IDescontoStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }
    public IEnumerable<IDescontoStrategy> Selecionar(Pedido pedido)
    {
        var aplicaveis = _strategies.Where(s => s.AplicarPara(pedido)).ToList();
        
        if (aplicaveis.Any(s => s.Exclusiva))
            return aplicaveis.Where(s => s.Exclusiva);

        return aplicaveis;
    }
}