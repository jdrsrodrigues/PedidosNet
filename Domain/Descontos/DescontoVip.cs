using PedidosNet.Models;

namespace PedidosNet.Domain.Descontos;

public class DescontoVip : IDescontoStrategy
{
    public bool AplicarPara(Pedido pedido)
        => pedido.Cliente is { Vip: true };

    public decimal CalcularDesconto(Pedido pedido)
        => pedido.Total * 0.10m;
}