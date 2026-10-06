using PedidosNet.Domain.Entities;

namespace PedidosNet.Domain.Factories;

public interface IPedidoFactory
{
    Pedido Criar(Cliente cliente, IReadOnlyCollection<ItemPedido> itens);
}