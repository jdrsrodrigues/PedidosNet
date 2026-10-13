using PedidosNet.Models;

namespace PedidosNet.Domain.Criacao;
public interface IPedidoFactory
{
    Pedido Criar(Cliente cliente, IReadOnlyCollection<ItemPedido> itens);
}