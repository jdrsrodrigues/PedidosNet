using PedidosNet.Models;

namespace PedidosNet.Domain.Descontos;

public class DescontoSemanaDosPais : IDescontoStrategy
{
    private readonly DateTime _dataInicio = new DateTime(DateTime.Now.Year, 8, 3);
    private readonly DateTime _dataFim = new DateTime(DateTime.Now.Year, 8, 7);
    private const decimal ValorMinimo = 100m;
    public bool AplicarPara(Pedido pedido)
    {
        var data = pedido.DataCriacao;
        var isSemanaDosPais = data >= _dataInicio && data <= _dataFim;
        return isSemanaDosPais && pedido.Total > ValorMinimo;
    }

    public decimal CalcularDesconto(Pedido pedido)
        => pedido.Total * 0.20m;

    public bool Exclusiva => true;
}