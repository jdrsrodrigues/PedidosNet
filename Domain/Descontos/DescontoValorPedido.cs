using PedidosNet.Models;

namespace PedidosNet.Domain.Descontos;

public class DescontoValorPedido : IDescontoStrategy
{
    public bool AplicarPara(Pedido pedido)
        => pedido.Total >= 2000m;

    public decimal CalcularDesconto(Pedido pedido)
        => pedido.Total * 0.05m;
    
}