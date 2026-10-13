using System.Runtime.InteropServices.Marshalling;
using PedidosNet.Domain.Descontos;

namespace PedidosNet.Models;

public class Pedido
{
    public Guid Id { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public string EnderecoEntrega { get; set; } = string.Empty;
    public string CupomDesconto { get; set; } = string.Empty;

    // ⚠️ Status como string solta — "magic string",
    // sem enum, sem State.
    // Isso é o gatilho da dor que
    // resolvemos no Módulo 4 (State).
    public string Status { get; set; } = string.Empty;

    public decimal Total { get; set; }
    public DateTime DataCriacao { get; set; }

    public List<ItemPedido> Itens { get; set; } = new();

    public void CalcularTotal()
    {
        Total = Itens.Sum(i => i.Preco * i.Quantidade);
    }

    public void AplicarDesconto(IEnumerable<IDescontoStrategy> strategies)
    {
        foreach (var strategy in strategies)
        {
            var desconto = strategy.CalcularDesconto(this);
            Total -= desconto;
        }
    }
}
