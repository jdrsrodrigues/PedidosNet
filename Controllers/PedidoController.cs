using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidosNet.Data;
using PedidosNet.Domain.Criacao;
using PedidosNet.Domain.Descontos;
using PedidosNet.DTOs;
using PedidosNet.Models;

namespace PedidosNet.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPedidoFactory _pedidoFactory;
    private readonly SeletorDescontos _seletorDescontos;

    public PedidoController(
        AppDbContext context, 
        IPedidoFactory pedidoFactory, 
        SeletorDescontos seletorDescontos
    )
    {
        _context = context;
        _pedidoFactory = pedidoFactory;
        _seletorDescontos = seletorDescontos;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .AsNoTracking()
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

        return pedido is null ? NotFound() : Ok(pedido);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CreatePedidoRequest request)
    {
        var cliente = await _context.Clientes.FindAsync(request.ClienteId);
        if (cliente is null)
            return BadRequest("Cliente não encontrado.");

        //Regra 1: cliente bloqueado nem chega a criar pedido
        if (cliente.Bloqueado)
            return BadRequest("Cliente bloqueado não pode criar pedidos.");

        var itens = new List<ItemPedido>();

        foreach (var itemRequest in request.Itens)
        {
            var produto = await _context.Produtos.FindAsync(itemRequest.ProdutoId);
            if (produto is null)
                return BadRequest($"Produto {itemRequest.ProdutoId} não encontrado.");

            itens.Add(new ItemPedido
            {
                Id = Guid.NewGuid(),
                ProdutoId = produto.Id,
                NomeProduto = produto.Nome,
                Preco = produto.Preco,
                Quantidade = itemRequest.Quantidade
            });
        }

        //A decisão de criação e status inicial foi para a factory
        var pedido = _pedidoFactory.Criar(cliente, itens);

        return await FinalizarPedido(pedido);
    }

    [HttpPost("kits")]
    public async Task<IActionResult> CriarKit(CreatePedidoKitRequest request)
    {
        var cliente = await _context.Clientes.FindAsync(request.ClienteId);
        if (cliente is null)
            return BadRequest("Cliente não encontrado.");

        //Regra 1: cliente bloqueado nem chega a criar pedido
        if (cliente.Bloqueado)
            return BadRequest("Cliente bloqueado não pode criar pedidos.");

        var kitBuilder = new KitPromocionalBuilder((PedidoFactory)_pedidoFactory)
            .ParaCliente(cliente);

        foreach (var itemRequest in request.Itens)
        {
            var produto = await _context.Produtos.FindAsync(itemRequest.ProdutoId);

            if (produto is null)
                return BadRequest($"Produto {itemRequest.ProdutoId} não encontrado.");

            try
            {
                kitBuilder.AddProduto(produto, itemRequest.Quantidade);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        if (!string.IsNullOrEmpty(request.CupomDesconto))
        {
            try
            {
                kitBuilder.AplicarCupom(request.CupomDesconto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Pedidos de kit promocional são criados via builder, que encapsula a lógica de criação e validação do kit.
        var pedido = kitBuilder.Build();

        //Aplicando descontos
        //A aplicação de descontos foi movida para o domain, usando o padrão Strategy.
        return await FinalizarPedido(pedido);
    }

    private async Task<IActionResult> FinalizarPedido(Pedido pedido)
    {
        var estrategiasDesconto = _seletorDescontos.Selecionar(pedido);
        pedido.AplicarDesconto(estrategiasDesconto);
        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return Ok(pedido.Id);
    }
}
