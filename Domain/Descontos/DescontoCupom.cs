using PedidosNet.Models;

namespace PedidosNet.Domain.Descontos;

public class DescontoCupom : IDescontoStrategy
{
    public bool AplicarPara(Pedido pedido)
        => pedido.CupomDesconto is not null;
    public decimal CalcularDesconto(Pedido pedido)
        => pedido.Total * 0.10m;
}